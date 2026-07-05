// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;

#if REACTIVE_SHIM
namespace ReactiveMarbles.Extensions.Hosting.Reactive.Plugins.Internals;
#else
namespace ReactiveMarbles.Extensions.Hosting.Plugins.Internals;
#endif

/// <summary>Provides extension methods for the AssemblyLoadContext class.</summary>
/// <remarks>This static class contains methods that extend the functionality of AssemblyLoadContext, enabling
/// additional operations such as searching for loaded assemblies by name. These methods are intended to simplify common
/// tasks when working with assembly loading contexts.</remarks>
public static class AssemblyLoadContextExtensions
{
    /// <summary>Provides extension members for this receiver.</summary>
    /// <param name="assemblyLoadContext">The receiver instance.</param>
    extension(AssemblyLoadContext assemblyLoadContext)
    {
        /// <summary>Attempts to find an assembly with the specified name in the given assembly load context.</summary>
        /// <remarks>The search compares only the simple name of the assembly (the Name property) and does not
        /// consider version, culture, or public key token. If multiple assemblies with the same name exist in the context,
        /// the first match is returned.</remarks>
        /// <param name="assemblyName">The name of the assembly to locate. Only the <see cref="AssemblyName.Name"/> property is used for comparison.</param>
        /// <param name="foundAssembly">When this method returns, contains the assembly that matches the specified name, if found; otherwise, null. This
        /// parameter is passed uninitialized.</param>
        /// <returns><see langword="true"/> if an assembly with the specified name is found; otherwise, <see langword="false"/>.</returns>
        public bool TryGetAssembly(AssemblyName assemblyName, out Assembly? foundAssembly)
        {
            if (assemblyLoadContext is null)
            {
                foundAssembly = null;
                return false;
            }

            foreach (var assembly in AssemblyLoadContext.Assemblies)
            {
                var name = assembly.GetName().Name;
                if (name is null)
                {
                    continue;
                }

                if (!name.Equals(assemblyName?.Name))
                {
                    continue;
                }

                foundAssembly = assembly;
                return true;
            }

            foundAssembly = null;
            return false;
        }
    }
}
