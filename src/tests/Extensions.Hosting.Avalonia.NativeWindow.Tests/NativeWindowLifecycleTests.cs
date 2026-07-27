// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Avalonia;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactiveMarbles.Extensions.Hosting.Avalonia;

namespace Extensions.Hosting.Avalonia.NativeWindow.Tests;

/// <summary>Exercises native shell window paths in an isolated Avalonia test process.</summary>
[NotInParallel]
public sealed class NativeWindowLifecycleTests
{
    /// <summary>Gets the maximum duration allowed for a native window lifecycle.</summary>
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

#if ONE_SHELL
    /// <summary>Verifies a single configured shell is made the main window and shown.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task OneShellWindow_StartsAndShutsDownInDedicatedProcess()
    {
        var signals = new LifecycleSignals();
        using var host = BuildHost(signals, static builder => builder.UseWindow<FirstShellWindow>());

        await StartAndAwaitShutdown(host, signals);
    }
#else
    /// <summary>Verifies multiple configured shells use the multiple-window lifetime path.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task MultipleShellWindows_StartAndShutDownInDedicatedProcess()
    {
        var signals = new LifecycleSignals();
        using var host = BuildHost(signals, static builder =>
        {
            _ = builder.UseWindow<FirstShellWindow>();
            _ = builder.UseWindow<SecondShellWindow>();
        });

        await StartAndAwaitShutdown(host, signals);
    }
#endif

    /// <summary>Builds a host with a native Avalonia application.</summary>
    /// <param name="signals">Signals set by the Avalonia service at startup.</param>
    /// <param name="configureWindows">Registers the shell windows to exercise.</param>
    /// <returns>The configured host.</returns>
    private static IHost BuildHost(LifecycleSignals signals, Action<IAvaloniaBuilder> configureWindows)
    {
        var builder = Host.CreateApplicationBuilder();
        _ = builder.Services.AddSingleton(signals);
        _ = builder.Services.AddSingleton<IAvaloniaService, RecordingAvaloniaService>();
        _ = builder.ConfigureAvalonia(avaloniaBuilder =>
        {
            _ = avaloniaBuilder.UseApplication<ShutdownApplication>();
            configureWindows(avaloniaBuilder);
        });
        _ = builder.UseAvaloniaLifetime();
        return builder.Build();
    }

    /// <summary>Starts a host on STA and waits for its application-requested shutdown.</summary>
    /// <param name="host">The host to start.</param>
    /// <param name="signals">Signals recorded during Avalonia startup.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    private static async Task StartAndAwaitShutdown(IHost host, LifecycleSignals signals)
    {
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var thread = new Thread(() => StartHost(host, completion)) { IsBackground = true };
        SetApartmentState(thread, ApartmentState.STA);
        thread.Start();

        await signals.Initialized.Task.WaitAsync(Timeout);
        await GetAvaloniaHostedService(host).StopAsync(CancellationToken.None).WaitAsync(Timeout);
        await completion.Task.WaitAsync(Timeout);
        await host.StopAsync().WaitAsync(Timeout);

        await Assert.That(host.Services.GetRequiredService<IAvaloniaContext>().IsRunning).IsFalse();
    }

    /// <summary>Gets the Avalonia hosted service from a configured host.</summary>
    /// <param name="host">The host that owns the service.</param>
    /// <returns>The registered Avalonia hosted service.</returns>
    private static AvaloniaHostedService GetAvaloniaHostedService(IHost host)
    {
        foreach (var service in host.Services.GetServices<IHostedService>())
        {
            if (service is AvaloniaHostedService avaloniaHostedService)
            {
                return avaloniaHostedService;
            }
        }

        throw new InvalidOperationException("The host did not register an Avalonia hosted service.");
    }

    /// <summary>Starts a host and completes the supplied source from its result.</summary>
    /// <param name="host">The host to start.</param>
    /// <param name="completion">The completion source to set.</param>
    private static void StartHost(IHost host, TaskCompletionSource completion)
    {
        try
        {
            var start = host.StartAsync();
            _ = start.ContinueWith(
                static (task, state) => CompleteTask(task, (TaskCompletionSource)state!),
                completion,
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
        }
        catch (Exception exception)
        {
            _ = completion.TrySetException(exception);
        }
    }

    /// <summary>Sets a thread apartment state on Windows.</summary>
    /// <param name="thread">The thread to configure.</param>
    /// <param name="apartmentState">The requested apartment state.</param>
    private static void SetApartmentState(Thread thread, ApartmentState apartmentState)
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        thread.SetApartmentState(apartmentState);
    }

    /// <summary>Completes a task completion source from an arbitrary task result.</summary>
    /// <param name="task">The task to inspect.</param>
    /// <param name="completion">The source to complete.</param>
    private static void CompleteTask(Task task, TaskCompletionSource completion)
    {
        if (task.IsFaulted)
        {
            _ = completion.TrySetException(task.Exception!.InnerExceptions);
            return;
        }

        if (task.IsCanceled)
        {
            _ = completion.TrySetCanceled();
            return;
        }

        _ = completion.TrySetResult();
    }

    /// <summary>An Avalonia application used by native window tests.</summary>
    public sealed class ShutdownApplication : Application;

    /// <summary>A first shell window.</summary>
    public sealed class FirstShellWindow : Window, IAvaloniaShell;

    /// <summary>A second shell window.</summary>
    public sealed class SecondShellWindow : Window, IAvaloniaShell;

    /// <summary>Records Avalonia service initialization.</summary>
    /// <param name="signals">The signal store to update.</param>
    public sealed class RecordingAvaloniaService(LifecycleSignals signals) : IAvaloniaService
    {
        /// <inheritdoc />
        public void Initialize(Application application) => _ = signals.Initialized.TrySetResult(application);
    }

    /// <summary>Stores the application startup signal.</summary>
    public sealed class LifecycleSignals
    {
        /// <summary>Gets the completion source for Avalonia service initialization.</summary>
        public TaskCompletionSource<Application> Initialized { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    }
}
