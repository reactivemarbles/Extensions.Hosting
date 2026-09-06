<#
.SYNOPSIS
Fails when packaged source modules are missing from Cobertura coverage or are below 100%.

.DESCRIPTION
The script discovers packable projects from evaluated MSBuild metadata, resolves the
build output for the selected coverage target framework, then audits Cobertura reports.
Coverage is tracked by assembly and source path so projects that link shared source,
such as the .Reactive packages, are verified as separate package modules.

Line hits are aggregated across reports. Branch coverage is intentionally conservative:
Cobertura does not expose stable branch identities, so a branch line must reach full
covered/total coverage in at least one report instead of passing by summing partial
counts from unrelated runs.

When the built assembly contains portable PDB information, the script also compares
reported Cobertura source files with executable sequence-point source files. This
prevents a package module from passing when only a small covered file is reported and
another executable source file from the same module is omitted.
#>
[CmdletBinding()]
param(
    [Parameter()]
    [string] $SourceRoot = (Join-Path $PSScriptRoot '..\src'),

    [Parameter()]
    [Alias('CoverageDirectory')]
    [string] $CoveragePath = (Join-Path $PSScriptRoot '..\artifacts\coverage\final'),

    [Parameter()]
    [string] $ReportPattern = '*.cobertura.xml',

    [Parameter()]
    [string] $PreferredTargetFramework = 'net10.0',

    [Parameter()]
    [string] $Configuration = 'Release'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Add-Type -AssemblyName System.Reflection.Metadata
Add-Type -AssemblyName System.Collections.Immutable

function Resolve-FullPath
{
    param([Parameter(Mandatory = $true)][string] $Path)

    return [System.IO.Path]::GetFullPath($Path)
}

function Get-XmlAttribute
{
    param(
        [Parameter(Mandatory = $true)][System.Xml.XmlNode] $Node,
        [Parameter(Mandatory = $true)][string] $Name
    )

    $attribute = $Node.Attributes.GetNamedItem($Name)
    if ($null -eq $attribute)
    {
        return $null
    }

    return $attribute.Value
}

function Get-MsBuildJson
{
    param(
        [Parameter(Mandatory = $true)][string] $ProjectPath,
        [Parameter()][string] $TargetFramework,
        [Parameter()][string] $BuildConfiguration
    )

    $arguments = @(
        'msbuild',
        $ProjectPath,
        '-getProperty:PackageId',
        '-getProperty:IsPackable',
        '-getProperty:IsTestProject',
        '-getProperty:AssemblyName',
        '-getProperty:TargetPath',
        '-getProperty:TargetFrameworks',
        '-getProperty:TargetFramework',
        '-getItem:Compile',
        '-nologo'
    )

    if (-not [string]::IsNullOrWhiteSpace($TargetFramework))
    {
        $arguments += "-p:TargetFramework=$TargetFramework"
    }

    if (-not [string]::IsNullOrWhiteSpace($BuildConfiguration))
    {
        $arguments += "-p:Configuration=$BuildConfiguration"
    }

    $output = & dotnet @arguments 2>&1
    if ($LASTEXITCODE -ne 0)
    {
        $message = ($output | Out-String).Trim()
        throw "MSBuild evaluation failed for $ProjectPath. $message"
    }

    $text = ($output | Out-String).Trim()
    $jsonStart = $text.IndexOf('{')
    if ($jsonStart -lt 0)
    {
        throw "MSBuild evaluation for $ProjectPath did not return JSON."
    }

    return $text.Substring($jsonStart) | ConvertFrom-Json
}

function Get-PortablePdbReaderProvider
{
    param(
        [Parameter(Mandatory = $true)][System.Reflection.PortableExecutable.PEReader] $PeReader,
        [Parameter(Mandatory = $true)][string] $AssemblyPath
    )

    foreach ($entry in $PeReader.ReadDebugDirectory())
    {
        if ($entry.Type -eq [System.Reflection.PortableExecutable.DebugDirectoryEntryType]::EmbeddedPortablePdb)
        {
            return $PeReader.ReadEmbeddedPortablePdbDebugDirectoryData($entry)
        }
    }

    foreach ($entry in $PeReader.ReadDebugDirectory())
    {
        if ($entry.Type -ne [System.Reflection.PortableExecutable.DebugDirectoryEntryType]::CodeView)
        {
            continue
        }

        $codeViewData = $PeReader.ReadCodeViewDebugDirectoryData($entry)
        $pdbPath = $codeViewData.Path
        if ([string]::IsNullOrWhiteSpace($pdbPath))
        {
            continue
        }

        if (-not [System.IO.Path]::IsPathRooted($pdbPath))
        {
            $pdbPath = Join-Path (Split-Path -Parent $AssemblyPath) $pdbPath
        }

        if (Test-Path -LiteralPath $pdbPath)
        {
            $pdbStream = [System.IO.File]::OpenRead($pdbPath)
            return [System.Reflection.Metadata.MetadataReaderProvider]::FromPortablePdbStream($pdbStream)
        }
    }

    return $null
}

function Test-IlContainsConditionalBranch
{
    param([Parameter(Mandatory = $true)][byte[]] $Bytes)

    for ($index = 0; $index -lt $Bytes.Length; $index++)
    {
        $opcode = $Bytes[$index]
        if (($opcode -ge 0x2c -and $opcode -le 0x37) -or
            ($opcode -ge 0x39 -and $opcode -le 0x45))
        {
            return $true
        }
    }

    return $false
}

function Get-SequencePointInventory
{
    param(
        [Parameter(Mandatory = $true)][string] $AssemblyPath,
        [Parameter(Mandatory = $true)][string] $ProjectDirectory
    )

    if (-not (Test-Path -LiteralPath $AssemblyPath))
    {
        return $null
    }

    $fileStream = $null
    $peReader = $null
    $provider = $null
    try
    {
        $fileStream = [System.IO.File]::OpenRead($AssemblyPath)
        $peReader = [System.Reflection.PortableExecutable.PEReader]::new($fileStream)
        $provider = Get-PortablePdbReaderProvider -PeReader $peReader -AssemblyPath $AssemblyPath
        if ($null -eq $provider)
        {
            return $null
        }

        $metadataReader = $provider.GetMetadataReader()
        $peMetadataReader = [System.Reflection.Metadata.PEReaderExtensions]::GetMetadataReader($peReader)
        $sourceFiles = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
        $hasBranchInstructions = $false
        foreach ($methodHandle in $peMetadataReader.MethodDefinitions)
        {
            if ($hasBranchInstructions)
            {
                break
            }

            $methodDefinition = $peMetadataReader.GetMethodDefinition($methodHandle)
            if ($methodDefinition.RelativeVirtualAddress -eq 0)
            {
                continue
            }

            $methodBody = [System.Reflection.Metadata.PEReaderExtensions]::GetMethodBody($peReader, $methodDefinition.RelativeVirtualAddress)
            $bytes = $methodBody.GetILBytes()
            if (Test-IlContainsConditionalBranch -Bytes $bytes)
            {
                $hasBranchInstructions = $true
            }
        }

        foreach ($methodHandle in $metadataReader.MethodDebugInformation)
        {
            $methodDebugInformation = $metadataReader.GetMethodDebugInformation($methodHandle)
            if ($methodDebugInformation.Document.IsNil)
            {
                continue
            }

            $hasExecutableSequencePoint = $false
            foreach ($sequencePoint in $methodDebugInformation.GetSequencePoints())
            {
                if (-not $sequencePoint.IsHidden)
                {
                    $hasExecutableSequencePoint = $true
                    break
                }
            }

            if (-not $hasExecutableSequencePoint)
            {
                continue
            }

            $document = $metadataReader.GetDocument($methodDebugInformation.Document)
            $documentName = $metadataReader.GetString($document.Name)
            if ([string]::IsNullOrWhiteSpace($documentName))
            {
                continue
            }

            $normalizedDocumentName = $documentName.Replace('/', [System.IO.Path]::DirectorySeparatorChar)
            if ([System.IO.Path]::IsPathRooted($normalizedDocumentName))
            {
                $resolvedDocumentPath = Resolve-FullPath $normalizedDocumentName
            }
            else
            {
                $resolvedDocumentPath = Resolve-FullPath (Join-Path $ProjectDirectory $normalizedDocumentName)
            }

            if ($resolvedDocumentPath -notmatch '[\\/](bin|obj)[\\/]')
            {
                [void]$sourceFiles.Add($resolvedDocumentPath)
            }
        }

        return [pscustomobject]@{
            SourceFiles = @($sourceFiles | Sort-Object)
            HasBranchInstructions = $hasBranchInstructions
        }
    }
    finally
    {
        if ($null -ne $provider)
        {
            $provider.Dispose()
        }

        if ($null -ne $peReader)
        {
            $peReader.Dispose()
        }

        if ($null -ne $fileStream)
        {
            $fileStream.Dispose()
        }
    }
}

function Select-TargetFramework
{
    param(
        [Parameter(Mandatory = $true)] $Evaluation,
        [Parameter(Mandatory = $true)][string] $Preferred
    )

    $targetFrameworks = [string]$Evaluation.Properties.TargetFrameworks
    if (-not [string]::IsNullOrWhiteSpace($targetFrameworks))
    {
        $frameworks = @($targetFrameworks.Split(';', [System.StringSplitOptions]::RemoveEmptyEntries))
        $exactMatch = $frameworks | Where-Object { $_ -eq $Preferred } | Select-Object -First 1
        if (-not [string]::IsNullOrWhiteSpace($exactMatch))
        {
            return $exactMatch
        }

        $compatibleMatch = $frameworks | Where-Object { $_.StartsWith($Preferred, [System.StringComparison]::OrdinalIgnoreCase) } | Select-Object -First 1
        if (-not [string]::IsNullOrWhiteSpace($compatibleMatch))
        {
            return $compatibleMatch
        }

        return $frameworks[0]
    }

    return [string]$Evaluation.Properties.TargetFramework
}

function Get-PackableProjects
{
    param(
        [Parameter(Mandatory = $true)][string] $Root,
        [Parameter(Mandatory = $true)][string] $PreferredTarget,
        [Parameter(Mandatory = $true)][string] $BuildConfiguration
    )

    $rootPath = Resolve-FullPath $Root
    $projects = Get-ChildItem -LiteralPath $rootPath -Recurse -Filter '*.csproj' -File

    foreach ($projectFile in $projects)
    {
        $outerEvaluation = Get-MsBuildJson -ProjectPath $projectFile.FullName -BuildConfiguration $BuildConfiguration
        $packageId = [string]$outerEvaluation.Properties.PackageId
        $isPackable = [string]$outerEvaluation.Properties.IsPackable
        $isTestProject = [string]$outerEvaluation.Properties.IsTestProject

        if ($isPackable -eq 'false' -or $isTestProject -eq 'true')
        {
            continue
        }

        $targetFramework = Select-TargetFramework -Evaluation $outerEvaluation -Preferred $PreferredTarget
        $evaluation = $outerEvaluation
        if (-not [string]::IsNullOrWhiteSpace($targetFramework))
        {
            $evaluation = Get-MsBuildJson -ProjectPath $projectFile.FullName -TargetFramework $targetFramework -BuildConfiguration $BuildConfiguration
        }

        $assemblyName = [string]$evaluation.Properties.AssemblyName
        if ([string]::IsNullOrWhiteSpace($assemblyName))
        {
            $assemblyName = $projectFile.BaseName
        }

        if ([string]::IsNullOrWhiteSpace($packageId))
        {
            $packageId = $assemblyName
        }

        $targetPath = [string]$evaluation.Properties.TargetPath
        $sequencePointInventory = $null
        if (-not [string]::IsNullOrWhiteSpace($targetPath))
        {
            $sequencePointInventory = Get-SequencePointInventory -AssemblyPath $targetPath -ProjectDirectory $projectFile.DirectoryName
        }

        $sourceFiles = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
        foreach ($compileItem in @($evaluation.Items.Compile))
        {
            $fullPath = [string]$compileItem.FullPath
            if ([string]::IsNullOrWhiteSpace($fullPath))
            {
                continue
            }

            $resolvedPath = Resolve-FullPath $fullPath
            if ($resolvedPath -match '[\\/](bin|obj)[\\/]')
            {
                continue
            }

            [void]$sourceFiles.Add($resolvedPath)
        }

        [pscustomobject]@{
            ProjectPath = Resolve-FullPath $projectFile.FullName
            AssemblyName = $assemblyName
            PackageId = $packageId
            TargetFramework = $targetFramework
            TargetPath = $targetPath
            SourceFiles = @($sourceFiles | Sort-Object)
            SequencePointSourceFiles = if ($null -eq $sequencePointInventory) { $null } else { $sequencePointInventory.SourceFiles }
            HasBranchInstructions = $null -ne $sequencePointInventory -and $sequencePointInventory.HasBranchInstructions
        }
    }
}

function Resolve-CoverageReports
{
    param(
        [Parameter(Mandatory = $true)][string] $Path,
        [Parameter(Mandatory = $true)][string] $Pattern
    )

    $resolvedPath = Resolve-FullPath $Path
    if (-not (Test-Path -LiteralPath $resolvedPath))
    {
        throw "Coverage path was not found: $resolvedPath"
    }

    $item = Get-Item -LiteralPath $resolvedPath
    if (-not $item.PSIsContainer)
    {
        return @($item)
    }

    return @(Get-ChildItem -LiteralPath $resolvedPath -Recurse -Filter $Pattern -File)
}

function Resolve-CoverageFilePath
{
    param(
        [Parameter(Mandatory = $true)][string[]] $Sources,
        [Parameter(Mandatory = $true)][string] $FileName
    )

    $normalizedFileName = $FileName.Replace('/', [System.IO.Path]::DirectorySeparatorChar)
    if ([System.IO.Path]::IsPathRooted($normalizedFileName))
    {
        return Resolve-FullPath $normalizedFileName
    }

    foreach ($source in $Sources)
    {
        $candidate = Resolve-FullPath (Join-Path $source $normalizedFileName)
        if (Test-Path -LiteralPath $candidate)
        {
            return $candidate
        }
    }

    return Resolve-FullPath $normalizedFileName
}

function New-CoverageKey
{
    param(
        [Parameter(Mandatory = $true)][string] $AssemblyName,
        [Parameter(Mandatory = $true)][string] $FilePath
    )

    return "$AssemblyName|$FilePath"
}


function ConvertTo-CanonicalCoverageClassName
{
    param(
        [Parameter(Mandatory = $true)][string] $ClassName
    )

    return [regex]::Replace(
        $ClassName,
        '(?<type>[A-Za-z_][A-Za-z0-9_.+]*)(?:<(?<parameters>[^<>]+)>|\{(?<parameters>[^{}]+)\})(?=$|/|\+)',
        {
            param($match)

            $parameters = @($match.Groups['parameters'].Value.Split(',') | ForEach-Object { $_.Trim() } | Where-Object { $_.Length -gt 0 })
            if ($parameters.Count -eq 0)
            {
                return $match.Value
            }

            return "$($match.Groups['type'].Value)``$($parameters.Count)"
        })
}

function New-CoverageLineKey
{
    param(
        [Parameter(Mandatory = $true)][string] $AssemblyName,
        [Parameter(Mandatory = $true)][string] $ClassName,
        [Parameter(Mandatory = $true)][string] $FilePath
    )

    return "$AssemblyName|$ClassName|$FilePath"
}

function Add-CoverageLine
{
    param(
        [Parameter(Mandatory = $true)] [hashtable] $CoverageByModuleFile,
        [Parameter(Mandatory = $true)] [string] $Key,
        [Parameter(Mandatory = $true)] [System.Xml.XmlNode] $Line
    )

    if (-not $CoverageByModuleFile.ContainsKey($Key))
    {
        $CoverageByModuleFile[$Key] = @{}
    }

    $number = [int](Get-XmlAttribute -Node $Line -Name 'number')
    $hits = [int](Get-XmlAttribute -Node $Line -Name 'hits')
    $branch = ([string](Get-XmlAttribute -Node $Line -Name 'branch')).Equals('true', [System.StringComparison]::OrdinalIgnoreCase)
    $conditionCoverage = [string](Get-XmlAttribute -Node $Line -Name 'condition-coverage')

    if (-not $CoverageByModuleFile[$Key].ContainsKey($number))
    {
        $CoverageByModuleFile[$Key][$number] = [pscustomobject]@{
            Hits = 0
            BranchCovered = 0
            BranchTotal = 0
        }
    }

    $entry = $CoverageByModuleFile[$Key][$number]
    $entry.Hits += $hits

    if ($branch -and $conditionCoverage -match '\((\d+)/(\d+)\)')
    {
        $covered = [int]$Matches[1]
        $total = [int]$Matches[2]
        if ($covered -gt $entry.BranchCovered)
        {
            $entry.BranchCovered = $covered
        }

        if ($total -gt $entry.BranchTotal)
        {
            $entry.BranchTotal = $total
        }
    }
}

$resolvedSourceRoot = Resolve-FullPath $SourceRoot
$coverageReports = @(Resolve-CoverageReports -Path $CoveragePath -Pattern $ReportPattern)
if ($coverageReports.Count -eq 0)
{
    throw "No Cobertura reports matching '$ReportPattern' were found under $(Resolve-FullPath $CoveragePath)"
}

$packableProjects = @(Get-PackableProjects -Root $resolvedSourceRoot -PreferredTarget $PreferredTargetFramework -BuildConfiguration $Configuration)
$expectedModules = @($packableProjects | Where-Object { $_.SourceFiles.Count -gt 0 })
$expectedModulesByAssembly = @{}
$expectedSequencePointSourcesByModule = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
$modulesWithSequencePointInventory = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)

foreach ($project in $expectedModules)
{
    if (-not $expectedModulesByAssembly.ContainsKey($project.AssemblyName))
    {
        $expectedModulesByAssembly[$project.AssemblyName] = [System.Collections.Generic.List[object]]::new()
    }

    $expectedModulesByAssembly[$project.AssemblyName].Add($project)
    if ($null -ne $project.SequencePointSourceFiles)
    {
        [void]$modulesWithSequencePointInventory.Add($project.AssemblyName)
        foreach ($sourceFile in $project.SequencePointSourceFiles)
        {
            [void]$expectedSequencePointSourcesByModule.Add((New-CoverageKey -AssemblyName $project.AssemblyName -FilePath (Resolve-FullPath $sourceFile)))
        }
    }
}

$coveredModules = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
$coveredModuleFiles = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
$coverageByClassFile = @{}

foreach ($report in $coverageReports)
{
    [xml]$coverage = Get-Content -LiteralPath $report.FullName
    $sourceNodes = @($coverage.SelectNodes('/coverage/sources/source'))
    $sources = @($sourceNodes | ForEach-Object { $_.InnerText } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
    if ($sources.Count -eq 0)
    {
        $sources = @($resolvedSourceRoot)
    }

    foreach ($package in @($coverage.SelectNodes('/coverage/packages/package')))
    {
        $assemblyName = [string](Get-XmlAttribute -Node $package -Name 'name')
        if (-not $expectedModulesByAssembly.ContainsKey($assemblyName))
        {
            continue
        }

        [void]$coveredModules.Add($assemblyName)
        foreach ($class in @($package.SelectNodes('classes/class')))
        {
            $fileName = [string](Get-XmlAttribute -Node $class -Name 'filename')
            if ([string]::IsNullOrWhiteSpace($fileName))
            {
                continue
            }

            $filePath = Resolve-CoverageFilePath -Sources $sources -FileName $fileName
            if ($filePath -match '[\\/](bin|obj)[\\/]')
            {
                continue
            }

            $coverageKey = New-CoverageKey -AssemblyName $assemblyName -FilePath $filePath
            $className = [string](Get-XmlAttribute -Node $class -Name 'name')
            if ([string]::IsNullOrWhiteSpace($className))
            {
                $className = '<unknown>'
            }

            $canonicalClassName = ConvertTo-CanonicalCoverageClassName -ClassName $className
            $coverageLineKey = New-CoverageLineKey -AssemblyName $assemblyName -ClassName $canonicalClassName -FilePath $filePath

            [void]$coveredModuleFiles.Add($coverageKey)
            foreach ($line in @($class.SelectNodes('lines/line')))
            {
                Add-CoverageLine -CoverageByModuleFile $coverageByClassFile -Key $coverageLineKey -Line $line
            }
        }
    }
}

$failures = [System.Collections.Generic.List[string]]::new()
$branchTotalsByModule = @{}

foreach ($coverageKey in $coverageByClassFile.Keys)
{
    $separatorIndex = $coverageKey.IndexOf('|')
    $assemblyName = $coverageKey.Substring(0, $separatorIndex)
    if (-not $branchTotalsByModule.ContainsKey($assemblyName))
    {
        $branchTotalsByModule[$assemblyName] = 0
    }

    foreach ($lineNumber in $coverageByClassFile[$coverageKey].Keys)
    {
        $branchTotalsByModule[$assemblyName] += $coverageByClassFile[$coverageKey][$lineNumber].BranchTotal
    }
}

foreach ($project in $expectedModules | Sort-Object AssemblyName, PackageId)
{
    if (-not $coveredModules.Contains($project.AssemblyName))
    {
        $failures.Add("Missing package module in coverage: $($project.AssemblyName) ($($project.PackageId), $($project.TargetFramework))")
        continue
    }

    if (-not $modulesWithSequencePointInventory.Contains($project.AssemblyName))
    {
        $failures.Add("Missing sequence-point inventory for package module: $($project.AssemblyName) ($($project.PackageId), $($project.TargetPath))")
        continue
    }

    $moduleBranchTotal = 0
    if ($branchTotalsByModule.ContainsKey($project.AssemblyName))
    {
        $moduleBranchTotal = $branchTotalsByModule[$project.AssemblyName]
    }

    if ($project.HasBranchInstructions -and $moduleBranchTotal -eq 0)
    {
        $failures.Add("Missing branch coverage instrumentation for package module with branch IL: $($project.AssemblyName) ($($project.PackageId))")
    }
}

foreach ($sequencePointSource in $expectedSequencePointSourcesByModule | Sort-Object)
{
    $separatorIndex = $sequencePointSource.IndexOf('|')
    $assemblyName = $sequencePointSource.Substring(0, $separatorIndex)
    if (-not $coveredModules.Contains($assemblyName))
    {
        continue
    }

    if (-not $coveredModuleFiles.Contains($sequencePointSource))
    {
        $sourceFile = $sequencePointSource.Substring($separatorIndex + 1)
        $failures.Add("Missing executable source file in coverage: $assemblyName $sourceFile")
    }
}

foreach ($coveredModuleFile in $coveredModuleFiles | Sort-Object)
{
    $separatorIndex = $coveredModuleFile.IndexOf('|')
    $assemblyName = $coveredModuleFile.Substring(0, $separatorIndex)
    if (-not $modulesWithSequencePointInventory.Contains($assemblyName))
    {
        continue
    }

    if (-not $expectedSequencePointSourcesByModule.Contains($coveredModuleFile))
    {
        $sourceFile = $coveredModuleFile.Substring($separatorIndex + 1)
        $failures.Add("Coverage source file is not present in executable sequence-point inventory: $assemblyName $sourceFile")
    }
}

foreach ($coverageKey in $coverageByClassFile.Keys | Sort-Object)
{
    $separatorIndex = $coverageKey.IndexOf('|')
    $assemblyName = $coverageKey.Substring(0, $separatorIndex)
    $secondSeparatorIndex = $coverageKey.IndexOf('|', $separatorIndex + 1)
    $className = $coverageKey.Substring($separatorIndex + 1, $secondSeparatorIndex - $separatorIndex - 1)
    $sourceFile = $coverageKey.Substring($secondSeparatorIndex + 1)

    foreach ($lineNumber in $coverageByClassFile[$coverageKey].Keys | Sort-Object)
    {
        $entry = $coverageByClassFile[$coverageKey][$lineNumber]
        if ($entry.Hits -eq 0)
        {
            $failures.Add("Uncovered line: $assemblyName $className ${sourceFile}:$lineNumber")
        }

        if ($entry.BranchTotal -gt 0 -and $entry.BranchCovered -lt $entry.BranchTotal)
        {
            $failures.Add("Partially covered branch: $assemblyName $className ${sourceFile}:$lineNumber ($($entry.BranchCovered)/$($entry.BranchTotal))")
        }
    }
}

$lineCount = 0
$branchCovered = 0
$branchTotal = 0
foreach ($coverageKey in $coverageByClassFile.Keys)
{
    foreach ($lineNumber in $coverageByClassFile[$coverageKey].Keys)
    {
        $entry = $coverageByClassFile[$coverageKey][$lineNumber]
        $lineCount++
        $branchCovered += $entry.BranchCovered
        $branchTotal += $entry.BranchTotal
    }
}

Write-Host "Reports: $($coverageReports.Count)"
Write-Host "Packable source modules: $($expectedModules.Count)"
Write-Host "Covered package source files: $($coveredModuleFiles.Count)"
Write-Host "Executable package source files: $($expectedSequencePointSourcesByModule.Count)"
Write-Host "Executable package lines: $lineCount"
Write-Host "Executable package branches: $branchCovered/$branchTotal"

if ($failures.Count -gt 0)
{
    $failures | Sort-Object | ForEach-Object { Write-Error $_ -ErrorAction Continue }
    throw "Packaged code coverage gate failed with $($failures.Count) issue(s)."
}

Write-Host "Packaged code coverage gate passed."
