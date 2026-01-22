// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Reflection;

namespace ReactiveMarbles.Extensions.Hosting.Plugins.Internals;

/// <summary>
/// Provides methods for resolving the paths of managed assemblies and unmanaged DLLs relative to a specified plugin
/// directory.
/// </summary>
/// <param name="pluginPath">The absolute path to the root directory containing the plugin and its dependencies. Cannot be null or empty.</param>
public class AssemblyDependencyResolver(string pluginPath)
{
    private readonly string? _pluginPath = Path.GetDirectoryName(pluginPath);

    /// <summary>
    /// Resolves the file system path to the assembly file corresponding to the specified assembly name, if it exists in
    /// the plugin directory.
    /// </summary>
    /// <param name="assemblyName">The assembly name to resolve to a file path. Cannot be null.</param>
    /// <returns>The full path to the assembly file if it exists in the plugin directory; otherwise, null.</returns>
    /// <exception cref="ArgumentNullException">Thrown if assemblyName is null.</exception>
    public string? ResolveAssemblyToPath(AssemblyName assemblyName)
    {
        if (assemblyName == null)
        {
            throw new ArgumentNullException(nameof(assemblyName));
        }

        var assemblyPath = Path.Combine(_pluginPath!, $"{assemblyName.Name}.dll");
        if (File.Exists(assemblyPath))
        {
            return assemblyPath;
        }

        return null;
    }

    /// <summary>
    /// Resolves the specified unmanaged DLL name to an absolute file path on disk.
    /// </summary>
    /// <param name="unmanagedDllName">The name of the unmanaged DLL to locate. This should not include a path or file extension.</param>
    /// <returns>The absolute path to the located unmanaged DLL, or null if the DLL cannot be found.</returns>
    /// <exception cref="NotImplementedException">The method is not implemented.</exception>
    public string ResolveUnmanagedDllToPath(string unmanagedDllName) =>
        throw new NotImplementedException();
}
