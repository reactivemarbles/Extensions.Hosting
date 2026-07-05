// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Reflection;

#if REACTIVE_SHIM
namespace ReactiveMarbles.Extensions.Hosting.Reactive.Plugins.Internals;
#else
namespace ReactiveMarbles.Extensions.Hosting.Plugins.Internals;
#endif

/// <summary>Provides methods for resolving the paths of managed assemblies and unmanaged DLLs relative to a specified plugin directory.</summary>
/// <param name="pluginPath">The absolute path to the root directory containing the plugin and its dependencies. Cannot be null or empty.</param>
public sealed class AssemblyDependencyResolver(string pluginPath)
{
    /// <summary>Stores the plugin path value.</summary>
    private readonly string _pluginPath = Path.GetDirectoryName(pluginPath) ?? string.Empty;

    /// <summary>Resolves the file system path to the assembly file corresponding to the specified assembly name, if it exists in the plugin directory.</summary>
    /// <param name="assemblyName">The assembly name to resolve to a file path. Cannot be null.</param>
    /// <returns>The full path to the assembly file if it exists in the plugin directory; otherwise, null.</returns>
    /// <exception cref="ArgumentNullException">Thrown if assemblyName is null.</exception>
    public string? ResolveAssemblyToPath(AssemblyName assemblyName) =>
        assemblyName is null
            ? throw new ArgumentNullException(nameof(assemblyName))
            : ResolveExistingPath($"{assemblyName.Name}.dll");

    /// <summary>Resolves the specified unmanaged DLL name to an absolute file path on disk.</summary>
    /// <param name="unmanagedDllName">The name of the unmanaged DLL to locate. This should not include a path or file extension.</param>
    /// <returns>The absolute path to the located unmanaged DLL, or null if the DLL cannot be found.</returns>
    /// <exception cref="ArgumentException">Thrown if unmanagedDllName is null, empty, or consists only of white-space characters.</exception>
    public string? ResolveUnmanagedDllToPath(string unmanagedDllName)
    {
        var libraryName = !string.IsNullOrWhiteSpace(unmanagedDllName)
            ? unmanagedDllName
            : throw new ArgumentException("The unmanaged DLL name cannot be null, empty, or consist only of white-space characters.", nameof(unmanagedDllName));

        var fileName = Path.HasExtension(libraryName) ? libraryName : $"{libraryName}.dll";
        return ResolveExistingPath(fileName);
    }

    /// <summary>Resolves a file name relative to the plugin path when it exists.</summary>
    /// <param name="fileName">The file name to resolve.</param>
    /// <returns>The resolved path, or null when the file does not exist.</returns>
    private string? ResolveExistingPath(string fileName)
    {
        var path = Path.Combine(_pluginPath, fileName);
        return File.Exists(path) ? path : null;
    }
}
