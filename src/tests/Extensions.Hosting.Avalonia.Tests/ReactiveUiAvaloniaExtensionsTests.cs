// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Extensions.Hosting;
using ReactiveMarbles.Extensions.Hosting.ReactiveUI;

namespace Extensions.Hosting.Avalonia.Tests;

/// <summary>Verifies ReactiveUI and Splat host integration.</summary>
[NotInParallel]
public sealed class ReactiveUiAvaloniaExtensionsTests
{
    /// <summary>Verifies a built host maps its service provider to Splat and returns itself.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task MapSplatLocator_WithHost_MapsServicesAndReturnsHost()
    {
        using var host = Host.CreateApplicationBuilder().Build();
        IServiceProvider? receivedProvider = null;

        var result = host.MapSplatLocator(provider => receivedProvider = provider);

        await Assert.That(result).IsSameReferenceAs(host);
        await Assert.That(receivedProvider).IsSameReferenceAs(host.Services);
    }

    /// <summary>Verifies mapping tolerates a null host and invokes the callback with null.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task MapSplatLocator_WithNullHost_InvokesCallbackWithNull()
    {
        IHost host = null!;
        IServiceProvider? receivedProvider = null;

        var result = host.MapSplatLocator(provider => receivedProvider = provider);

        await Assert.That(result).IsNull();
        await Assert.That(receivedProvider).IsNull();
    }

    /// <summary>Verifies application host builder configuration returns its receiver.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureSplatForMicrosoftDependencyResolver_WithApplicationBuilder_ReturnsBuilder()
    {
        var builder = Host.CreateApplicationBuilder();

        var result = builder.ConfigureSplatForMicrosoftDependencyResolver();

        await Assert.That(result).IsSameReferenceAs(builder);
    }

    /// <summary>Verifies host builder configuration executes while building its service container.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureSplatForMicrosoftDependencyResolver_WithHostBuilder_BuildsHost()
    {
        var builder = Host.CreateDefaultBuilder().ConfigureSplatForMicrosoftDependencyResolver();

        using var host = builder.Build();

        await Assert.That(host).IsNotNull();
    }
}
