// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using ReactiveMarbles.Extensions.Hosting.AppServices.Internal;
using ReactiveMarbles.Extensions.Hosting.Plugins.Internals;
using ReactiveMarbles.Extensions.Hosting.Reactive.Plugins.Internals;
using PluginAssemblyLoadContext = ReactiveMarbles.Extensions.Hosting.Plugins.Internals.AssemblyLoadContext;
using ReactivePluginAssemblyLoadContext = ReactiveMarbles.Extensions.Hosting.Reactive.Plugins.Internals.AssemblyLoadContext;

namespace Extensions.Hosting.Tests;

/// <summary>Verifies argument validation at hosting and plugin-loading boundaries.</summary>
public sealed class ArgumentValidationCoverageTests
{
    /// <summary>Verifies a null assembly identity never resolves an arbitrary loaded assembly.</summary>
    /// <returns>A task representing the asynchronous assertions.</returns>
    [Test]
    public async Task AssemblyLoadContext_WithNullIdentity_DoesNotResolveAssembly()
    {
        var found = PluginAssemblyLoadContext.Default.TryGetAssembly(null!, out var assembly);
        var reactiveFound = ReactivePluginAssemblyLoadContext.Default.TryGetAssembly(null!, out var reactiveAssembly);

        await Assert.That(found).IsFalse();
        await Assert.That(assembly).IsNull();
        await Assert.That(reactiveFound).IsFalse();
        await Assert.That(reactiveAssembly).IsNull();
    }

    /// <summary>Verifies plugin resolution reuses a matching assembly from the default context.</summary>
    /// <returns>A task representing the asynchronous assertions.</returns>
    [Test]
    public async Task PluginLoadContext_ResolvesAlreadyLoadedAssembly()
    {
        var expected = typeof(ArgumentValidationCoverageTests).Assembly;
        var identity = expected.GetName();
        var context = new ReactiveMarbles.Extensions.Hosting.Plugins.Internals.PluginLoadContext(expected.Location, nameof(Plugin));
        var reactiveContext = new ReactiveMarbles.Extensions.Hosting.Reactive.Plugins.Internals.PluginLoadContext(expected.Location, nameof(Plugin));

        await Assert.That(context.TryLoadFromAssemblyName(identity)).IsEqualTo(expected);
        await Assert.That(reactiveContext.TryLoadFromAssemblyName(identity)).IsEqualTo(expected);
    }

    /// <summary>Verifies unannotated plugins receive the default order in both package variants.</summary>
    /// <returns>A task representing the asynchronous assertions.</returns>
    [Test]
    public async Task PluginOrdering_UnannotatedPlugins_PreservesDiscoveryOrder()
    {
        var first = new Plugin();
        var second = new Plugin();
        var assemblies = new HashSet<System.Reflection.Assembly> { typeof(Plugin).Assembly };
        var builder = new ReactiveMarbles.Extensions.Hosting.Plugins.Internals.PluginBuilder
        { AssemblyScanFunc = _ => [first, second] };
        var reactiveBuilder = new ReactiveMarbles.Extensions.Hosting.Reactive.Plugins.Internals.PluginBuilder
        { AssemblyScanFunc = _ => [first, second] };

        var plugins = ReactiveMarbles.Extensions.Hosting.Plugins.HostBuilderPluginExtensions.GetOrderedPlugins(builder, assemblies);
        var reactivePlugins = ReactiveMarbles.Extensions.Hosting.Reactive.Plugins.HostBuilderPluginExtensions.GetOrderedPlugins(reactiveBuilder, assemblies);

        await Assert.That(plugins[0]).IsEqualTo(first);
        await Assert.That(plugins[1]).IsEqualTo(second);
        await Assert.That(reactivePlugins[0]).IsEqualTo(first);
        await Assert.That(reactivePlugins[1]).IsEqualTo(second);
    }

    /// <summary>Verifies that every required mutex lifetime dependency is validated.</summary>
    /// <param name="parameterName">The dependency to omit.</param>
    /// <returns>A task representing the asynchronous assertions.</returns>
    [Test]
    [Arguments("logger")]
    [Arguments("hostEnvironment")]
    [Arguments("hostApplicationLifetime")]
    [Arguments("mutexBuilder")]
    public async Task MutexLifetimeService_RejectsMissingDependency(string parameterName)
    {
        using var host = Host.CreateApplicationBuilder().Build();
        var environment = host.Services.GetRequiredService<IHostEnvironment>();
        var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
        MutexLifetimeService CreateService() => new(
            parameterName == "logger" ? null! : NullLogger<MutexLifetimeService>.Instance,
            parameterName == "hostEnvironment" ? null! : environment,
            parameterName == "hostApplicationLifetime" ? null! : lifetime,
            parameterName == "mutexBuilder" ? null! : new MutexBuilder());

        await Assert.That(CreateService).Throws<ArgumentNullException>();
    }

    /// <summary>Verifies invalid paths are rejected before invoking either assembly loader.</summary>
    /// <param name="assemblyPath">The invalid assembly path.</param>
    /// <returns>A task representing the asynchronous assertions.</returns>
    [Test]
    [Arguments(null)]
    [Arguments("")]
    [Arguments(" ")]
    public async Task AssemblyLoadContext_RejectsInvalidPath(string? assemblyPath)
    {
        await Assert.That(() => PluginAssemblyLoadContext.LoadFromAssemblyPath(assemblyPath!)).Throws<ArgumentException>();
        await Assert.That(() => ReactivePluginAssemblyLoadContext.LoadFromAssemblyPath(assemblyPath!)).Throws<ArgumentException>();
    }

    /// <summary>Verifies a root path without a parent directory uses relative dependency lookup.</summary>
    /// <returns>A task representing the asynchronous assertions.</returns>
    [Test]
    public async Task AssemblyDependencyResolver_WithRootPath_ReturnsNullForMissingDependency()
    {
        var root = Path.GetPathRoot(AppContext.BaseDirectory)!;
        var missingAssembly = new System.Reflection.AssemblyName($"MissingDependency{Guid.NewGuid():N}");
        var resolver = new ReactiveMarbles.Extensions.Hosting.Plugins.Internals.AssemblyDependencyResolver(root);
        var reactiveResolver = new ReactiveMarbles.Extensions.Hosting.Reactive.Plugins.Internals.AssemblyDependencyResolver(root);

        await Assert.That(resolver.ResolveAssemblyToPath(missingAssembly)).IsNull();
        await Assert.That(reactiveResolver.ResolveAssemblyToPath(missingAssembly)).IsNull();
    }
}
