// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
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

/// <summary>Provides extension methods for configuring and loading plugins in host builder pipelines.</summary>
/// <remarks>These extension methods enable plugin discovery and configuration for applications using IHostBuilder
/// or IHostApplicationBuilder. They allow developers to register, scan, and load plugins as part of the application's
/// startup process. Use these methods to integrate plugin-based extensibility into your application's hosting
/// lifecycle.</remarks>
public static class HostBuilderPluginExtensions
{
    /// <summary>Stores the plugin builder key value.</summary>
    private const string PluginBuilderKey = nameof(PluginBuilder);

    /// <summary>Attempts to retrieve an existing <see cref="IPluginBuilder"/> instance from the specified properties dictionary, or creates and adds a new instance if one does not exist.</summary>
    /// <remarks>This method ensures that the properties dictionary always contains a valid <see
    /// cref="IPluginBuilder"/> instance after the call. If no instance was present, a new one is created and stored for
    /// future retrieval.</remarks>
    /// <param name="properties">The properties dictionary that stores host builder state.</param>
    /// <param name="pluginBuilder">When this method returns, contains the retrieved or newly created <see cref="IPluginBuilder"/> instance.</param>
    /// <returns><see langword="true"/> if an existing <see cref="IPluginBuilder"/> was found in the dictionary; otherwise, <see
    /// langword="false"/> and a new instance is created and added.</returns>
    private static bool TryRetrievePluginBuilder(IDictionary<object, object> properties, out IPluginBuilder pluginBuilder)
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

    /// <summary>Retrieves the order value specified by the <see cref="PluginOrderAttribute"/> applied to the plugin type.</summary>
    /// <param name="plugin">The plugin whose order should be retrieved.</param>
    /// <returns>The order value defined by the <see cref="PluginOrderAttribute"/> on the plugin type; returns 0 if the attribute
    /// is not present.</returns>
    private static int GetOrder(IPlugin plugin) =>
        plugin.GetType().GetCustomAttribute<PluginOrderAttribute>(false)?.Order ?? 0;

    /// <summary>Provides extension members for this receiver.</summary>
    /// <param name="hostBuilder">The receiver instance.</param>
    extension(IHostApplicationBuilder? hostBuilder)
    {
        /// <summary>Configures plugins for the application by invoking the specified configuration action on the plugin builder.</summary>
        /// <remarks>This method ensures that plugin scanning and loading are configured only once per host
        /// builder instance. Subsequent calls will reuse the existing plugin builder. The method is intended to be used as
        /// part of the application's startup configuration.</remarks>
        /// <param name="configurePlugin">An action that configures the plugin builder. The action receives the current plugin builder instance, or null
        /// if not available.</param>
        /// <returns>The same <see cref="IHostApplicationBuilder"/> instance for chaining further configuration.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="hostBuilder"/> is null.</exception>
        public IHostApplicationBuilder ConfigurePlugins(Action<IPluginBuilder?> configurePlugin)
        {
            if (hostBuilder is null)
            {
                throw new ArgumentNullException(nameof(hostBuilder));
            }

            var alreadyConfigured = TryRetrievePluginBuilder(hostBuilder.Properties, out var pluginBuilder);
            configurePlugin?.Invoke(pluginBuilder);

            if (!alreadyConfigured)
            {
                _ = ConfigurePluginScanAndLoad(hostBuilder);
            }

            return hostBuilder;
        }
    }

    /// <summary>Provides extension members for this receiver.</summary>
    /// <param name="hostBuilder">The receiver instance.</param>
    extension(IHostBuilder? hostBuilder)
    {
        /// <summary>Configures plugin support for the specified host builder by invoking the provided configuration action.</summary>
        /// <remarks>This method ensures that plugin support is configured only once for the host builder.
        /// Subsequent calls will reuse the existing plugin builder instance. Use this method to register or customize
        /// plugins before building the host.</remarks>
        /// <param name="configurePlugin">An action that configures the plugin builder. The action receives the current plugin builder instance, or null
        /// if plugin support has not yet been initialized.</param>
        /// <returns>The same instance of <see cref="IHostBuilder"/> with plugin support configured.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="hostBuilder"/> is null.</exception>
        public IHostBuilder ConfigurePlugins(Action<IPluginBuilder?> configurePlugin)
        {
            if (hostBuilder is null)
            {
                throw new ArgumentNullException(nameof(hostBuilder));
            }

            var alreadyConfigured = TryRetrievePluginBuilder(hostBuilder.Properties, out var pluginBuilder);
            configurePlugin?.Invoke(pluginBuilder);

            if (!alreadyConfigured)
            {
                _ = ConfigurePluginScanAndLoad(hostBuilder);
            }

            return hostBuilder;
        }
    }

    /// <summary>Configures the specified host builder to scan for and load plugins from predefined directories during application startup.</summary>
    /// <param name="hostBuilder">The host builder to configure for plugin scanning and loading. Must not be null.</param>
    /// <returns>The same instance of <see cref="IHostBuilder"/> with plugin scanning and loading configured.</returns>
    private static IHostBuilder ConfigurePluginScanAndLoad(IHostBuilder hostBuilder) =>
        hostBuilder.ConfigureServices((hostBuilderContext, serviceCollection) =>
            ConfigurePlugins(
                hostBuilder.Properties,
                hostBuilderContext.HostingEnvironment.ContentRootPath,
                plugin => plugin.ConfigureHost(hostBuilderContext, serviceCollection)));

    /// <summary>Scans for and loads plugin assemblies into the application, configuring each discovered plugin with the provided host builder.</summary>
    /// <param name="hostBuilder">The application host builder used to configure services and environment settings for plugin discovery and
    /// initialization.</param>
    /// <returns>The same <see cref="IHostApplicationBuilder"/> instance, configured with any discovered plugins.</returns>
    private static IHostApplicationBuilder ConfigurePluginScanAndLoad(IHostApplicationBuilder hostBuilder)
    {
        ConfigurePlugins(
            hostBuilder.Properties,
            hostBuilder.Environment.ContentRootPath,
            plugin => plugin.ConfigureHost(hostBuilder, hostBuilder.Services));

        return hostBuilder;
    }

    /// <summary>Scans, loads, orders, and configures plugins from the configured plugin builder.</summary>
    /// <param name="properties">The host builder properties that contain plugin configuration state.</param>
    /// <param name="contentRootPath">The content root used when content-root scanning is enabled.</param>
    /// <param name="configurePlugin">The callback used to configure each discovered plugin.</param>
    private static void ConfigurePlugins(IDictionary<object, object> properties, string contentRootPath, Action<IPlugin> configurePlugin)
    {
        _ = TryRetrievePluginBuilder(properties, out var pluginBuilder);
        AddContentRootScanDirectory(pluginBuilder, contentRootPath);

        var scannedAssemblies = LoadAssemblies(pluginBuilder);
        var plugins = GetOrderedPlugins(pluginBuilder, scannedAssemblies);

        if (plugins.Count == 0 && pluginBuilder.FailIfNoPlugins)
        {
            throw new InvalidOperationException("No plugins were found. Set FailIfNoPlugins=false to allow empty plugin sets.");
        }

        foreach (var plugin in plugins)
        {
            configurePlugin(plugin);
        }
    }

    /// <summary>Adds the content root to the plugin scan directories when configured.</summary>
    /// <param name="pluginBuilder">The plugin builder to update.</param>
    /// <param name="contentRootPath">The content root path to add.</param>
    private static void AddContentRootScanDirectory(IPluginBuilder pluginBuilder, string contentRootPath)
    {
        if (!pluginBuilder.UseContentRoot)
        {
            return;
        }

        pluginBuilder.AddScanDirectories(contentRootPath);
    }

    /// <summary>Loads all configured framework and plugin assemblies.</summary>
    /// <param name="pluginBuilder">The plugin builder that contains scan configuration.</param>
    /// <returns>The loaded assemblies that should be scanned for plugins.</returns>
    private static HashSet<Assembly> LoadAssemblies(IPluginBuilder pluginBuilder)
    {
        var scannedAssemblies = new HashSet<Assembly>();
        LoadFrameworkAssemblies(pluginBuilder, scannedAssemblies);
        LoadPluginAssemblies(pluginBuilder, scannedAssemblies);
        return scannedAssemblies;
    }

    /// <summary>Loads configured framework assemblies into the default load context.</summary>
    /// <param name="pluginBuilder">The plugin builder that contains framework scan configuration.</param>
    /// <param name="scannedAssemblies">The set that receives loaded assemblies.</param>
    private static void LoadFrameworkAssemblies(IPluginBuilder pluginBuilder, HashSet<Assembly> scannedAssemblies)
    {
        foreach (var frameworkScanRoot in pluginBuilder.FrameworkDirectories)
        {
            foreach (var frameworkAssemblyPath in pluginBuilder.FrameworkMatcher.GetResultsInFullPath(frameworkScanRoot))
            {
                var frameworkAssemblyName = new AssemblyName(Path.GetFileNameWithoutExtension(frameworkAssemblyPath));
                if (AssemblyLoadContext.Default.TryGetAssembly(frameworkAssemblyName, out var alreadyLoadedAssembly))
                {
                    _ = scannedAssemblies.Add(alreadyLoadedAssembly!);
                    continue;
                }

                var loadedAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(frameworkAssemblyPath);
                _ = scannedAssemblies.Add(loadedAssembly);
            }
        }
    }

    /// <summary>Loads configured plugin assemblies into their plugin load contexts.</summary>
    /// <param name="pluginBuilder">The plugin builder that contains plugin scan configuration.</param>
    /// <param name="scannedAssemblies">The set that receives loaded assemblies.</param>
    private static void LoadPluginAssemblies(IPluginBuilder pluginBuilder, HashSet<Assembly> scannedAssemblies)
    {
        foreach (var pluginScanRootPath in pluginBuilder.PluginDirectories)
        {
            foreach (var pluginAssemblyPath in pluginBuilder.PluginMatcher.GetResultsInFullPath(pluginScanRootPath))
            {
                var pluginAssembly = LoadPlugin(pluginBuilder, pluginAssemblyPath);
                if (pluginAssembly is not null)
                {
                    _ = scannedAssemblies.Add(pluginAssembly);
                }
            }
        }
    }

    /// <summary>Scans the loaded assemblies and returns plugins in configured order.</summary>
    /// <param name="pluginBuilder">The plugin builder that contains the assembly scanning delegate.</param>
    /// <param name="scannedAssemblies">The assemblies to scan.</param>
    /// <returns>The ordered plugins discovered from the loaded assemblies.</returns>
    private static List<IPlugin> GetOrderedPlugins(IPluginBuilder pluginBuilder, HashSet<Assembly> scannedAssemblies) =>
        scannedAssemblies
            .SelectMany(assembly => pluginBuilder.AssemblyScanFunc(assembly) ?? Enumerable.Empty<IPlugin?>())
            .Where(plugin => plugin is not null)
            .Cast<IPlugin>()
            .OrderBy(GetOrder)
            .ToList();

    /// <summary>Loads a plugin assembly from the specified location using the provided plugin builder.</summary>
    /// <remarks>The method performs validation on the plugin assembly before attempting to load it. If the
    /// assembly is already loaded in the default context, the method returns <see langword="null"/>.</remarks>
    /// <param name="pluginBuilder">An instance of <see cref="IPluginBuilder"/> used to validate and configure the plugin before loading.</param>
    /// <param name="pluginAssemblyLocation">The file path to the plugin assembly to load. Must refer to an existing file.</param>
    /// <returns>An <see cref="Assembly"/> representing the loaded plugin if successful; otherwise, <see langword="null"/> if the
    /// file does not exist, validation fails, or the assembly is already loaded.</returns>
    private static Assembly? LoadPlugin(IPluginBuilder pluginBuilder, string pluginAssemblyLocation)
    {
        if (!File.Exists(pluginAssemblyLocation) || !pluginBuilder.ValidatePlugin(pluginAssemblyLocation))
        {
            return null;
        }

        var pluginName = Path.GetFileNameWithoutExtension(pluginAssemblyLocation);
        var pluginAssemblyName = new AssemblyName(pluginName);
        if (AssemblyLoadContext.Default.TryGetAssembly(pluginAssemblyName, out _))
        {
            return null;
        }

        var loadContext = new PluginLoadContext(pluginAssemblyLocation, pluginName);
        return loadContext.LoadFromAssemblyName(pluginAssemblyName);
    }
}
