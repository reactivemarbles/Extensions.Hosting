// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactiveMarbles.Extensions.Hosting.Plugins;

namespace Extensions.Hosting.Tests;

/// <summary>Contains tests for host builder plugin configuration extensions.</summary>
public class HostBuilderPluginExtensionsTests
{
    /// <summary>The expected number of configured plugins.</summary>
    private const int ExpectedPluginCount = 2;

    /// <summary>The expected number of ordered explicit plugin entries.</summary>
    private const int ExpectedOrderedPluginCount = 3;

    /// <summary>The explicit plugin name configured before scanning.</summary>
    private const string BeforePluginName = "Before";

    /// <summary>The explicit plugin name configured after scanning.</summary>
    private const string AfterPluginName = "After";

    /// <summary>The discovered plugin name configured during scanning.</summary>
    private const string ScannedPluginName = "Scanned";

    /// <summary>Verifies that ConfigurePlugins with a null IHostBuilder throws.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigurePlugins_IHostBuilder_WithNullHostBuilder_ThrowsArgumentNullException()
    {
        IHostBuilder? hostBuilder = null;
        var act = () => hostBuilder!.ConfigurePlugins(static _ => { });
        await Assert.That(act).Throws<ArgumentNullException>();
    }

    /// <summary>Verifies that ConfigurePlugins with a null IHostApplicationBuilder throws.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigurePlugins_IHostApplicationBuilder_WithNullHostBuilder_ThrowsArgumentNullException()
    {
        IHostApplicationBuilder? hostBuilder = null;
        var act = () => hostBuilder!.ConfigurePlugins(static _ => { });
        await Assert.That(act).Throws<ArgumentNullException>();
    }

    /// <summary>Verifies that ConfigurePlugin with a null IHostBuilder throws.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigurePlugin_IHostBuilder_WithNullHostBuilder_ThrowsArgumentNullException()
    {
        IHostBuilder? hostBuilder = null;
        var plugin = new ContextRecordingPlugin();
        var act = () => hostBuilder!.ConfigurePlugin(plugin);
        await Assert.That(act).Throws<ArgumentNullException>();
    }

    /// <summary>Verifies that ConfigurePlugin with a null IHostBuilder plugin throws.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigurePlugin_IHostBuilder_WithNullPlugin_ThrowsArgumentNullException()
    {
        var hostBuilder = Host.CreateDefaultBuilder();
        IPlugin? plugin = null;
        var act = () => hostBuilder.ConfigurePlugin(plugin!);
        await Assert.That(act).Throws<ArgumentNullException>();
    }

    /// <summary>Verifies that ConfigurePlugin with a null IHostApplicationBuilder throws.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigurePlugin_IHostApplicationBuilder_WithNullHostBuilder_ThrowsArgumentNullException()
    {
        IHostApplicationBuilder? hostBuilder = null;
        var plugin = new ContextRecordingPlugin();
        var act = () => hostBuilder!.ConfigurePlugin(plugin);
        await Assert.That(act).Throws<ArgumentNullException>();
    }

    /// <summary>Verifies that ConfigurePlugin with a null IHostApplicationBuilder plugin throws.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigurePlugin_IHostApplicationBuilder_WithNullPlugin_ThrowsArgumentNullException()
    {
        var hostBuilder = Host.CreateApplicationBuilder();
        IPlugin? plugin = null;
        var act = () => hostBuilder.ConfigurePlugin(plugin!);
        await Assert.That(act).Throws<ArgumentNullException>();
    }

    /// <summary>Verifies that IHostBuilder ConfigurePlugin configures the caller-owned plugin during host build.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigurePlugin_IHostBuilder_ConfiguresCallerOwnedPlugin()
    {
        var plugin = new ContextRecordingPlugin();
        var hostBuilder = Host.CreateDefaultBuilder();

        const int expectedConfigurationCount = 2;
        var result = hostBuilder.ConfigurePlugin(plugin);
        _ = hostBuilder.ConfigurePlugin(plugin);

        await Assert.That(result).IsEqualTo(hostBuilder);
        await Assert.That(plugin.ConfigureCount).IsEqualTo(0);

        using var host = hostBuilder.Build();

        await Assert.That(plugin.ConfigureCount).IsEqualTo(expectedConfigurationCount);
        await Assert.That(plugin.Context).IsAssignableTo<HostBuilderContext>();
        await Assert.That(host.Services.GetService<ExplicitPluginMarker>()).IsNotNull();
    }

    /// <summary>Verifies that IHostApplicationBuilder ConfigurePlugin configures the caller-owned plugin immediately.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigurePlugin_IHostApplicationBuilder_ConfiguresCallerOwnedPlugin()
    {
        var plugin = new ContextRecordingPlugin();
        var hostBuilder = Host.CreateApplicationBuilder();

        const int expectedConfigurationCount = 2;
        var result = hostBuilder.ConfigurePlugin(plugin);
        _ = hostBuilder.ConfigurePlugin(plugin);

        using var host = hostBuilder.Build();

        await Assert.That(result).IsEqualTo(hostBuilder);
        await Assert.That(plugin.ConfigureCount).IsEqualTo(expectedConfigurationCount);
        await Assert.That(plugin.Context).IsEqualTo(hostBuilder);
        await Assert.That(host.Services.GetService<ExplicitPluginMarker>()).IsNotNull();
    }

    /// <summary>Verifies that explicit IHostBuilder plugins follow host builder configuration call order.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigurePlugin_IHostBuilder_PreservesCallOrderWithScannedPlugins()
    {
        var configuredPlugins = new List<string>();
        var hostBuilder = Host.CreateDefaultBuilder();

        _ = hostBuilder.ConfigurePlugin(new OrderedExplicitPlugin(BeforePluginName, configuredPlugins));
        _ = hostBuilder.ConfigurePlugins(builder =>
        {
            _ = builder ?? throw new ArgumentNullException(nameof(builder));
            AddCurrentAssemblyAsFramework(builder);
            builder.AssemblyScanFunc = _ => [new OrderedExplicitPlugin(ScannedPluginName, configuredPlugins)];
        });
        _ = hostBuilder.ConfigurePlugin(new OrderedExplicitPlugin(AfterPluginName, configuredPlugins));

        using var host = hostBuilder.Build();

        await Assert.That(configuredPlugins.Count).IsEqualTo(ExpectedOrderedPluginCount);
        await Assert.That(configuredPlugins[0]).IsEqualTo(BeforePluginName);
        await Assert.That(configuredPlugins[1]).IsEqualTo(ScannedPluginName);
        await Assert.That(configuredPlugins[2]).IsEqualTo(AfterPluginName);
    }

    /// <summary>Verifies that explicit IHostApplicationBuilder plugins follow configuration call order.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigurePlugin_IHostApplicationBuilder_PreservesCallOrderWithScannedPlugins()
    {
        var configuredPlugins = new List<string>();
        var hostBuilder = Host.CreateApplicationBuilder();

        _ = hostBuilder.ConfigurePlugin(new OrderedExplicitPlugin(BeforePluginName, configuredPlugins));
        _ = hostBuilder.ConfigurePlugins(builder =>
        {
            _ = builder ?? throw new ArgumentNullException(nameof(builder));
            AddCurrentAssemblyAsFramework(builder);
            builder.AssemblyScanFunc = _ => [new OrderedExplicitPlugin(ScannedPluginName, configuredPlugins)];
        });
        _ = hostBuilder.ConfigurePlugin(new OrderedExplicitPlugin(AfterPluginName, configuredPlugins));

        await Assert.That(configuredPlugins.Count).IsEqualTo(ExpectedOrderedPluginCount);
        await Assert.That(configuredPlugins[0]).IsEqualTo(BeforePluginName);
        await Assert.That(configuredPlugins[1]).IsEqualTo(ScannedPluginName);
        await Assert.That(configuredPlugins[2]).IsEqualTo(AfterPluginName);
    }

    /// <summary>Verifies that IHostBuilder plugin configuration scans and configures ordered plugins.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigurePlugins_IHostBuilder_LoadsConfiguredPluginsInOrder()
    {
        var configuredPlugins = new List<string>();
        var hostBuilder = Host.CreateDefaultBuilder();

        _ = hostBuilder.ConfigurePlugins(builder =>
        {
            _ = builder ?? throw new ArgumentNullException(nameof(builder));
            AddCurrentAssemblyAsFramework(builder);
            builder.AssemblyScanFunc = _ =>
            [
                new LaterRecordingPlugin(configuredPlugins),
                null,
                new EarlierRecordingPlugin(configuredPlugins),
            ];
        });

        using var host = hostBuilder.Build();

        await Assert.That(configuredPlugins.Count).IsEqualTo(ExpectedPluginCount);
        await Assert.That(configuredPlugins[0]).IsEqualTo(EarlierRecordingPlugin.Name);
        await Assert.That(configuredPlugins[1]).IsEqualTo(LaterRecordingPlugin.Name);
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
            _ = builder ?? throw new ArgumentNullException(nameof(builder));
            AddCurrentAssemblyAsFramework(builder);
            builder.AssemblyScanFunc = _ => [new EarlierRecordingPlugin(configuredPlugins)];
        });

        await Assert.That(result).IsEqualTo(hostBuilder);
        await Assert.That(configuredPlugins.Count).IsEqualTo(1);
        await Assert.That(configuredPlugins[0]).IsEqualTo(EarlierRecordingPlugin.Name);
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

    /// <summary>Verifies that content-root scanning can provide the assembly used for plugin discovery.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigurePlugins_IHostBuilder_WithContentRootScan_LoadsConfiguredPlugin()
    {
        var configuredPlugins = new List<string>();
        var assemblyPath = typeof(HostBuilderPluginExtensionsTests).Assembly.Location;
        var hostBuilder = Host.CreateDefaultBuilder()
            .UseContentRoot(Path.GetDirectoryName(assemblyPath)!);

        _ = hostBuilder.ConfigurePlugins(builder =>
        {
            _ = builder ?? throw new ArgumentNullException(nameof(builder));
            builder.UseContentRoot = true;
            _ = builder.FrameworkMatcher.AddInclude(Path.GetFileName(assemblyPath));
            builder.AssemblyScanFunc = _ => [new EarlierRecordingPlugin(configuredPlugins)];
        });

        using var host = hostBuilder.Build();

        await Assert.That(configuredPlugins.Count).IsEqualTo(1);
        await Assert.That(configuredPlugins[0]).IsEqualTo(EarlierRecordingPlugin.Name);
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
                _ = builder ?? throw new ArgumentNullException(nameof(builder));
                builder.PluginDirectories.Add(tempDirectory);
                _ = builder.PluginMatcher.AddInclude(Path.GetFileName(pluginPath));
                builder.ValidatePlugin = static _ => false;
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
        var assemblyPath = typeof(HostBuilderPluginExtensionsTests).Assembly.Location;
        var hostBuilder = Host.CreateDefaultBuilder();

        _ = hostBuilder.ConfigurePlugins(builder =>
        {
            _ = builder ?? throw new ArgumentNullException(nameof(builder));
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
                _ = builder ?? throw new ArgumentNullException(nameof(builder));
                builder.FrameworkDirectories.Add(Path.GetDirectoryName(assemblyPath)!);
                _ = builder.FrameworkMatcher.AddInclude(Path.GetFileName(assemblyPath));
                builder.AssemblyScanFunc = _ => [new EarlierRecordingPlugin(configuredPlugins)];
            });

            using var host = hostBuilder.Build();

            await Assert.That(configuredPlugins.Count).IsEqualTo(1);
            await Assert.That(configuredPlugins[0]).IsEqualTo(EarlierRecordingPlugin.Name);
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
        var assemblyPath = Path.Combine(
            AppContext.BaseDirectory,
            "ExternalPluginFixtures",
            "normal",
            "Extensions.Hosting.PluginLoading.Fixture.dll");
        string? loadedAssemblyLocation = null;
        var hostBuilder = Host.CreateDefaultBuilder();

        _ = hostBuilder.ConfigurePlugins(builder =>
        {
            _ = builder ?? throw new ArgumentNullException(nameof(builder));
            builder.PluginDirectories.Add(Path.GetDirectoryName(assemblyPath)!);
            _ = builder.PluginMatcher.AddInclude(Path.GetFileName(assemblyPath));
            builder.AssemblyScanFunc = assembly =>
            {
                loadedAssemblyLocation = assembly.Location;
                return [new EarlierRecordingPlugin(configuredPlugins)];
            };
        });

        using var host = hostBuilder.Build();

        await Assert.That(configuredPlugins.Count).IsEqualTo(1);
        await Assert.That(configuredPlugins[0]).IsEqualTo(EarlierRecordingPlugin.Name);
        await Assert.That(Path.GetFullPath(loadedAssemblyLocation!)).IsEqualTo(Path.GetFullPath(assemblyPath));
    }

    /// <summary>Verifies that required plugin configuration fails when no plugins are discovered.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigurePlugins_IHostBuilder_WithRequiredPluginsAndNoPlugins_ThrowsInvalidOperationException()
    {
        var hostBuilder = Host.CreateDefaultBuilder();
        _ = hostBuilder.ConfigurePlugins(
            static builder => (builder ?? throw new InvalidOperationException("Plugin builder was not created."))
                .RequirePlugins());

        var act = () => hostBuilder.Build();
        await Assert.That(act).Throws<InvalidOperationException>();
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

    /// <summary>Adds the current test assembly to the framework scan set.</summary>
    /// <param name="pluginBuilder">The plugin builder to configure.</param>
    private static void AddCurrentAssemblyAsFramework(IPluginBuilder pluginBuilder)
    {
        var assemblyPath = typeof(HostBuilderPluginExtensionsTests).Assembly.Location;
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
        File.Copy(typeof(HostBuilderPluginExtensionsTests).Assembly.Location, pluginPath);
        return pluginPath;
    }

    /// <summary>Marker service registered by explicit plugin tests.</summary>
    /// <param name="name">The marker name.</param>
    private sealed class ExplicitPluginMarker(string name)
    {
        /// <summary>Gets the marker name.</summary>
        public string Name { get; } = name;
    }

    /// <summary>Records explicit plugin configuration context.</summary>
    private sealed class ContextRecordingPlugin : IPlugin
    {
        /// <summary>Gets the number of times the plugin was configured.</summary>
        public int ConfigureCount { get; private set; }

        /// <summary>Gets the context passed to the plugin.</summary>
        public object? Context { get; private set; }

        /// <inheritdoc />
        public void ConfigureHost(object hostBuilderContext, IServiceCollection serviceCollection)
        {
            ConfigureCount++;
            Context = hostBuilderContext;
            _ = serviceCollection.AddSingleton(new ExplicitPluginMarker(nameof(ExplicitPluginMarker)));
        }
    }

    /// <summary>Records explicit plugin call order.</summary>
    /// <param name="name">The configured plugin name.</param>
    /// <param name="configuredPlugins">The configured plugin log.</param>
    private sealed class OrderedExplicitPlugin(string name, List<string> configuredPlugins) : IPlugin
    {
        /// <inheritdoc />
        public void ConfigureHost(object hostBuilderContext, IServiceCollection serviceCollection) =>
            configuredPlugins.Add(name);
    }

    /// <summary>Records configuration with an earlier plugin order.</summary>
    /// <param name="configuredPlugins">The configured plugin log.</param>
    [PluginOrder(-1)]
    private sealed class EarlierRecordingPlugin(List<string> configuredPlugins) : IPlugin
    {
        /// <summary>Stores the plugin name.</summary>
        public const string Name = "Earlier";

        /// <inheritdoc />
        public void ConfigureHost(object hostBuilderContext, IServiceCollection serviceCollection) =>
            configuredPlugins.Add(Name);
    }

    /// <summary>Records configuration with a later plugin order.</summary>
    /// <param name="configuredPlugins">The configured plugin log.</param>
    [PluginOrder(10)]
    private sealed class LaterRecordingPlugin(List<string> configuredPlugins) : IPlugin
    {
        /// <summary>Stores the plugin name.</summary>
        public const string Name = "Later";

        /// <inheritdoc />
        public void ConfigureHost(object hostBuilderContext, IServiceCollection serviceCollection) =>
            configuredPlugins.Add(Name);
    }
}
