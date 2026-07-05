// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using ReactiveMarbles.Extensions.Hosting.Reactive.Plugins;

namespace Extensions.Hosting.Tests;

/// <summary>Contains unit tests for the reactive shim PluginScanner class.</summary>
public class ReactivePluginScannerTests
{
    /// <summary>Verifies that ScanForPluginInstances throws an ArgumentNullException when passed a null argument.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ScanForPluginInstances_WithNull_ThrowsArgumentNullException()
    {
        static IEnumerable<IPlugin> Act() => PluginScanner.ScanForPluginInstances(null!);
        await Assert.That(Act).Throws<ArgumentNullException>();
    }

    /// <summary>Verifies that ByNamingConvention throws an ArgumentNullException when passed a null argument.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ByNamingConvention_WithNull_ThrowsArgumentNullException()
    {
        static IEnumerable<IPlugin> Act() => PluginScanner.ByNamingConvention(null!);
        await Assert.That(Act).Throws<ArgumentNullException>();
    }

    /// <summary>Verifies that naming convention scanning returns empty when no conventional plugin exists.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ByNamingConvention_FindsNoPlugin_ReturnsEmpty()
    {
        var plugins = PluginScanner.ByNamingConvention(typeof(string).Assembly);

        await Assert.That(plugins).IsNotNull();
        await Assert.That(plugins.Any()).IsFalse();
    }

    /// <summary>Verifies that the reactive shim plugin scanner discovers a plugin by naming convention.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ByNamingConvention_FindsConventionalPlugin()
    {
        var plugins = PluginScanner.ByNamingConvention(typeof(Plugin).Assembly).ToList();

        await Assert.That(plugins.Any(plugin => plugin is Plugin)).IsTrue();
    }

    /// <summary>Verifies that ScanForPluginInstances returns a non-null collection for a valid assembly.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ScanForPluginInstances_WithValidAssembly_ReturnsNonNullCollection()
    {
        var plugins = PluginScanner.ScanForPluginInstances(typeof(ReactivePluginScannerTests).Assembly);

        await Assert.That(plugins).IsNotNull();
    }

    /// <summary>Verifies that ScanForPluginInstances discovers the reactive test plugin.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ScanForPluginInstances_FindsReactiveTestPlugin()
    {
        var plugins = PluginScanner.ScanForPluginInstances(typeof(ReactiveScannerTestPlugin).Assembly).ToList();

        await Assert.That(plugins.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(plugins.Any(plugin => plugin is ReactiveScannerTestPlugin)).IsTrue();
    }

    /// <summary>Verifies that ScanForPluginInstances does not include abstract reactive plugin classes.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ScanForPluginInstances_DoesNotIncludeAbstractPlugins()
    {
        var plugins = PluginScanner.ScanForPluginInstances(typeof(ReactiveAbstractTestPlugin).Assembly).ToList();

        await Assert.That(plugins.Any(plugin => plugin.GetType() == typeof(ReactiveAbstractTestPlugin))).IsFalse();
    }

    /// <summary>Verifies that the reactive test plugin can be configured via ConfigureHost.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveTestPlugin_ConfigureHost_SetsConfiguredFlag()
    {
        var plugin = new ReactiveScannerTestPlugin();
        var services = new ServiceCollection();

        plugin.ConfigureHost(new object(), services);

        await Assert.That(plugin.WasConfigured).IsTrue();
    }

    /// <summary>A reactive shim test plugin implementation for unit testing purposes.</summary>
    public class ReactiveScannerTestPlugin : IPlugin
    {
        /// <summary>Gets a value indicating whether ConfigureHost was called.</summary>
        public bool WasConfigured { get; private set; }

        /// <inheritdoc />
        public void ConfigureHost(object hostBuilderContext, IServiceCollection serviceCollection) => WasConfigured = true;
    }

    /// <summary>An abstract reactive shim plugin used to test that abstract classes are not discovered.</summary>
    public abstract class ReactiveAbstractTestPlugin : IPlugin
    {
        /// <inheritdoc />
        public abstract void ConfigureHost(object hostBuilderContext, IServiceCollection serviceCollection);
    }
}
