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
/// Provides extension methods for configuring and loading plugins in host builder pipelines.
/// </summary>
/// <remarks>These extension methods enable plugin discovery and configuration for applications using IHostBuilder
/// or IHostApplicationBuilder. They allow developers to register, scan, and load plugins as part of the application's
/// startup process. Use these methods to integrate plugin-based extensibility into your application's hosting
/// lifecycle.</remarks>
public static class HostBuilderPluginExtensions
{
    private const string PluginBuilderKey = nameof(PluginBuilder);

    /// <summary>
    /// Configures plugin support for the specified host builder by invoking the provided configuration action.
    /// </summary>
    /// <remarks>This method ensures that plugin support is configured only once for the host builder.
    /// Subsequent calls will reuse the existing plugin builder instance. Use this method to register or customize
    /// plugins before building the host.</remarks>
    /// <param name="hostBuilder">The host builder to configure with plugin support. Cannot be null.</param>
    /// <param name="configurePlugin">An action that configures the plugin builder. The action receives the current plugin builder instance, or null
    /// if plugin support has not yet been initialized.</param>
    /// <returns>The same instance of <see cref="IHostBuilder"/> with plugin support configured.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="hostBuilder"/> is null.</exception>
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
    /// Configures plugins for the application by invoking the specified configuration action on the plugin builder.
    /// </summary>
    /// <remarks>This method ensures that plugin scanning and loading are configured only once per host
    /// builder instance. Subsequent calls will reuse the existing plugin builder. The method is intended to be used as
    /// part of the application's startup configuration.</remarks>
    /// <param name="hostBuilder">The host application builder to configure. Cannot be null.</param>
    /// <param name="configurePlugin">An action that configures the plugin builder. The action receives the current plugin builder instance, or null
    /// if not available.</param>
    /// <returns>The same <see cref="IHostApplicationBuilder"/> instance for chaining further configuration.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="hostBuilder"/> is null.</exception>
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
    /// Attempts to retrieve an existing <see cref="IPluginBuilder"/> instance from the specified properties dictionary,
    /// or creates and adds a new instance if one does not exist.
    /// </summary>
    /// <remarks>This method ensures that the properties dictionary always contains a valid <see
    /// cref="IPluginBuilder"/> instance after the call. If no instance was present, a new one is created and stored for
    /// future retrieval.</remarks>
    /// <param name="properties">The dictionary containing property values, used to store and retrieve the <see cref="IPluginBuilder"/> instance.
    /// Cannot be null.</param>
    /// <param name="pluginBuilder">When this method returns, contains the retrieved or newly created <see cref="IPluginBuilder"/> instance.</param>
    /// <returns><see langword="true"/> if an existing <see cref="IPluginBuilder"/> was found in the dictionary; otherwise, <see
    /// langword="false"/> and a new instance is created and added.</returns>
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
    /// Configures the specified host builder to scan for and load plugins from predefined directories during
    /// application startup.
    /// </summary>
    /// <remarks>This method adds services to the dependency injection container by scanning specified
    /// framework and plugin directories for assemblies. It invokes each discovered plugin's configuration logic. The
    /// method should be called before building the host to ensure all plugins are loaded and configured
    /// appropriately.</remarks>
    /// <param name="hostBuilder">The host builder to configure for plugin scanning and loading. Must not be null.</param>
    /// <returns>The same instance of <see cref="IHostBuilder"/> with plugin scanning and loading configured.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no plugins are found and the configuration requires at least one plugin to be present.</exception>
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

            var plugins = scannedAssemblies.SelectMany(pluginBuilder.AssemblyScanFunc!).Where(plugin => plugin != null).OrderBy(plugin => plugin?.GetOrder()).ToList();

            if (plugins.Count == 0 && pluginBuilder.FailIfNoPlugins)
            {
                throw new InvalidOperationException("No plugins were found. Set FailIfNoPlugins=false to allow empty plugin sets.");
            }

            foreach (var plugin in plugins)
            {
                plugin?.ConfigureHost(hostBuilderContext, serviceCollection);
            }
        });

    /// <summary>
    /// Scans for and loads plugin assemblies into the application, configuring each discovered plugin with the provided
    /// host builder.
    /// </summary>
    /// <remarks>This method scans specified directories for both framework and plugin assemblies, loads them,
    /// and invokes their configuration logic. The behavior is controlled by plugin builder properties such as scan
    /// directories and failure conditions. Plugins are configured in order as determined by their metadata. The method
    /// does not modify the original host builder instance beyond plugin configuration.</remarks>
    /// <param name="hostBuilder">The application host builder used to configure services and environment settings for plugin discovery and
    /// initialization.</param>
    /// <returns>The same <see cref="IHostApplicationBuilder"/> instance, configured with any discovered plugins.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no plugins are found and the plugin configuration requires at least one plugin to be present.</exception>
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

            var plugins = scannedAssemblies.SelectMany(pluginBuilder.AssemblyScanFunc!).Where(plugin => plugin != null).OrderBy(plugin => plugin?.GetOrder()).ToList();

            if (plugins.Count == 0 && pluginBuilder.FailIfNoPlugins)
            {
                throw new InvalidOperationException("No plugins were found. Set FailIfNoPlugins=false to allow empty plugin sets.");
            }

            foreach (var plugin in plugins)
            {
                plugin?.ConfigureHost(hostBuilder, hostBuilder.Services);
            }

            return hostBuilder;
    }

    /// <summary>
    /// Retrieves the order value specified by the <see cref="PluginOrderAttribute"/> applied to the plugin type.
    /// </summary>
    /// <param name="plugin">The plugin instance whose order value is to be retrieved. Cannot be null.</param>
    /// <returns>The order value defined by the <see cref="PluginOrderAttribute"/> on the plugin type; returns 0 if the attribute
    /// is not present.</returns>
    private static int GetOrder(this IPlugin plugin) =>
        plugin.GetType().GetCustomAttribute<PluginOrderAttribute>(false)?.Order ?? 0;

    /// <summary>
    /// Loads a plugin assembly from the specified location using the provided plugin builder.
    /// </summary>
    /// <remarks>The method performs validation on the plugin assembly before attempting to load it. If the
    /// assembly is already loaded in the default context, the method returns <see langword="null"/>.</remarks>
    /// <param name="pluginBuilder">An instance of <see cref="IPluginBuilder"/> used to validate and configure the plugin before loading.</param>
    /// <param name="pluginAssemblyLocation">The file path to the plugin assembly to load. Must refer to an existing file.</param>
    /// <returns>An <see cref="Assembly"/> representing the loaded plugin if successful; otherwise, <see langword="null"/> if the
    /// file does not exist, validation fails, or the assembly is already loaded.</returns>
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
