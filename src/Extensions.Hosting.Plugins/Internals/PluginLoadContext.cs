// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Reflection;

namespace ReactiveMarbles.Extensions.Hosting.Plugins.Internals;

/// <summary>
/// This AssemblyLoadContext uses an AssemblyDependencyResolver as described here: https://devblogs.microsoft.com/dotnet/announcing-net-core-3-preview-3/
/// Before loading an assembly, the current domain is checked if this assembly was not already loaded, if so this is returned.
/// This way the Assemblies already loaded by the application are available to all the plugins and can provide interaction.
/// </summary>
internal class PluginLoadContext(string pluginPath, string name) : AssemblyLoadContext(name)
{
    private readonly AssemblyDependencyResolver _resolver = new(pluginPath);

    /// <summary>
    /// Returns the path where the specified assembly can be found.
    /// </summary>
    /// <param name="assemblyName">AssemblyName.</param>
    /// <returns>string with the path.</returns>
    public string? ResolveAssemblyPath(AssemblyName assemblyName) =>
        _resolver.ResolveAssemblyToPath(assemblyName);

    /// <inheritdoc />
    protected override Assembly Load(AssemblyName assemblyName)
    {
        // Try to get the assembly from the AssemblyLoadContext.Default, when it is already loaded
        if (Default.TryGetAssembly(assemblyName, out var alreadyLoadedAssembly))
        {
            return alreadyLoadedAssembly!;
        }

        var assemblyPath = ResolveAssemblyPath(assemblyName);
        if (assemblyPath == null)
        {
            return null!;
        }

        return LoadFromAssemblyPath(assemblyPath);
    }

    /// <inheritdoc />
    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        var libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        if (libraryPath == null)
        {
            return IntPtr.Zero;
        }

        return LoadUnmanagedDllFromPath(libraryPath);
    }
}
