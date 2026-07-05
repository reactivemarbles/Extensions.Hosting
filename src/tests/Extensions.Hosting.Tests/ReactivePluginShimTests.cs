// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactiveMarbles.Extensions.Hosting.Reactive.Plugins;

namespace Extensions.Hosting.Tests;

/// <summary>Contains tests for the reactive plugin shim surface.</summary>
public class ReactivePluginShimTests
{
    /// <summary>Verifies that the reactive shim PluginBase registers hosted services.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task PluginBase_WithReactiveNamespace_RegistersHostedService()
    {
        var services = new ServiceCollection();
        var plugin = new PluginBase<ReactiveHostedService>();

        plugin.ConfigureHost(new object(), services);

        await Assert.That(services.Any(IsReactiveHostedServiceRegistration)).IsTrue();
    }

    /// <summary>Verifies that the reactive shim plugin scanner discovers reactive plugin implementations.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task PluginScanner_WithReactiveNamespace_FindsReactivePlugin()
    {
        var plugins = PluginScanner.ScanForPluginInstances(typeof(ReactiveDiscoveredPlugin).Assembly).ToList();

        await Assert.That(plugins.Any(plugin => plugin is ReactiveDiscoveredPlugin)).IsTrue();
    }

    /// <summary>Verifies that the reactive shim host builder extension configures discovered plugins in order.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigurePlugins_WithReactiveNamespace_LoadsConfiguredPluginsInOrder()
    {
        var configuredPlugins = new List<string>();
        var hostBuilder = Host.CreateDefaultBuilder();

        _ = hostBuilder.ConfigurePlugins(builder =>
        {
            ArgumentNullException.ThrowIfNull(builder);
            AddCurrentAssemblyAsFramework(builder);
            builder.AssemblyScanFunc = _ =>
            [
                new LaterReactiveRecordingPlugin(configuredPlugins),
                null,
                new EarlierReactiveRecordingPlugin(configuredPlugins),
            ];
        });

        using var host = hostBuilder.Build();

        await Assert.That(configuredPlugins.Count).IsEqualTo(2);
        await Assert.That(configuredPlugins[0]).IsEqualTo(EarlierReactiveRecordingPlugin.Name);
        await Assert.That(configuredPlugins[1]).IsEqualTo(LaterReactiveRecordingPlugin.Name);
    }

    /// <summary>Verifies that ConfigurePlugins with a null IHostBuilder throws.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigurePlugins_IHostBuilder_WithNullHostBuilder_ThrowsArgumentNullException()
    {
        IHostBuilder? hostBuilder = null;
        var act = () => hostBuilder!.ConfigurePlugins(_ => { });
        await Assert.That(act).Throws<ArgumentNullException>();
    }

    /// <summary>Verifies that ConfigurePlugins with a null IHostApplicationBuilder throws.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigurePlugins_IHostApplicationBuilder_WithNullHostBuilder_ThrowsArgumentNullException()
    {
        IHostApplicationBuilder? hostBuilder = null;
        var act = () => hostBuilder!.ConfigurePlugins(_ => { });
        await Assert.That(act).Throws<ArgumentNullException>();
    }

    /// <summary>Verifies that IHostApplicationBuilder applies caller configuration before scanning for plugins.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigurePlugins_IHostApplicationBuilder_AppliesConfigurationBeforeScanning()
    {
        var configuredPlugins = new List<string>();
        var hostBuilder = Host.CreateApplicationBuilder();

        var result = hostBuilder.ConfigurePlugins(builder =>
        {
            ArgumentNullException.ThrowIfNull(builder);
            AddCurrentAssemblyAsFramework(builder);
            builder.AssemblyScanFunc = _ => [new EarlierReactiveRecordingPlugin(configuredPlugins)];
        });

        await Assert.That(result).IsEqualTo(hostBuilder);
        await Assert.That(configuredPlugins.Count).IsEqualTo(1);
        await Assert.That(configuredPlugins[0]).IsEqualTo(EarlierReactiveRecordingPlugin.Name);
    }

    /// <summary>Verifies that repeated IHostBuilder plugin configuration calls reuse the same builder instance.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigurePlugins_IHostBuilder_MultipleCalls_ReusePluginBuilder()
    {
        IPluginBuilder? firstBuilder = null;
        IPluginBuilder? secondBuilder = null;
        var hostBuilder = Host.CreateDefaultBuilder();

        _ = hostBuilder.ConfigurePlugins(builder => firstBuilder = builder);
        _ = hostBuilder.ConfigurePlugins(builder => secondBuilder = builder);

        await Assert.That(firstBuilder).IsNotNull();
        await Assert.That(secondBuilder).IsNotNull();
        await Assert.That(firstBuilder).IsEqualTo(secondBuilder);
    }

    /// <summary>Verifies that repeated IHostApplicationBuilder plugin configuration calls reuse the same builder instance.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigurePlugins_IHostApplicationBuilder_MultipleCalls_ReusePluginBuilder()
    {
        IPluginBuilder? firstBuilder = null;
        IPluginBuilder? secondBuilder = null;
        var hostBuilder = Host.CreateApplicationBuilder();

        _ = hostBuilder.ConfigurePlugins(builder => firstBuilder = builder);
        _ = hostBuilder.ConfigurePlugins(builder => secondBuilder = builder);

        await Assert.That(firstBuilder).IsNotNull();
        await Assert.That(secondBuilder).IsNotNull();
        await Assert.That(firstBuilder).IsEqualTo(secondBuilder);
    }

    /// <summary>Verifies that content-root scanning can provide the assembly used for plugin discovery.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigurePlugins_IHostBuilder_WithContentRootScan_LoadsConfiguredPlugin()
    {
        var configuredPlugins = new List<string>();
        var assemblyPath = typeof(ReactivePluginShimTests).Assembly.Location;
        var hostBuilder = Host.CreateDefaultBuilder()
            .UseContentRoot(Path.GetDirectoryName(assemblyPath)!);

        _ = hostBuilder.ConfigurePlugins(builder =>
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.UseContentRoot = true;
            _ = builder.FrameworkMatcher.AddInclude(Path.GetFileName(assemblyPath));
            builder.AssemblyScanFunc = _ => [new EarlierReactiveRecordingPlugin(configuredPlugins)];
        });

        using var host = hostBuilder.Build();

        await Assert.That(configuredPlugins.Count).IsEqualTo(1);
        await Assert.That(configuredPlugins[0]).IsEqualTo(EarlierReactiveRecordingPlugin.Name);
    }

    /// <summary>Verifies that invalid plugin files discovered by the plugin matcher are ignored.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigurePlugins_IHostBuilder_WithInvalidPluginFile_IgnoresPlugin()
    {
        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var pluginPath = Path.Combine(tempDirectory, "InvalidPlugin.dll");
            await File.WriteAllTextAsync(pluginPath, string.Empty);
            var hostBuilder = Host.CreateDefaultBuilder();

            _ = hostBuilder.ConfigurePlugins(builder =>
            {
                ArgumentNullException.ThrowIfNull(builder);
                builder.PluginDirectories.Add(tempDirectory);
                _ = builder.PluginMatcher.AddInclude(Path.GetFileName(pluginPath));
                builder.ValidatePlugin = _ => false;
            });

            using var host = hostBuilder.Build();
            await Assert.That(host).IsNotNull();
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

    /// <summary>Verifies that plugin files already loaded in the default context are ignored.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigurePlugins_IHostBuilder_WithAlreadyLoadedPluginFile_IgnoresPlugin()
    {
        var assemblyPath = typeof(ReactivePluginShimTests).Assembly.Location;
        var hostBuilder = Host.CreateDefaultBuilder();

        _ = hostBuilder.ConfigurePlugins(builder =>
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.PluginDirectories.Add(Path.GetDirectoryName(assemblyPath)!);
            _ = builder.PluginMatcher.AddInclude(Path.GetFileName(assemblyPath));
        });

        using var host = hostBuilder.Build();
        await Assert.That(host).IsNotNull();
    }

    /// <summary>Verifies that framework assemblies not yet loaded in the default context are loaded and scanned.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigurePlugins_IHostBuilder_WithUnloadedFrameworkAssembly_LoadsAndScansAssembly()
    {
        var configuredPlugins = new List<string>();
        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var assemblyPath = CopyCurrentAssemblyToTemporaryPlugin(tempDirectory);
            var hostBuilder = Host.CreateDefaultBuilder();

            _ = hostBuilder.ConfigurePlugins(builder =>
            {
                ArgumentNullException.ThrowIfNull(builder);
                builder.FrameworkDirectories.Add(Path.GetDirectoryName(assemblyPath)!);
                _ = builder.FrameworkMatcher.AddInclude(Path.GetFileName(assemblyPath));
                builder.AssemblyScanFunc = _ => [new EarlierReactiveRecordingPlugin(configuredPlugins)];
            });

            using var host = hostBuilder.Build();

            await Assert.That(configuredPlugins.Count).IsEqualTo(1);
            await Assert.That(configuredPlugins[0]).IsEqualTo(EarlierReactiveRecordingPlugin.Name);
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

    /// <summary>Verifies that plugin assemblies not yet loaded in the default context are loaded and scanned.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigurePlugins_IHostBuilder_WithUnloadedPluginAssembly_LoadsAndScansAssembly()
    {
        var configuredPlugins = new List<string>();
        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var assemblyPath = CopyCurrentAssemblyToTemporaryPlugin(tempDirectory);
            var hostBuilder = Host.CreateDefaultBuilder();

            _ = hostBuilder.ConfigurePlugins(builder =>
            {
                ArgumentNullException.ThrowIfNull(builder);
                builder.PluginDirectories.Add(Path.GetDirectoryName(assemblyPath)!);
                _ = builder.PluginMatcher.AddInclude(Path.GetFileName(assemblyPath));
                builder.AssemblyScanFunc = _ => [new EarlierReactiveRecordingPlugin(configuredPlugins)];
            });

            using var host = hostBuilder.Build();

            await Assert.That(configuredPlugins.Count).IsEqualTo(1);
            await Assert.That(configuredPlugins[0]).IsEqualTo(EarlierReactiveRecordingPlugin.Name);
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

    /// <summary>Verifies that required plugin configuration fails when no plugins are discovered.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigurePlugins_IHostBuilder_WithRequiredPluginsAndNoPlugins_ThrowsInvalidOperationException()
    {
        var hostBuilder = Host.CreateDefaultBuilder();
        _ = hostBuilder.ConfigurePlugins(builder => builder.RequirePlugins());

        var act = () => hostBuilder.Build();
        await Assert.That(act).Throws<InvalidOperationException>();
    }

    /// <summary>Adds the current test assembly to the framework scan set.</summary>
    /// <param name="pluginBuilder">The plugin builder to configure.</param>
    private static void AddCurrentAssemblyAsFramework(IPluginBuilder pluginBuilder)
    {
        var assemblyPath = typeof(ReactivePluginShimTests).Assembly.Location;
        pluginBuilder.FrameworkDirectories.Add(Path.GetDirectoryName(assemblyPath)!);
        _ = pluginBuilder.FrameworkMatcher.AddInclude(Path.GetFileName(assemblyPath));
    }

    /// <summary>Creates a temporary directory for plugin scanning tests.</summary>
    /// <returns>The created temporary directory.</returns>
    private static string CreateTemporaryDirectory()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), "Extensions.Hosting.Tests", Guid.NewGuid().ToString("N"));
        _ = Directory.CreateDirectory(tempDirectory);
        return tempDirectory;
    }

    /// <summary>Copies the current test assembly to a unique plugin assembly path.</summary>
    /// <param name="tempDirectory">The temporary directory to copy into.</param>
    /// <returns>The copied assembly path.</returns>
    private static string CopyCurrentAssemblyToTemporaryPlugin(string tempDirectory)
    {
        var pluginPath = Path.Combine(tempDirectory, $"{nameof(Plugin)}.{Guid.NewGuid():N}.dll");
        File.Copy(typeof(ReactivePluginShimTests).Assembly.Location, pluginPath);
        return pluginPath;
    }

    /// <summary>Returns a value indicating whether the descriptor registers the reactive hosted service.</summary>
    /// <param name="serviceDescriptor">The service descriptor to inspect.</param>
    /// <returns>True when the descriptor registers the reactive hosted service.</returns>
    private static bool IsReactiveHostedServiceRegistration(ServiceDescriptor serviceDescriptor) =>
        serviceDescriptor.ServiceType == typeof(IHostedService) &&
        serviceDescriptor.ImplementationType == typeof(ReactiveHostedService);

    /// <summary>Hosted service used to verify reactive shim service registration.</summary>
    public sealed class ReactiveHostedService : IHostedService
    {
        /// <inheritdoc />
        public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    /// <summary>Reactive plugin implementation discovered by the reactive shim scanner.</summary>
    public sealed class ReactiveDiscoveredPlugin : IPlugin
    {
        /// <inheritdoc />
        public void ConfigureHost(object hostBuilderContext, IServiceCollection serviceCollection)
        {
        }
    }

    /// <summary>Records configuration with an earlier reactive plugin order.</summary>
    /// <param name="configuredPlugins">The configured plugin log.</param>
    [PluginOrder(-1)]
    private sealed class EarlierReactiveRecordingPlugin(List<string> configuredPlugins) : IPlugin
    {
        /// <summary>Stores the plugin name.</summary>
        public const string Name = "EarlierReactive";

        /// <inheritdoc />
        public void ConfigureHost(object hostBuilderContext, IServiceCollection serviceCollection) =>
            configuredPlugins.Add(Name);
    }

    /// <summary>Records configuration with a later reactive plugin order.</summary>
    /// <param name="configuredPlugins">The configured plugin log.</param>
    [PluginOrder(10)]
    private sealed class LaterReactiveRecordingPlugin(List<string> configuredPlugins) : IPlugin
    {
        /// <summary>Stores the plugin name.</summary>
        public const string Name = "LaterReactive";

        /// <inheritdoc />
        public void ConfigureHost(object hostBuilderContext, IServiceCollection serviceCollection) =>
            configuredPlugins.Add(Name);
    }
}
