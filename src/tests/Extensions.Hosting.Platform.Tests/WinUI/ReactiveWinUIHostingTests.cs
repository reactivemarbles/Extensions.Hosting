// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Extensions.Hosting;
using Microsoft.UI.Dispatching;
#if REACTIVE_SHIM
using ReactiveMarbles.Extensions.Hosting.Reactive.ReactiveUI;
#else
using ReactiveMarbles.Extensions.Hosting.ReactiveUI;
#endif

namespace Extensions.Hosting.WinUI.Platform.Tests;

/// <summary>Tests ReactiveUI WinUI hosting configuration without starting a WinUI application.</summary>
public class ReactiveWinUIHostingTests
{
    /// <summary>Verifies that WinUI ReactiveUI configuration succeeds with an unpackaged dispatcher queue.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    [NotInParallel]
    public async Task ConfigureSplatForMicrosoftDependencyResolver_WithDispatcherQueue_ReturnsApplicationBuilder()
    {
        var dispatcherQueueController = EnsureDispatcherQueue();
        var builder = Host.CreateApplicationBuilder();

        var result = builder.ConfigureSplatForMicrosoftDependencyResolver();

        await Assert.That(DispatcherQueue.GetForCurrentThread()).IsNotNull();
        await Assert.That(ReferenceEquals(result, builder)).IsTrue();
        GC.KeepAlive(dispatcherQueueController);
    }

    /// <summary>Verifies that legacy host-builder configuration is applied while the host is built.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    [NotInParallel]
    public async Task ConfigureSplatForMicrosoftDependencyResolver_WithHostBuilder_ReturnsConfiguredBuilder()
    {
        var dispatcherQueueController = EnsureDispatcherQueue();
        var hostBuilder = Host.CreateDefaultBuilder();

        var configuredBuilder = hostBuilder.ConfigureSplatForMicrosoftDependencyResolver();
        using var host = hostBuilder.Build();

        await Assert.That(DispatcherQueue.GetForCurrentThread()).IsNotNull();
        await Assert.That(configuredBuilder).IsSameReferenceAs(hostBuilder);
        await Assert.That(host.Services).IsNotNull();
        GC.KeepAlive(dispatcherQueueController);
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

    /// <summary>Verifies that null application builders are rejected before any ReactiveUI registration occurs.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureSplatForMicrosoftDependencyResolver_WithNullApplicationBuilder_ThrowsArgumentNullException()
    {
        IHostApplicationBuilder? builder = null;

        await Assert.That(() => builder!.ConfigureSplatForMicrosoftDependencyResolver()).Throws<ArgumentNullException>();
    }

    /// <summary>Creates a dispatcher queue for the current test thread when one is not already present.</summary>
    /// <returns>The controller that owns a newly created queue, or <see langword="null"/> when the thread already has a queue.</returns>
    private static DispatcherQueueController? EnsureDispatcherQueue() =>
        DispatcherQueue.GetForCurrentThread() is null ? DispatcherQueueController.CreateOnCurrentThread() : null;
}
