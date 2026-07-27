// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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

            _ = hostedService.StartAsync(new(canceled: true));
            _ = hostedService.StopAsync(CancellationToken.None);

            var genericHostBuilder = new HostBuilder();
            _ = genericHostBuilder.ConfigureWpf(static wpfBuilder =>
                wpfBuilder.ConfigureContext(static configuredContext => configuredContext.IsLifetimeLinked = true));
            _ = genericHostBuilder.ConfigureWpf();
            using var genericHost = genericHostBuilder.Build();
            var genericContext = genericHost.Services.GetRequiredService<IWpfContext>();

            return ReferenceEquals(host.Services.GetRequiredService<Application>(), application)
                && !context.IsRunning
                && genericContext.IsLifetimeLinked;
        });

        await Assert.That(configurationSucceeded).IsTrue();
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

    /// <summary>Provides the services required by <see cref="WpfThread"/>.</summary>
    /// <param name="context">The WPF context to provide.</param>
    /// <param name="application">The WPF application to provide.</param>
    public sealed class WpfServiceProvider(IWpfContext context, Application application) : IServiceProvider
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
}
