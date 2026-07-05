// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactiveMarbles.Extensions.Hosting.Plugins;

namespace Extensions.Hosting.Tests;

/// <summary>Contains tests for host builder plugin configuration extensions.</summary>
public class HostBuilderPluginExtensionsTests
{
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

    /// <summary>Verifies that IHostBuilder plugin configuration scans and configures ordered plugins.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigurePlugins_IHostBuilder_LoadsConfiguredPluginsInOrder()
    {
        var configuredPlugins = new List<string>();
        var hostBuilder = Host.CreateDefaultBuilder();

        _ = hostBuilder.ConfigurePlugins(builder =>
        {
            ArgumentNullException.ThrowIfNull(builder);
            AddCurrentAssemblyAsFramework(builder);
            builder.AssemblyScanFunc = _ =>
            [
                new LaterRecordingPlugin(configuredPlugins),
                null,
                new EarlierRecordingPlugin(configuredPlugins),
            ];
        });

        using var host = hostBuilder.Build();

        await Assert.That(configuredPlugins.Count).IsEqualTo(2);
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
            ArgumentNullException.ThrowIfNull(builder);
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
            ArgumentNullException.ThrowIfNull(builder);
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
        var assemblyPath = typeof(HostBuilderPluginExtensionsTests).Assembly.Location;
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
        var assemblyPath = GetUnloadedAssemblyPath();
        var assemblyName = Path.GetFileNameWithoutExtension(assemblyPath);
        var hostBuilder = Host.CreateDefaultBuilder();

        _ = hostBuilder.ConfigurePlugins(builder =>
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.FrameworkDirectories.Add(Path.GetDirectoryName(assemblyPath)!);
            _ = builder.FrameworkMatcher.AddInclude(Path.GetFileName(assemblyPath));
            builder.AssemblyScanFunc = assembly =>
                assembly.GetName().Name == assemblyName
                    ? [new EarlierRecordingPlugin(configuredPlugins)]
                    : [];
        });

        using var host = hostBuilder.Build();

        await Assert.That(configuredPlugins.Count).IsEqualTo(1);
        await Assert.That(configuredPlugins[0]).IsEqualTo(EarlierRecordingPlugin.Name);
    }

    /// <summary>Verifies that plugin assemblies not yet loaded in the default context are loaded and scanned.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigurePlugins_IHostBuilder_WithUnloadedPluginAssembly_LoadsAndScansAssembly()
    {
        var configuredPlugins = new List<string>();
        var assemblyPath = GetUnloadedAssemblyPath();
        var assemblyName = Path.GetFileNameWithoutExtension(assemblyPath);
        var hostBuilder = Host.CreateDefaultBuilder();

        _ = hostBuilder.ConfigurePlugins(builder =>
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.PluginDirectories.Add(Path.GetDirectoryName(assemblyPath)!);
            _ = builder.PluginMatcher.AddInclude(Path.GetFileName(assemblyPath));
            builder.AssemblyScanFunc = assembly =>
                assembly.GetName().Name == assemblyName
                    ? [new EarlierRecordingPlugin(configuredPlugins)]
                    : [];
        });

        using var host = hostBuilder.Build();

        await Assert.That(configuredPlugins.Count).IsEqualTo(1);
        await Assert.That(configuredPlugins[0]).IsEqualTo(EarlierRecordingPlugin.Name);
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

    /// <summary>Finds a managed assembly in the test output that has not yet been loaded.</summary>
    /// <returns>The path to an unloaded managed assembly.</returns>
    private static string GetUnloadedAssemblyPath()
    {
        var assemblyDirectory = Path.GetDirectoryName(typeof(HostBuilderPluginExtensionsTests).Assembly.Location)!;
        var loadedAssemblyNames = AppDomain.CurrentDomain.GetAssemblies()
            .Select(assembly => assembly.GetName().Name)
            .Where(name => name is not null)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var assemblyPath in Directory.EnumerateFiles(assemblyDirectory, "*.dll"))
        {
            var assemblyName = Path.GetFileNameWithoutExtension(assemblyPath);
            if (!loadedAssemblyNames.Contains(assemblyName) &&
                !assemblyName.StartsWith("Extensions.Hosting", StringComparison.OrdinalIgnoreCase))
            {
                return assemblyPath;
            }
        }

        throw new InvalidOperationException("No unloaded assembly was available in the test output directory.");
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
