// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactiveMarbles.Extensions.Hosting.Avalonia;

namespace Extensions.Hosting.Avalonia.Tests;

/// <summary>Verifies the bounded hosted Avalonia application lifecycle.</summary>
[NotInParallel]
public sealed class AvaloniaHostedServiceTests
{
    /// <summary>Gets the maximum duration for a desktop lifecycle test.</summary>
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

    /// <summary>Verifies a desktop application starts on an STA thread and is shut down through the hosted service.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task HostedAvaloniaApplication_StartsInitializesServicesAndStops()
    {
        var signals = new LifecycleSignals();
        var host = BuildHost(signals, configureAppBuilder: true);
        var service = GetHostedService(host);
        var context = host.Services.GetRequiredService<IAvaloniaContext>();
        await service.StartAsync(new(canceled: true));
        await Assert.That(context.IsRunning).IsFalse();

        var startupException = await StartOnMtaThread(service);
        await Assert.That(startupException).IsTypeOf<InvalidOperationException>();

        var start = StartHostOnStaThread(host);
        try
        {
            var application = await signals.Initialized.Task.WaitAsync(Timeout);

            await Assert.That(signals.AppBuilderConfigured).IsTrue();
            await Assert.That(context.IsRunning).IsTrue();
            await Assert.That(context.AvaloniaApplication).IsSameReferenceAs(application);

            await service.StopAsync(CancellationToken.None).WaitAsync(Timeout);
            await start.WaitAsync(Timeout);
        }
        finally
        {
            if (start.IsCompleted)
            {
                await host.StopAsync().WaitAsync(Timeout);
            }
            else
            {
                await service.StopAsync(CancellationToken.None).WaitAsync(Timeout);
                await start.WaitAsync(Timeout);
                await host.StopAsync().WaitAsync(Timeout);
            }

            host.Dispose();
        }

        await Assert.That(context.IsRunning).IsFalse();
    }

    /// <summary>Creates a host configured with a non-visible Avalonia application.</summary>
    /// <param name="signals">Lifecycle signals to register with the host.</param>
    /// <param name="configureAppBuilder">Whether to configure the Avalonia application builder.</param>
    /// <returns>The configured host.</returns>
    private static IHost BuildHost(LifecycleSignals signals, bool configureAppBuilder = false)
    {
        var builder = Host.CreateApplicationBuilder();
        _ = builder.Services.AddSingleton(signals);
        _ = builder.Services.AddSingleton<IAvaloniaService, RecordingAvaloniaService>();
        Action<IAvaloniaBuilder>? configureAppBuilderAction = configureAppBuilder
            ? avaloniaBuilder => avaloniaBuilder.ConfigureAppBuilder(_ => signals.AppBuilderConfigured = true)
            : null;
        _ = builder.ConfigureAvalonia(avaloniaBuilder =>
        {
            _ = avaloniaBuilder.UseApplication<TestApplication>();
            configureAppBuilderAction?.Invoke(avaloniaBuilder);
        });

        return builder.Build();
    }

    /// <summary>Gets the Avalonia hosted service registered by a host.</summary>
    /// <param name="host">The configured host.</param>
    /// <returns>The registered Avalonia hosted service.</returns>
    private static AvaloniaHostedService GetHostedService(IHost host)
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

    /// <summary>Starts a host synchronously from a background STA thread.</summary>
    /// <param name="host">The host to start.</param>
    /// <returns>A task which completes when the Avalonia message loop stops.</returns>
    private static Task StartHostOnStaThread(IHost host)
    {
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var thread = new Thread(() =>
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
                completion.SetException(exception);
            }
        }) { IsBackground = true };
        SetApartmentState(thread, ApartmentState.STA);
        thread.Start();
        return completion.Task;
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

    /// <summary>Invokes Avalonia startup on an MTA thread.</summary>
    /// <param name="service">The hosted service to start.</param>
    /// <returns>A task containing the startup exception, if any.</returns>
    private static Task<Exception?> StartOnMtaThread(AvaloniaHostedService service)
    {
        var completion = new TaskCompletionSource<Exception?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var thread = new Thread(() =>
        {
            try
            {
                _ = service.StartAsync(CancellationToken.None);
                _ = completion.TrySetResult(null);
            }
            catch (Exception exception)
            {
                _ = completion.TrySetResult(exception);
            }
        }) { IsBackground = true };
        SetApartmentState(thread, ApartmentState.MTA);
        thread.Start();
        return completion.Task;
    }
}
