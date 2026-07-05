// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Reflection;

namespace ReactiveMarbles.Extensions.Hosting.Plugins.Internals;

/// <summary>Provides an isolated assembly load context for loading plugins from a specified directory.</summary>
/// <remarks>PluginLoadContext enables loading and resolving assemblies and unmanaged libraries for plugins
/// without interfering with the default application context. This allows multiple plugins to be loaded with their own
/// dependencies, reducing the risk of version conflicts. Assemblies are resolved relative to the specified plugin
/// path.</remarks>
/// <param name="pluginPath">The file system path to the root directory containing the plugin's assemblies and dependencies. Cannot be null or
/// empty.</param>
/// <param name="name">The unique name for the assembly load context. Used to identify the context within the application.</param>
internal sealed class PluginLoadContext(string pluginPath, string name) : AssemblyLoadContext(name)
{
    /// <summary>Stores the resolver value.</summary>
    private readonly AssemblyDependencyResolver _resolver = new(pluginPath);

    /// <summary>Resolves the file system path to the assembly specified by the given assembly name.</summary>
    /// <param name="assemblyName">The assembly name to resolve. Cannot be null.</param>
    /// <returns>The full path to the resolved assembly file, or null if the assembly cannot be found.</returns>
    public string? ResolveAssemblyPath(AssemblyName assemblyName) =>
        _resolver.ResolveAssemblyToPath(assemblyName);

    /// <inheritdoc />
    protected override Assembly Load(AssemblyName assemblyName) =>
        Default.TryGetAssembly(assemblyName, out var alreadyLoadedAssembly)
            ? alreadyLoadedAssembly!
            : LoadAssemblyFromResolvedPath(assemblyName)!;

    /// <inheritdoc />
    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        var libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        return libraryPath is null ? IntPtr.Zero : LoadUnmanagedDllFromPath(libraryPath);
    }

    /// <summary>Loads an assembly from the resolved plugin path when one exists.</summary>
    /// <param name="assemblyName">The assembly name to resolve and load.</param>
    /// <returns>The loaded assembly, or null when no plugin-local path exists.</returns>
    private Assembly? LoadAssemblyFromResolvedPath(AssemblyName assemblyName)
    {
        var assemblyPath = ResolveAssemblyPath(assemblyName);
        return assemblyPath is null ? null : LoadFromAssemblyPath(assemblyPath);
    }
}
