// Copyright (c) 2019-2024 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;

namespace ReactiveMarbles.Extensions.Hosting.Plugins.Internals;

/// <summary>
/// AssemblyLoadContext extensions.
/// </summary>
public static class AssemblyLoadContextExtensions
{
    /// <summary>
    /// Try to get an assembly from the specified AssemblyLoadContext.
    /// </summary>
    /// <param name="assemblyLoadContext">AssemblyLoadContext.</param>
    /// <param name="assemblyName">AssemblyName to look for.</param>
    /// <param name="foundAssembly">Assembly out.</param>
    /// <returns>bool.</returns>
    public static bool TryGetAssembly(this AssemblyLoadContext assemblyLoadContext, AssemblyName assemblyName, out Assembly? foundAssembly)
    {
        if (assemblyLoadContext == null)
        {
            foundAssembly = null;
            return false;
        }

        foreach (var assembly in assemblyLoadContext.Assemblies)
        {
            var name = assembly.GetName().Name;
            if (name == null)
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
