// Copyright (c) 2019-2024 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;

namespace ReactiveMarbles.Extensions.Hosting.Plugins.Internals;

/// <summary>
/// This is a wrapper class to simulate the behavior of the AssemblyLoadContext under the .NET Framework.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="AssemblyLoadContext"/> class.
/// </remarks>
/// <param name="name">The name.</param>
public class AssemblyLoadContext(string name)
{
    /// <summary>
    /// Gets default AssemblyLoadContext.
    /// </summary>
    public static AssemblyLoadContext Default { get; } = new AssemblyLoadContext("default");

    /// <summary>
    /// Gets the name of the AssemblyLoadContext.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// Gets all the available assemblies.
    /// </summary>
    public IEnumerable<Assembly> Assemblies => AppDomain.CurrentDomain.GetAssemblies();

    /// <summary>
    /// Load the assembly by name.
    /// </summary>
    /// <param name="assemblyName">AssemblyName.</param>
    /// <returns>Assembly.</returns>
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
    /// Load an assembly from the path.
    /// </summary>
    /// <param name="assemblyPath">string with the path to the assembly file.</param>
    /// <returns>Assembly.</returns>
    public Assembly LoadFromAssemblyPath(string assemblyPath) => Assembly.LoadFrom(assemblyPath);

    /// <summary>
    /// Implement the loading of the assembly.
    /// </summary>
    /// <param name="assemblyName">AssemblyName.</param>
    /// <returns>Assembly.</returns>
    protected virtual Assembly Load(AssemblyName assemblyName) => null!;

    /// <summary>
    /// Load the DLL from the specified path.
    /// </summary>
    /// <param name="dllPath">string.</param>
    /// <returns>IntPtr.</returns>
#pragma warning disable RCS1163 // Unused parameter
    protected IntPtr LoadUnmanagedDllFromPath(string dllPath) => IntPtr.Zero;
#pragma warning restore RCS1163 // Unused parameter

    /// <summary>
    /// Loads the specified DLL from the plugin path.
    /// </summary>
    /// <param name="unmanagedDllName">string.</param>
    /// <returns>IntPtr.</returns>
    protected virtual IntPtr LoadUnmanagedDll(string unmanagedDllName) => IntPtr.Zero;
}
