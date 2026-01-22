// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;

namespace ReactiveMarbles.Extensions.Hosting.Plugins.Internals;

/// <summary>
/// Provides functionality for loading and managing assemblies in a custom context, enabling isolation and control over
/// assembly loading behavior.
/// </summary>
/// <remarks>AssemblyLoadContext allows applications to load assemblies into isolated contexts, which can be
/// useful for plugin architectures, unloading assemblies, or resolving assembly version conflicts. Each context
/// maintains its own set of loaded assemblies and can customize how assemblies and unmanaged libraries are resolved.
/// The default context is used for most application assemblies, while additional contexts can be created for advanced
/// scenarios.</remarks>
/// <param name="name">The name that uniquely identifies the assembly load context. This value is used for diagnostic and identification
/// purposes and can be null or empty.</param>
public class AssemblyLoadContext(string name)
{
    /// <summary>
    /// Gets the default assembly load context for the application domain.
    /// </summary>
    /// <remarks>The default context is used to load assemblies that are part of the application or shared
    /// framework. Assemblies loaded into the default context are visible to all code in the application domain. Use
    /// this property to access the standard assembly loading behavior provided by .NET.</remarks>
    public static AssemblyLoadContext Default { get; } = new AssemblyLoadContext("default");

    /// <summary>
    /// Gets the name associated with the current instance.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// Gets the assemblies that are loaded into the current application domain.
    /// </summary>
    public IEnumerable<Assembly> Assemblies => AppDomain.CurrentDomain.GetAssemblies();

    /// <summary>
    /// Loads an assembly given its display name.
    /// </summary>
    /// <remarks>This method loads the assembly into the current load context. If the assembly has already
    /// been loaded, the existing assembly is returned.</remarks>
    /// <param name="assemblyName">The assembly name object that specifies the display name of the assembly to load. Cannot be null.</param>
    /// <returns>The loaded assembly represented by the specified display name.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="assemblyName"/> is null.</exception>
    public Assembly LoadFromAssemblyName(AssemblyName assemblyName)
    {
        if (assemblyName == null)
        {
            throw new ArgumentNullException(nameof(assemblyName));
        }

        // Attempt to load the assembly, using the same ordering as static load, in the current load context.
        return Load(assemblyName);
    }

    /// <summary>
    /// Loads an assembly from the specified file path.
    /// </summary>
    /// <remarks>The assembly is loaded into the load-from context. If the assembly has already been loaded,
    /// this method may return a reference to the existing assembly. This method does not resolve dependencies
    /// automatically; dependent assemblies must be available to the loader.</remarks>
    /// <param name="assemblyPath">The path to the assembly file to load. The path must be a valid file system path to a managed assembly file.</param>
    /// <returns>The loaded assembly represented by the specified file path.</returns>
    public Assembly LoadFromAssemblyPath(string assemblyPath) => Assembly.LoadFrom(assemblyPath);

    /// <summary>
    /// Loads the assembly with the specified name.
    /// </summary>
    /// <remarks>Override this method to implement custom assembly loading logic in a derived class.</remarks>
    /// <param name="assemblyName">The name of the assembly to load. Cannot be null.</param>
    /// <returns>The loaded assembly, or null if the assembly cannot be found.</returns>
    protected virtual Assembly Load(AssemblyName assemblyName) => null!;

    /// <summary>
    /// Loads an unmanaged dynamic-link library (DLL) from the specified absolute path.
    /// </summary>
    /// <remarks>This method is intended to be called by derived classes to provide custom logic for loading
    /// unmanaged libraries. The caller is responsible for ensuring that the specified path points to a valid and
    /// compatible DLL.</remarks>
    /// <param name="dllPath">The absolute path to the unmanaged DLL to load. Cannot be null or empty.</param>
    /// <returns>A handle to the loaded unmanaged DLL. Returns <see cref="IntPtr.Zero"/> if the library could not be loaded.</returns>
    protected IntPtr LoadUnmanagedDllFromPath(string dllPath) => IntPtr.Zero;

    /// <summary>
    /// Loads the specified unmanaged DLL into the process address space.
    /// </summary>
    /// <remarks>Override this method to provide custom logic for loading unmanaged libraries when resolving
    /// native dependencies. The default implementation returns <see cref="IntPtr.Zero"/>, indicating that the DLL could
    /// not be loaded.</remarks>
    /// <param name="unmanagedDllName">The name of the unmanaged DLL to load. This should not include the file extension or path.</param>
    /// <returns>A handle to the loaded DLL if successful; otherwise, <see cref="IntPtr.Zero"/>.</returns>
    protected virtual IntPtr LoadUnmanagedDll(string unmanagedDllName) => IntPtr.Zero;
}
