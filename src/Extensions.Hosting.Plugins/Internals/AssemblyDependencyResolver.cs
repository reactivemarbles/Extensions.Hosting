// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Reflection;

namespace ReactiveMarbles.Extensions.Hosting.Plugins.Internals;

/// <summary>
/// This is a wrapper class to simulate the behavior of the AssemblyDependencyResolver under the .NET Framework.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="AssemblyDependencyResolver"/> class.
/// </remarks>
/// <param name="pluginPath">string with the path for the plugin and where his dependencies are loaded from.</param>
public class AssemblyDependencyResolver(string pluginPath)
{
    private readonly string? _pluginPath = Path.GetDirectoryName(pluginPath);

    /// <summary>
    /// Find the assembly in the directory of the plugin.
    /// </summary>
    /// <param name="assemblyName">AssemblyName.</param>
    /// <returns>string path for the assembly.</returns>
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
    /// Find the location of an unmanaged DLL.
    /// </summary>
    /// <param name="unmanagedDllName">string.</param>
    /// <returns>string with the path.</returns>
    /// <exception cref="NotImplementedException">Intensional.</exception>
    public string ResolveUnmanagedDllToPath(string unmanagedDllName) =>
        throw new NotImplementedException();
}
