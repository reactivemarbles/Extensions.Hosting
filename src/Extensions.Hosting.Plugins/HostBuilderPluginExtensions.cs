// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.Hosting;
using ReactiveMarbles.Extensions.Hosting.Plugins.Internals;

namespace ReactiveMarbles.Extensions.Hosting.Plugins;

/// <summary>
/// Extensions for adding plug-ins to your host.
/// </summary>
public static class HostBuilderPluginExtensions
{
    private const string PluginBuilderKey = nameof(PluginBuilder);

    /// <summary>
    /// Configure the plugins.
    /// </summary>
    /// <param name="hostBuilder">IHostBuilder.</param>
    /// <param name="configurePlugin">Action to configure the IPluginBuilder.</param>
    /// <returns>An IHostBuilder.</returns>
    public static IHostBuilder ConfigurePlugins(this IHostBuilder? hostBuilder, Action<IPluginBuilder?> configurePlugin)
    {
        if (hostBuilder == null)
        {
            throw new ArgumentNullException(nameof(hostBuilder));
        }

        if (!hostBuilder.Properties.TryRetrievePluginBuilder(out var pluginBuilder))
        {
            // Configure a single time
            ConfigurePluginScanAndLoad(hostBuilder);
        }

        configurePlugin?.Invoke(pluginBuilder);

        return hostBuilder;
    }

    /// <summary>
    /// Configure the plugins.
    /// </summary>
    /// <param name="hostBuilder">IHostBuilder.</param>
    /// <param name="configurePlugin">Action to configure the IPluginBuilder.</param>
    /// <returns>An IHostBuilder.</returns>
    public static IHostApplicationBuilder ConfigurePlugins(this IHostApplicationBuilder? hostBuilder, Action<IPluginBuilder?> configurePlugin)
    {
        if (hostBuilder == null)
        {
            throw new ArgumentNullException(nameof(hostBuilder));
        }

        if (!hostBuilder.Properties.TryRetrievePluginBuilder(out var pluginBuilder))
        {
            // Configure a single time
            ConfigurePluginScanAndLoad(hostBuilder);
        }

        configurePlugin?.Invoke(pluginBuilder);

        return hostBuilder;
    }

    /// <summary>
    /// Helper method to retrieve the plugin builder.
    /// </summary>
    /// <param name="properties">IDictionary.</param>
    /// <param name="pluginBuilder">IPluginBuilder out value.</param>
    /// <returns>bool if there was a matcher.</returns>
    private static bool TryRetrievePluginBuilder(this IDictionary<object, object> properties, out IPluginBuilder pluginBuilder)
    {
        if (properties.TryGetValue(PluginBuilderKey, out var pluginBuilderObject))
        {
            pluginBuilder = (IPluginBuilder)pluginBuilderObject;
            return true;
        }

        pluginBuilder = new PluginBuilder();
        properties[PluginBuilderKey] = pluginBuilder;
        return false;
    }

    /// <summary>
    /// This enables scanning for and loading of plug-ins.
    /// </summary>
    /// <param name="hostBuilder">IHostBuilder.</param>
    private static IHostBuilder ConfigurePluginScanAndLoad(IHostBuilder hostBuilder) =>
        //// Configure the actual scanning & loading
        hostBuilder.ConfigureServices((hostBuilderContext, serviceCollection) =>
        {
            hostBuilder.Properties.TryRetrievePluginBuilder(out var pluginBuilder);

            if (pluginBuilder.UseContentRoot)
            {
                var contentRootPath = hostBuilderContext.HostingEnvironment.ContentRootPath;
                pluginBuilder.AddScanDirectories(contentRootPath);
            }

            var scannedAssemblies = new HashSet<Assembly?>();

            if (pluginBuilder.FrameworkDirectories.Count > 0)
            {
                foreach (var frameworkScanRoot in pluginBuilder.FrameworkDirectories)
                {
                    // Do the globbing and try to load the framework files into the default AssemblyLoadContext
                    foreach (var frameworkAssemblyPath in pluginBuilder.FrameworkMatcher.GetResultsInFullPath(frameworkScanRoot))
                    {
                        var frameworkAssemblyName = new AssemblyName(Path.GetFileNameWithoutExtension(frameworkAssemblyPath));
                        if (AssemblyLoadContext.Default.TryGetAssembly(frameworkAssemblyName, out var alreadyLoadedAssembly))
                        {
                            scannedAssemblies.Add(alreadyLoadedAssembly);
                            continue;
                        }

                        // TODO: Log the loading?
                        var loadedAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(frameworkAssemblyPath);
                        scannedAssemblies.Add(loadedAssembly);
                    }
                }
            }

            if (pluginBuilder.PluginDirectories.Count > 0)
            {
                foreach (var pluginScanRootPath in pluginBuilder.PluginDirectories)
                {
                    // Do the globbing and try to load the plug-ins
                    var pluginPaths = pluginBuilder.PluginMatcher.GetResultsInFullPath(pluginScanRootPath);

                    // Use the globbed files, and load the assemblies
                    var pluginAssemblies = pluginPaths
                        .Select(s => LoadPlugin(pluginBuilder, s))
                        .Where(plugin => plugin != null);
                    foreach (var pluginAssembly in pluginAssemblies)
                    {
                        scannedAssemblies.Add(pluginAssembly);
                    }
                }
            }

            var plugins = scannedAssemblies.SelectMany(pluginBuilder.AssemblyScanFunc!).Where(plugin => plugin != null).OrderBy(plugin => plugin?.GetOrder());

            foreach (var plugin in plugins)
            {
                plugin?.ConfigureHost(hostBuilderContext, serviceCollection);
            }
        });

    /// <summary>
    /// This enables scanning for and loading of plug-ins.
    /// </summary>
    /// <param name="hostBuilder">IHostBuilder.</param>
    private static IHostApplicationBuilder ConfigurePluginScanAndLoad(IHostApplicationBuilder hostBuilder)
    {
        //// Configure the actual scanning & loading
            hostBuilder.Properties.TryRetrievePluginBuilder(out var pluginBuilder);

            if (pluginBuilder.UseContentRoot)
            {
                var contentRootPath = hostBuilder.Environment.ContentRootPath;
                pluginBuilder.AddScanDirectories(contentRootPath);
            }

            var scannedAssemblies = new HashSet<Assembly?>();

            if (pluginBuilder.FrameworkDirectories.Count > 0)
            {
                foreach (var frameworkScanRoot in pluginBuilder.FrameworkDirectories)
                {
                    // Do the globbing and try to load the framework files into the default AssemblyLoadContext
                    foreach (var frameworkAssemblyPath in pluginBuilder.FrameworkMatcher.GetResultsInFullPath(frameworkScanRoot))
                    {
                        var frameworkAssemblyName = new AssemblyName(Path.GetFileNameWithoutExtension(frameworkAssemblyPath));
                        if (AssemblyLoadContext.Default.TryGetAssembly(frameworkAssemblyName, out var alreadyLoadedAssembly))
                        {
                            scannedAssemblies.Add(alreadyLoadedAssembly);
                            continue;
                        }

                        // TODO: Log the loading?
                        var loadedAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(frameworkAssemblyPath);
                        scannedAssemblies.Add(loadedAssembly);
                    }
                }
            }

            if (pluginBuilder.PluginDirectories.Count > 0)
            {
                foreach (var pluginScanRootPath in pluginBuilder.PluginDirectories)
                {
                    // Do the globbing and try to load the plug-ins
                    var pluginPaths = pluginBuilder.PluginMatcher.GetResultsInFullPath(pluginScanRootPath);

                    // Use the globbed files, and load the assemblies
                    var pluginAssemblies = pluginPaths
                        .Select(s => LoadPlugin(pluginBuilder, s))
                        .Where(plugin => plugin != null);
                    foreach (var pluginAssembly in pluginAssemblies)
                    {
                        scannedAssemblies.Add(pluginAssembly);
                    }
                }
            }

            var plugins = scannedAssemblies.SelectMany(pluginBuilder.AssemblyScanFunc!).Where(plugin => plugin != null).OrderBy(plugin => plugin?.GetOrder());

            foreach (var plugin in plugins)
            {
                plugin?.ConfigureHost(hostBuilder, hostBuilder.Services);
            }

            return hostBuilder;
    }

    /// <summary>
    /// Helper method to process the PluginOrder attribute.
    /// </summary>
    /// <param name="plugin">IPlugin.</param>
    /// <returns>int.</returns>
    private static int GetOrder(this IPlugin plugin) =>
        plugin.GetType().GetCustomAttribute<PluginOrderAttribute>(false)?.Order ?? 0;

    /// <summary>
    /// Helper method to load an assembly which contains plugins.
    /// </summary>
    /// <param name="pluginBuilder">IPluginBuilder.</param>
    /// <param name="pluginAssemblyLocation">string.</param>
    /// <returns>IPlugin.</returns>
    private static Assembly? LoadPlugin(IPluginBuilder pluginBuilder, string pluginAssemblyLocation)
    {
        if (!File.Exists(pluginAssemblyLocation))
        {
            // TODO: Log an error, how to get a logger here?
            return null;
        }

        // This allows validation like AuthenticodeExaminer
        if (!pluginBuilder.ValidatePlugin(pluginAssemblyLocation))
        {
            return null;
        }

        // TODO: Log verbose that we are loading a plugin
        var pluginName = Path.GetFileNameWithoutExtension(pluginAssemblyLocation);

        // TODO: Decide if we rather have this to come up with the name: AssemblyName.GetAssemblyName(pluginLocation)
        var pluginAssemblyName = new AssemblyName(pluginName);
        if (AssemblyLoadContext.Default.TryGetAssembly(pluginAssemblyName, out _))
        {
            return null!;
        }

        var loadContext = new PluginLoadContext(pluginAssemblyLocation, pluginName);
        return loadContext.LoadFromAssemblyName(pluginAssemblyName);
    }
}
