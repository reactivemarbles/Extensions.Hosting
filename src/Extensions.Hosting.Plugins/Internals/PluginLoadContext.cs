// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Reflection;

namespace ReactiveMarbles.Extensions.Hosting.Plugins.Internals;

/// <summary>
/// Provides an isolated assembly load context for loading plugins from a specified directory.
/// </summary>
/// <remarks>PluginLoadContext enables loading and resolving assemblies and unmanaged libraries for plugins
/// without interfering with the default application context. This allows multiple plugins to be loaded with their own
/// dependencies, reducing the risk of version conflicts. Assemblies are resolved relative to the specified plugin
/// path.</remarks>
/// <param name="pluginPath">The file system path to the root directory containing the plugin's assemblies and dependencies. Cannot be null or
/// empty.</param>
/// <param name="name">The unique name for the assembly load context. Used to identify the context within the application.</param>
internal class PluginLoadContext(string pluginPath, string name) : AssemblyLoadContext(name)
{
    private readonly AssemblyDependencyResolver _resolver = new(pluginPath);

    /// <summary>
    /// Resolves the file system path to the assembly specified by the given assembly name.
    /// </summary>
    /// <param name="assemblyName">The assembly name to resolve. Cannot be null.</param>
    /// <returns>The full path to the resolved assembly file, or null if the assembly cannot be found.</returns>
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
