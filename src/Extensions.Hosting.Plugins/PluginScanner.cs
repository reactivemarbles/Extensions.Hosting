// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;

namespace ReactiveMarbles.Extensions.Hosting.Plugins;

/// <summary>Provides methods for discovering and instantiating plugin implementations from assemblies using naming conventions or type scanning.</summary>
/// <remarks>The PluginScanner class is intended for use in scenarios where plugins implementing the IPlugin
/// interface need to be dynamically located and instantiated from assemblies. All methods are static and
/// thread-safe.</remarks>
public static class PluginScanner
{
    /// <summary>Discovers and instantiates plugins from the specified assembly using a naming convention.</summary>
    /// <remarks>This method looks for a type named 'Plugin' in the root namespace of the provided assembly.
    /// The type must implement the IPlugin interface and have a public parameterless constructor. If no such type
    /// exists, the method returns an empty sequence.</remarks>
    /// <param name="pluginAssembly">The assembly to search for a type named 'Plugin' within its root namespace. Cannot be null.</param>
    /// <returns>An enumerable containing the instantiated plugin if a matching type is found; otherwise, an empty enumerable.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="pluginAssembly"/> is null.</exception>
    public static IEnumerable<IPlugin> ByNamingConvention(Assembly pluginAssembly)
    {
        if (pluginAssembly is null)
        {
            throw new ArgumentNullException(nameof(pluginAssembly));
        }

        return ByNamingConventionCore(pluginAssembly);
    }

    /// <summary>Scans the specified assembly for types that implement the IPlugin interface and creates instances of each discovered plugin type.</summary>
    /// <remarks>Only non-abstract, public classes that implement IPlugin are considered. Each type is
    /// instantiated using its parameterless constructor. If instantiation fails, the corresponding element in the
    /// returned collection will be null.</remarks>
    /// <param name="pluginAssembly">The assembly to scan for types implementing the IPlugin interface. Cannot be null.</param>
    /// <returns>An enumerable collection of IPlugin instances representing each non-abstract, public class in the assembly that
    /// implements IPlugin. The collection may contain null values if a plugin type cannot be instantiated.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="pluginAssembly"/> is null.</exception>
    public static IEnumerable<IPlugin> ScanForPluginInstances(Assembly pluginAssembly)
    {
        if (pluginAssembly is null)
        {
            throw new ArgumentNullException(nameof(pluginAssembly));
        }

        return ScanForPluginInstancesCore(pluginAssembly);
    }

    /// <summary>Discovers a plugin using the conventional root namespace Plugin type name.</summary>
    /// <param name="pluginAssembly">The assembly to scan.</param>
    /// <returns>The discovered plugin, or an empty sequence when no conventional plugin type exists.</returns>
    private static IEnumerable<IPlugin> ByNamingConventionCore(Assembly pluginAssembly)
    {
        var assemblyName = pluginAssembly.GetName().Name;
        var type = pluginAssembly.GetType($"{assemblyName}.Plugin", false, false);
        if (type is null || Activator.CreateInstance(type) is not IPlugin plugin)
        {
            yield break;
        }

        yield return plugin;
    }

    /// <summary>Scans the supplied assembly for concrete plugin implementations.</summary>
    /// <param name="pluginAssembly">The assembly to scan.</param>
    /// <returns>The plugin instances created from matching exported types.</returns>
    private static IEnumerable<IPlugin> ScanForPluginInstancesCore(Assembly pluginAssembly)
    {
        var pluginType = typeof(IPlugin);
        foreach (var type in pluginAssembly.ExportedTypes)
        {
            if (!pluginType.IsAssignableFrom(type) || !type.IsClass || type.IsAbstract)
            {
                continue;
            }

            if (Activator.CreateInstance(type) is IPlugin plugin)
            {
                yield return plugin;
            }
        }
    }
}
