// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Extensions.Hosting;
#if REACTIVE_SHIM
using ReactiveMarbles.Extensions.Hosting.Reactive.ReactiveUI;
#else
using ReactiveMarbles.Extensions.Hosting.ReactiveUI;
#endif

namespace Extensions.Hosting.WinForms.Tests;

/// <summary>Verifies ReactiveUI and Microsoft dependency-injection integration for Windows Forms hosts.</summary>
[NotInParallel]
public sealed class ReactiveUiWinFormsExtensionsTests
{
    /// <summary>Verifies that application builders reject a null receiver.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureSplatForMicrosoftDependencyResolver_ApplicationBuilderNull_ThrowsArgumentNullException()
    {
        IHostApplicationBuilder? hostBuilder = null;

        await Assert.That(() => hostBuilder!.ConfigureSplatForMicrosoftDependencyResolver()).Throws<ArgumentNullException>();
    }

    /// <summary>Verifies that application builders can initialize the Microsoft dependency resolver integration.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureSplatForMicrosoftDependencyResolver_ApplicationBuilder_ReturnsConfiguredBuilder()
    {
        var hostBuilder = Host.CreateApplicationBuilder();

        var configuredBuilder = hostBuilder.ConfigureSplatForMicrosoftDependencyResolver();

        using var host = hostBuilder.Build();
        await Assert.That(configuredBuilder).IsSameReferenceAs(hostBuilder);
        await Assert.That(host.Services).IsNotNull();
    }

    /// <summary>Verifies that legacy builders configure the dependency resolver integration.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureSplatForMicrosoftDependencyResolver_HostBuilder_ReturnsConfiguredBuilder()
    {
        var hostBuilder = Host.CreateDefaultBuilder();

        var configuredBuilder = hostBuilder.ConfigureSplatForMicrosoftDependencyResolver();

        using var host = hostBuilder.Build();
        await Assert.That(configuredBuilder).IsSameReferenceAs(hostBuilder);
        await Assert.That(host.Services).IsNotNull();
    }

    /// <summary>Verifies that mapping the Splat locator invokes the supplied container callback.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task MapSplatLocator_Host_MapsServicesAndInvokesFactory()
    {
        var hostBuilder = Host.CreateApplicationBuilder();
        _ = hostBuilder.ConfigureSplatForMicrosoftDependencyResolver();
        using var host = hostBuilder.Build();
        IServiceProvider? mappedProvider = null;

        var mappedHost = host!.MapSplatLocator(provider => mappedProvider = provider);

        await Assert.That(mappedHost).IsSameReferenceAs(host);
        await Assert.That(mappedProvider).IsSameReferenceAs(host.Services);
    }

    /// <summary>Verifies that mapping a null host retains the nullable extension contract.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task MapSplatLocator_NullHost_ReturnsNullAndInvokesFactoryWithNull()
    {
        IHost? host = null;
        IServiceProvider? mappedProvider = null;

        var mappedHost = host!.MapSplatLocator(provider => mappedProvider = provider);

        await Assert.That(mappedHost).IsNull();
        await Assert.That(mappedProvider).IsNull();
    }
}
