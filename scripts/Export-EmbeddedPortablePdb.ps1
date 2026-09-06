param(
    [Parameter(Mandatory = $true)]
    [string] $Path,

    [string] $Pattern = '*.dll'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Add-Type -AssemblyName System.Reflection.Metadata

function Resolve-FullPath
{
    param([Parameter(Mandatory = $true)][string] $Value)

    return [System.IO.Path]::GetFullPath((Resolve-Path -LiteralPath $Value).Path)
}

function Export-EmbeddedPortablePdb
{
    param([Parameter(Mandatory = $true)][System.IO.FileInfo] $Assembly)

    $pdbPath = [System.IO.Path]::ChangeExtension($Assembly.FullName, '.pdb')
    if (Test-Path -LiteralPath $pdbPath)
    {
        return $false
    }

    $stream = [System.IO.File]::OpenRead($Assembly.FullName)
    try
    {
        $peReader = [System.Reflection.PortableExecutable.PEReader]::new($stream)
        try
        {
            foreach ($entry in $peReader.ReadDebugDirectory())
            {
                if ($entry.Type -ne [System.Reflection.PortableExecutable.DebugDirectoryEntryType]::EmbeddedPortablePdb)
                {
                    continue
                }

                $stream.Position = $entry.DataPointer
                $buffer = [byte[]]::new($entry.DataSize)
                $offset = 0
                while ($offset -lt $buffer.Length)
                {
                    $read = $stream.Read($buffer, $offset, $buffer.Length - $offset)
                    if ($read -eq 0)
                    {
                        throw "Unexpected end of file while reading embedded PDB from $($Assembly.FullName)."
                    }

                    $offset += $read
                }

                $magic = [System.Text.Encoding]::ASCII.GetString($buffer, 0, 4)
                if ($magic -ne 'MPDB')
                {
                    throw "Embedded PDB payload for $($Assembly.FullName) did not start with MPDB magic."
                }

                $uncompressedSize = [System.BitConverter]::ToInt32($buffer, 4)
                $compressedStream = [System.IO.MemoryStream]::new($buffer, 8, $buffer.Length - 8)
                try
                {
                    $deflateStream = [System.IO.Compression.DeflateStream]::new($compressedStream, [System.IO.Compression.CompressionMode]::Decompress)
                    try
                    {
                        $output = [System.IO.File]::Create($pdbPath)
                        try
                        {
                            $deflateStream.CopyTo($output)
                            if ($output.Length -ne $uncompressedSize)
                            {
                                throw "Embedded PDB size mismatch for $($Assembly.FullName). Expected $uncompressedSize bytes but wrote $($output.Length) bytes."
                            }
                        }
                        finally
                        {
                            $output.Dispose()
                        }
                    }
                    finally
                    {
                        $deflateStream.Dispose()
                    }
                }
                finally
                {
                    $compressedStream.Dispose()
                }

                return $true
            }
        }
        finally
        {
            $peReader.Dispose()
        }
    }
    finally
    {
        $stream.Dispose()
    }

    return $false
}

$resolvedPath = Resolve-FullPath $Path
$item = Get-Item -LiteralPath $resolvedPath
$assemblies = if ($item.PSIsContainer)
{
    @(Get-ChildItem -LiteralPath $item.FullName -Filter $Pattern -File -Recurse)
}
else
{
    @($item)
}

$exported = 0
foreach ($assembly in $assemblies)
{
    if (Export-EmbeddedPortablePdb -Assembly $assembly)
    {
        $exported++
        Write-Host "Exported $([System.IO.Path]::ChangeExtension($assembly.FullName, '.pdb'))"
    }
}

Write-Host "Embedded portable PDB files exported: $exported"
