// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using ReactiveMarbles.Extensions.Hosting.Plugins;
using TUnit;

namespace Extensions.Hosting.Tests;

/// <summary>
/// Contains unit tests for the PluginScanner class, verifying its behavior when scanning for plugin instances and
/// handling invalid input.
/// </summary>
/// <remarks>These tests ensure that PluginScanner methods correctly handle null arguments and return expected
/// results when no plugins are found. The class uses the TUnit testing framework.</remarks>
public class PluginScannerTests
{
    /// <summary>
    /// Verifies that ScanForPluginInstances throws an ArgumentNullException when passed a null argument.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ScanForPluginInstances_WithNull_ThrowsArgumentNullException()
    {
        static System.Collections.Generic.IEnumerable<IPlugin> Act() => PluginScanner.ScanForPluginInstances(null!);
        await Assert.That(Act).Throws<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that ByNamingConvention throws an ArgumentNullException when passed a null argument.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ByNamingConvention_WithNull_ThrowsArgumentNullException()
    {
        static System.Collections.Generic.IEnumerable<IPlugin> Act() => PluginScanner.ByNamingConvention(null!);
        await Assert.That(Act).Throws<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that the plugin scanner returns an empty collection when no plugins are found by naming convention in
    /// the current assembly.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ByNamingConvention_FindsNoPlugin_ReturnsEmpty()
    {
        var assembly = typeof(PluginScannerTests).Assembly;
        var plugins = PluginScanner.ByNamingConvention(assembly);
        await Assert.That(plugins).IsNotNull();
        await Assert.That(plugins.Any()).IsFalse();
    }

    /// <summary>
    /// Verifies that ScanForPluginInstances returns a non-null collection for a valid assembly.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ScanForPluginInstances_WithValidAssembly_ReturnsNonNullCollection()
    {
        var assembly = typeof(PluginScannerTests).Assembly;
        var plugins = PluginScanner.ScanForPluginInstances(assembly);
        await Assert.That(plugins).IsNotNull();
    }

    /// <summary>
    /// Verifies that ScanForPluginInstances discovers the TestPlugin class.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ScanForPluginInstances_FindsTestPlugin()
    {
        var assembly = typeof(TestPlugin).Assembly;
        var plugins = PluginScanner.ScanForPluginInstances(assembly).ToList();
        await Assert.That(plugins.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(plugins.Any(p => p is TestPlugin)).IsTrue();
    }

    /// <summary>
    /// Verifies that ScanForPluginInstances does not include abstract plugin classes.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ScanForPluginInstances_DoesNotIncludeAbstractPlugins()
    {
        var assembly = typeof(AbstractTestPlugin).Assembly;
        var plugins = PluginScanner.ScanForPluginInstances(assembly).ToList();
        await Assert.That(plugins.Any(p => p.GetType() == typeof(AbstractTestPlugin))).IsFalse();
    }

    /// <summary>
    /// Verifies that the test plugin can be configured via ConfigureHost.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task TestPlugin_ConfigureHost_DoesNotThrow()
    {
        var plugin = new TestPlugin();
        var serviceCollection = new ServiceCollection();

        Exception? exception = null;
        try
        {
            plugin.ConfigureHost(new object(), serviceCollection);
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        await Assert.That(exception).IsNull();
    }
}
