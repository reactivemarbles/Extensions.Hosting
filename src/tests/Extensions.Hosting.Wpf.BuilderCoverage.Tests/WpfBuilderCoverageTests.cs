// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using ReactiveMarbles.Extensions.Hosting.Wpf;
using ReactiveMarbles.Extensions.Hosting.Wpf.Internals;

namespace Extensions.Hosting.Wpf.BuilderCoverage.Tests;

/// <summary>Verifies WPF branches that require a process-owned application instance.</summary>
[NotInParallel]
public sealed class WpfBuilderCoverageTests
{
    /// <summary>Verifies base application registration, default generic builder configuration, and inactive hosted-service branches.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureWpf_RegistersCurrentBaseApplicationAndCoversInactiveHostedServiceBranches()
    {
        var configurationSucceeded = await RunOnStaThreadAsync(static () =>
        {
            var application = new Application();
            var applicationBuilder = Host.CreateApplicationBuilder();
            _ = applicationBuilder.ConfigureWpf(wpfBuilder => wpfBuilder.UseCurrentApplication(application));
            using var host = applicationBuilder.Build();
            var context = host.Services.GetRequiredService<IWpfContext>();
            var serviceProvider = new WpfServiceProvider(context, application);
            using var wpfThread = new WpfThread(serviceProvider);
            WpfHostedService hostedService = new(NullLogger<WpfHostedService>.Instance, wpfThread, context);

            var startTask = hostedService.StartAsync(new(canceled: true));
            var stopTask = hostedService.StopAsync(CancellationToken.None);

            var genericHostBuilder = new HostBuilder();
            _ = genericHostBuilder.ConfigureWpf(static wpfBuilder =>
                wpfBuilder.ConfigureContext(static configuredContext => configuredContext.IsLifetimeLinked = true));
            _ = genericHostBuilder.ConfigureWpf();
            using var genericHost = genericHostBuilder.Build();
            var genericContext = genericHost.Services.GetRequiredService<IWpfContext>();

            return startTask.IsCompletedSuccessfully
                && stopTask.IsCompletedSuccessfully
                && ReferenceEquals(host.Services.GetRequiredService<Application>(), application)
                && !context.IsRunning
                && genericContext.IsLifetimeLinked;
        });

        await Assert.That(configurationSucceeded).IsTrue();
    }

    /// <summary>Verifies WPF context and hosted shutdown branches that do not require an application instance.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task WpfContextAndHostedService_HandleMissingApplication()
    {
        var builder = Host.CreateApplicationBuilder();
        _ = builder.ConfigureWpf();
        using var host = builder.Build();
        var registeredContext = host.Services.GetRequiredService<IWpfContext>();

        Dispatcher? dispatcherBeforeApplication = registeredContext.Dispatcher;

        var shutdownSkipped = await RunOnDispatcherThreadAsync(async static dispatcher =>
        {
            var shutdownContext = new DispatcherOnlyWpfContext(dispatcher) { IsRunning = true };
            using var wpfThread = new WpfThread(new WpfServiceProvider(shutdownContext));
            WpfHostedService hostedService = new(NullLogger<WpfHostedService>.Instance, wpfThread, shutdownContext);

            await hostedService.StopAsync(CancellationToken.None);
            return shutdownContext.WpfApplication is null;
        });

        await Assert.That(dispatcherBeforeApplication).IsNull();
        await Assert.That(shutdownSkipped).IsTrue();
    }

    /// <summary>Runs an asynchronous operation on a dedicated single-threaded apartment thread.</summary>
    /// <typeparam name="T">The result type returned by the operation.</typeparam>
    /// <param name="operation">The operation to execute.</param>
    /// <returns>A task that completes when the operation finishes.</returns>
    private static Task<T> RunOnStaThreadAsync<T>(Func<T> operation)
    {
        var completion = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        var thread = new Thread(() =>
        {
            try
            {
                _ = completion.TrySetResult(operation());
            }
            catch (Exception exception)
            {
                _ = completion.TrySetException(exception);
            }
        }) { IsBackground = true };

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        return completion.Task;
    }

    /// <summary>Runs an asynchronous operation on a dedicated dispatcher thread.</summary>
    /// <typeparam name="T">The result type returned by the operation.</typeparam>
    /// <param name="operation">The operation to execute with the dispatcher.</param>
    /// <returns>A task that completes when the operation finishes.</returns>
    private static Task<T> RunOnDispatcherThreadAsync<T>(Func<Dispatcher, Task<T>> operation)
    {
        var completion = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        var thread = new Thread(() =>
        {
            var dispatcher = Dispatcher.CurrentDispatcher;
            var operationTask = ObserveDispatcherOperationAsync(operation, dispatcher, completion);
            _ = operationTask.ContinueWith(
                static (task, state) =>
                {
                    if (!task.IsFaulted)
                    {
                        return;
                    }

                    _ = ((TaskCompletionSource<T>)state!).TrySetException(task.Exception!);
                },
                completion,
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);

            Dispatcher.Run();
        }) { IsBackground = true };

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        return completion.Task;
    }

    /// <summary>Observes an asynchronous dispatcher operation and shuts down the dispatcher when it finishes.</summary>
    /// <typeparam name="T">The result type returned by the operation.</typeparam>
    /// <param name="operation">The operation to execute with the dispatcher.</param>
    /// <param name="dispatcher">The dispatcher running the operation.</param>
    /// <param name="completion">The completion source to update with the operation result.</param>
    /// <returns>A task that completes when observation has finished.</returns>
    private static async Task ObserveDispatcherOperationAsync<T>(
        Func<Dispatcher, Task<T>> operation,
        Dispatcher dispatcher,
        TaskCompletionSource<T> completion)
    {
        try
        {
            var result = await operation(dispatcher);
            _ = completion.TrySetResult(result);
        }
        catch (OperationCanceledException)
        {
            _ = completion.TrySetCanceled();
        }
        catch (Exception exception)
        {
            _ = completion.TrySetException(exception);
        }
        finally
        {
            dispatcher.BeginInvokeShutdown(DispatcherPriority.Normal);
        }
    }

    /// <summary>Provides the services required by <see cref="WpfThread"/>.</summary>
    /// <param name="context">The WPF context to provide.</param>
    /// <param name="application">The WPF application to provide.</param>
    public sealed class WpfServiceProvider(IWpfContext context, Application? application = null) : IServiceProvider
    {
        /// <inheritdoc />
        public object? GetService(Type serviceType)
        {
            if (serviceType == typeof(IWpfContext))
            {
                return context;
            }

            return serviceType == typeof(Application) ? application : null;
        }
    }

    /// <summary>Provides the WPF context values needed by hosted-service branch tests.</summary>
    /// <param name="dispatcher">The dispatcher to return for shutdown operations.</param>
    private sealed class DispatcherOnlyWpfContext(Dispatcher dispatcher) : IWpfContext
    {
        /// <inheritdoc />
        public ShutdownMode ShutdownMode { get; set; } = ShutdownMode.OnLastWindowClose;

        /// <inheritdoc />
        public Application? WpfApplication { get; set; }

        /// <inheritdoc />
        public Dispatcher Dispatcher { get; } = dispatcher;

        /// <inheritdoc />
        public bool IsLifetimeLinked { get; set; }

        /// <inheritdoc />
        public bool IsRunning { get; set; }
    }
}
