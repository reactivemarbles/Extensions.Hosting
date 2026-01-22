// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ReactiveMarbles.Extensions.Hosting.Plugins;

/// <summary>
/// Provides methods for discovering and instantiating plugin implementations from assemblies using naming conventions
/// or type scanning.
/// </summary>
/// <remarks>The PluginScanner class is intended for use in scenarios where plugins implementing the IPlugin
/// interface need to be dynamically located and instantiated from assemblies. All methods are static and
/// thread-safe.</remarks>
public static class PluginScanner
{
    /// <summary>
    /// Discovers and instantiates plugins from the specified assembly using a naming convention.
    /// </summary>
    /// <remarks>This method looks for a type named 'Plugin' in the root namespace of the provided assembly.
    /// The type must implement the IPlugin interface and have a public parameterless constructor. If no such type
    /// exists, the method returns an empty sequence.</remarks>
    /// <param name="pluginAssembly">The assembly to search for a type named 'Plugin' within its root namespace. Cannot be null.</param>
    /// <returns>An enumerable containing the instantiated plugin if a matching type is found; otherwise, an empty enumerable.</returns>
    public static IEnumerable<IPlugin?> ByNamingConvention(Assembly pluginAssembly)
    {
        var assemblyName = pluginAssembly?.GetName().Name;
        var type = pluginAssembly?.GetType($"{assemblyName}.Plugin", false, false);
        if (type != null)
        {
            yield return (IPlugin?)Activator.CreateInstance(type);
        }
    }

    /// <summary>
    /// Scans the specified assembly for types that implement the IPlugin interface and creates instances of each
    /// discovered plugin type.
    /// </summary>
    /// <remarks>Only non-abstract, public classes that implement IPlugin are considered. Each type is
    /// instantiated using its parameterless constructor. If instantiation fails, the corresponding element in the
    /// returned collection will be null.</remarks>
    /// <param name="pluginAssembly">The assembly to scan for types implementing the IPlugin interface. Cannot be null.</param>
    /// <returns>An enumerable collection of IPlugin instances representing each non-abstract, public class in the assembly that
    /// implements IPlugin. The collection may contain null values if a plugin type cannot be instantiated. Returns null
    /// if pluginAssembly is null.</returns>
    public static IEnumerable<IPlugin?>? ScanForPluginInstances(Assembly pluginAssembly)
    {
        var pluginType = typeof(IPlugin);
        return pluginAssembly?.ExportedTypes
            .Where(type => pluginType.IsAssignableFrom(type) && type.IsClass && !type.IsAbstract)
            .Select(type => (IPlugin?)Activator.CreateInstance(type));
    }
}
