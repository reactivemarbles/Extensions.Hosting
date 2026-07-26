// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactiveMarbles.Extensions.Hosting.Wpf;
#if REACTIVE_SHIM
using System.Reactive.Concurrency;
using ReactiveMarbles.Extensions.Hosting.Reactive.ReactiveUI;
using ReactiveUI.Primitives.Reactive.Concurrency;
using ReactiveUI.Reactive;
#else
using ReactiveMarbles.Extensions.Hosting.ReactiveUI;
using ReactiveUI;
using ReactiveUI.Primitives.Concurrency;
#endif

namespace Extensions.Hosting.ReactiveUI.Wpf.Tests;

/// <summary>Verifies that ReactiveUI uses the dispatcher created by the hosted WPF application.</summary>
[NotInParallel]
public sealed class ReactiveWpfSchedulerTests
{
    /// <summary>Gets the maximum duration allowed for an asynchronous test operation.</summary>
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(30);

    /// <summary>Verifies scheduler binding to the hosted application dispatcher.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task
        ConfigureSplatForMicrosoftDependencyResolver_BindsSchedulerToHostedApplicationDispatcher()
    {
        var originalScheduler = RxSchedulers.MainThreadScheduler;
        using var host = await BuildHostOnBootstrapThread().WaitAsync(Timeout);

        try
        {
            await host.StartAsync().WaitAsync(Timeout);
            var applicationDispatcher = await TestApplication.Started.Task.WaitAsync(Timeout);
            var scheduler = RxSchedulers.MainThreadScheduler;

            await Assert.That(scheduler).IsTypeOf<DispatcherSequencer>();

            var dispatcherScheduler = (DispatcherSequencer)scheduler;
            await Assert.That(dispatcherScheduler.Dispatcher).IsSameReferenceAs(applicationDispatcher);

            var scheduledThread = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
#if REACTIVE_SHIM
            Func<IScheduler, TaskCompletionSource<int>, IDisposable> scheduleAction =
                static (_, completion) =>
                {
                    completion.SetResult(Environment.CurrentManagedThreadId);
                    return System.Reactive.Disposables.Disposable.Empty;
                };
            using var scheduledWork = ((IScheduler)scheduler).Schedule(
                scheduledThread,
                scheduleAction);
#else
            using var scheduledWork = scheduler.Schedule(
                scheduledThread,
                static completion => _ = completion.TrySetResult(Environment.CurrentManagedThreadId));
#endif
            var scheduledThreadId = await scheduledThread.Task.WaitAsync(Timeout);

            await Assert.That(scheduledThreadId).IsEqualTo(applicationDispatcher.Thread.ManagedThreadId);
        }
        finally
        {
            await host.StopAsync().WaitAsync(Timeout);
            RxSchedulers.MainThreadScheduler = originalScheduler;
        }
    }

    /// <summary>Verifies that the scheduler service rejects an absent WPF application.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureSplatForMicrosoftDependencyResolver_SchedulerServiceRejectsNullApplication()
    {
        var builder = Host.CreateApplicationBuilder();
        _ = builder.ConfigureSplatForMicrosoftDependencyResolver();
        using var host = builder.Build();
        var schedulerService = host.Services.GetRequiredService<IWpfService>();

        void Act() => schedulerService.Initialize(null!);

        await Assert.That(Act).Throws<ArgumentNullException>();
    }

    /// <summary>Builds a host on a thread with a dispatcher that differs from the hosted WPF dispatcher.</summary>
    /// <returns>A task that produces the configured host.</returns>
    private static Task<IHost> BuildHostOnBootstrapThread()
    {
        var hostCompletion = new TaskCompletionSource<IHost>(TaskCreationOptions.RunContinuationsAsynchronously);
        var bootstrapThread = new Thread(() =>
        {
            try
            {
                var builder = Host.CreateApplicationBuilder();
                _ = builder.ConfigureSplatForMicrosoftDependencyResolver();
                _ = builder.ConfigureWpf(static wpfBuilder => wpfBuilder.UseApplication<TestApplication>());
                _ = hostCompletion.TrySetResult(builder.Build());
            }
            catch (Exception exception)
            {
                _ = hostCompletion.TrySetException(exception);
            }
        }) { IsBackground = true };

        bootstrapThread.SetApartmentState(ApartmentState.STA);
        bootstrapThread.Start();
        return hostCompletion.Task;
    }
}
