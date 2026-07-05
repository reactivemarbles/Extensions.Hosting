// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.IO;
using ReactiveMarbles.Extensions.Hosting.Reactive.Plugins;

namespace Extensions.Hosting.Tests;

/// <summary>Contains unit tests for the reactive shim PluginBuilderExtensions class.</summary>
public class ReactivePluginBuilderExtensionsTests
{
    /// <summary>Verifies that AddScanDirectories throws when directories is null.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task AddScanDirectories_WithNullDirectories_ThrowsArgumentNullException()
    {
        var builder = new ReactiveTestPluginBuilder();
        void Act() => PluginBuilderExtensions.AddScanDirectories(builder, null!);
        await Assert.That(Act).Throws<ArgumentNullException>();
    }

    /// <summary>Verifies that AddScanDirectories adds directories to both collections.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task AddScanDirectories_WithValidDirectories_AddsToBothCollections()
    {
        var builder = new ReactiveTestPluginBuilder();
        var testDir = Path.GetTempPath();

        builder.AddScanDirectories(testDir);

        await Assert.That(builder.PluginDirectories.Count).IsEqualTo(1);
        await Assert.That(builder.FrameworkDirectories.Count).IsEqualTo(1);
    }

    /// <summary>Verifies that multiple directories can be added.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task AddScanDirectories_MultipleDirectories_AddsAll()
    {
        var builder = new ReactiveTestPluginBuilder();
        var dir1 = Path.GetTempPath();
        var dir2 = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        builder.AddScanDirectories(dir1, dir2);

        await Assert.That(builder.PluginDirectories.Count).IsEqualTo(2);
        await Assert.That(builder.FrameworkDirectories.Count).IsEqualTo(2);
    }

    /// <summary>Verifies that ExcludeFrameworks throws when frameworkGlobs is null.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ExcludeFrameworks_WithNullGlobs_ThrowsArgumentNullException()
    {
        var builder = new ReactiveTestPluginBuilder();
        void Act() => PluginBuilderExtensions.ExcludeFrameworks(builder, null!);
        await Assert.That(Act).Throws<ArgumentNullException>();
    }

    /// <summary>Verifies that ExcludeFrameworks does not throw with valid input.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ExcludeFrameworks_WithValidGlobs_DoesNotThrow()
    {
        var builder = new ReactiveTestPluginBuilder();
        Exception? exception = null;

        try
        {
            builder.ExcludeFrameworks("**/bin/**");
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        await Assert.That(exception).IsNull();
    }

    /// <summary>Verifies that ExcludePlugins throws when pluginGlobs is null.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ExcludePlugins_WithNullGlobs_ThrowsArgumentNullException()
    {
        var builder = new ReactiveTestPluginBuilder();
        void Act() => PluginBuilderExtensions.ExcludePlugins(builder, null!);
        await Assert.That(Act).Throws<ArgumentNullException>();
    }

    /// <summary>Verifies that ExcludePlugins does not throw with valid input.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ExcludePlugins_WithValidGlobs_DoesNotThrow()
    {
        var builder = new ReactiveTestPluginBuilder();
        Exception? exception = null;

        try
        {
            builder.ExcludePlugins("**/test/**");
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        await Assert.That(exception).IsNull();
    }

    /// <summary>Verifies that IncludeFrameworks throws when frameworkGlobs is null.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task IncludeFrameworks_WithNullGlobs_ThrowsArgumentNullException()
    {
        var builder = new ReactiveTestPluginBuilder();
        void Act() => PluginBuilderExtensions.IncludeFrameworks(builder, null!);
        await Assert.That(Act).Throws<ArgumentNullException>();
    }

    /// <summary>Verifies that IncludeFrameworks does not throw with valid input.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task IncludeFrameworks_WithValidGlobs_DoesNotThrow()
    {
        var builder = new ReactiveTestPluginBuilder();
        Exception? exception = null;

        try
        {
            builder.IncludeFrameworks("**/*.dll");
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        await Assert.That(exception).IsNull();
    }

    /// <summary>Verifies that IncludePlugins throws when pluginGlobs is null.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task IncludePlugins_WithNullGlobs_ThrowsArgumentNullException()
    {
        var builder = new ReactiveTestPluginBuilder();
        void Act() => PluginBuilderExtensions.IncludePlugins(builder, null!);
        await Assert.That(Act).Throws<ArgumentNullException>();
    }

    /// <summary>Verifies that IncludePlugins does not throw with valid input.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task IncludePlugins_WithValidGlobs_DoesNotThrow()
    {
        var builder = new ReactiveTestPluginBuilder();
        Exception? exception = null;

        try
        {
            builder.IncludePlugins("**/*Plugin.dll");
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        await Assert.That(exception).IsNull();
    }

    /// <summary>Verifies that RequirePlugins throws when pluginBuilder is null.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task RequirePlugins_WithNullBuilder_ThrowsArgumentNullException()
    {
        static void Act() => PluginBuilderExtensions.RequirePlugins(null!);
        await Assert.That(Act).Throws<ArgumentNullException>();
    }

    /// <summary>Verifies that RequirePlugins sets FailIfNoPlugins to true by default.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task RequirePlugins_Default_SetsFailIfNoPluginsTrue()
    {
        var builder = new ReactiveTestPluginBuilder();
        builder.RequirePlugins();
        await Assert.That(builder.FailIfNoPlugins).IsTrue();
    }

    /// <summary>Verifies that RequirePlugins sets FailIfNoPlugins to false when specified.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task RequirePlugins_WithFalse_SetsFailIfNoPluginsFalse()
    {
        var builder = new ReactiveTestPluginBuilder
        {
            FailIfNoPlugins = true
        };

        builder.RequirePlugins(false);
        await Assert.That(builder.FailIfNoPlugins).IsFalse();
    }
}
