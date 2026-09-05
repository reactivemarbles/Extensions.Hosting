// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Extensions.Hosting;
#if REACTIVE_SHIM
using ReactiveMarbles.Extensions.Hosting.Reactive.ReactiveUI;
#else
using ReactiveMarbles.Extensions.Hosting.ReactiveUI;
#endif

namespace Extensions.Hosting.Maui.Platform.Tests;

/// <summary>Tests ReactiveUI MAUI hosting configuration without starting a MAUI application.</summary>
[NotInParallel]
public class ReactiveMauiHostingTests
{
    /// <summary>Verifies that application-builder configuration preserves the original builder.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureSplatForMicrosoftDependencyResolver_ReturnsApplicationBuilder()
    {
        var builder = Host.CreateApplicationBuilder();

        var result = builder.ConfigureSplatForMicrosoftDependencyResolver();

        await Assert.That(ReferenceEquals(result, builder)).IsTrue();
    }

    /// <summary>Verifies that legacy host-builder configuration is applied while the host is built.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureSplatForMicrosoftDependencyResolver_WithHostBuilder_ReturnsConfiguredBuilder()
    {
        var hostBuilder = Host.CreateDefaultBuilder();

        var configuredBuilder = hostBuilder.ConfigureSplatForMicrosoftDependencyResolver();
        using var host = hostBuilder.Build();

        await Assert.That(configuredBuilder).IsSameReferenceAs(hostBuilder);
        await Assert.That(host.Services).IsNotNull();
    }

    /// <summary>Verifies that mapping a null host invokes the supplied factory with a null provider.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task MapSplatLocator_WithNullHost_InvokesFactoryWithNullProvider()
    {
        IServiceProvider? factoryProvider = new object() as IServiceProvider;

        var result = ((IHost)null!).MapSplatLocator(provider => factoryProvider = provider);

        await Assert.That(result).IsNull();
        await Assert.That(factoryProvider).IsNull();
    }

    /// <summary>Verifies that mapping a null host tolerates a null factory.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task MapSplatLocator_WithNullHostAndNullFactory_ReturnsNull()
    {
        var result = ((IHost)null!).MapSplatLocator(null!);

        await Assert.That(result).IsNull();
    }

    /// <summary>Verifies that mapping a configured host invokes the supplied factory with the service provider.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task MapSplatLocator_WithConfiguredHost_InvokesFactoryWithServices()
    {
        var hostBuilder = Host.CreateApplicationBuilder();
        _ = hostBuilder.ConfigureSplatForMicrosoftDependencyResolver();
        using var host = hostBuilder.Build();
        IServiceProvider? factoryProvider = null;

        var result = host.MapSplatLocator(provider => factoryProvider = provider);

        await Assert.That(result).IsSameReferenceAs(host);
        await Assert.That(factoryProvider).IsSameReferenceAs(host.Services);
    }

    /// <summary>Verifies that null application builders are rejected before any ReactiveUI registration occurs.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureSplatForMicrosoftDependencyResolver_WithNullApplicationBuilder_ThrowsArgumentNullException()
    {
        IHostApplicationBuilder? builder = null;

        await Assert.That(() => builder!.ConfigureSplatForMicrosoftDependencyResolver()).Throws<ArgumentNullException>();
    }
}
