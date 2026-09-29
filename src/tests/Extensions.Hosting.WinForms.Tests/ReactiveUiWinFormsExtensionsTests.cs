// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactiveMarbles.Extensions.Hosting.WinForms;
#if REACTIVE_SHIM
using ReactiveMarbles.Extensions.Hosting.Reactive.ReactiveUI;
using ControlSequencer = ReactiveUI.Primitives.Reactive.Concurrency.ControlSequencer;
using RxSchedulers = ReactiveUI.Reactive.RxSchedulers;
#else
using ReactiveMarbles.Extensions.Hosting.ReactiveUI;
using ControlSequencer = ReactiveUI.Primitives.Concurrency.ControlSequencer;
using RxSchedulers = ReactiveUI.RxSchedulers;
#endif

namespace Extensions.Hosting.WinForms.Tests;

/// <summary>Verifies ReactiveUI and Microsoft dependency-injection integration for Windows Forms hosts.</summary>
[NotInParallel]
public sealed class ReactiveUiWinFormsExtensionsTests
{
    /// <summary>Gets the maximum duration allowed for hosted UI startup and shutdown.</summary>
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

    /// <summary>Verifies scheduler binding on the hosted UI thread before its message loop starts.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureSplatForMicrosoftDependencyResolver_HostedUiBindsWinFormsScheduler()
    {
        var originalScheduler = RxSchedulers.MainThreadScheduler;
        var captureService = new SchedulerCaptureService();
        var hostBuilder = Host.CreateApplicationBuilder();
        _ = hostBuilder.ConfigureSplatForMicrosoftDependencyResolver();
        _ = hostBuilder.ConfigureWinForms(static context => context.EnableVisualStyles = false);
        _ = hostBuilder.Services.AddSingleton<IWinFormsService>(captureService);
        using var host = hostBuilder.Build();

        try
        {
            await host.StartAsync().WaitAsync(Timeout);
            var schedulerIsBound = await captureService.SchedulerIsBound.Task.WaitAsync(Timeout);

            await Assert.That(schedulerIsBound).IsTrue();
        }
        finally
        {
            await host.StopAsync().WaitAsync(Timeout);
            RxSchedulers.MainThreadScheduler = originalScheduler;
        }
    }

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

    /// <summary>Captures the scheduler after the ReactiveUI service initializes on the UI thread.</summary>
    private sealed class SchedulerCaptureService : IWinFormsService
    {
        /// <summary>Gets the scheduler binding result.</summary>
        public TaskCompletionSource<bool> SchedulerIsBound { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        /// <inheritdoc />
        public void Initialize() => SchedulerIsBound.SetResult(RxSchedulers.MainThreadScheduler is ControlSequencer);
    }
}
