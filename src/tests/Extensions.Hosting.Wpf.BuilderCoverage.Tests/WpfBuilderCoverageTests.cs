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
        var result = await RunOnStaThreadAsync(static () =>
        {
            var dependency = new FactoryDependency();
            var application = new FactoryWpfApplication(dependency);

            var result = CreateCurrentApplicationCoverage(application);
            result.BaseTypeContextRegistered = IsBaseTypeContextRegistered();
            CaptureFactoryApplicationBuilderCoverage(result, dependency, application);
            CaptureBaseFactoryApplicationBuilderCoverage(result, dependency, application);
            CaptureClassicFactoryCoverage(result, dependency, application);
            result.NullFactoryFailedClearly = NullFactoryFailsClearly();
            return result;
        });

        await Assert.That(result.CanceledStartCompleted).IsTrue();
        await Assert.That(result.InactiveStopCompleted).IsTrue();
        await Assert.That(result.CurrentApplicationResolved).IsTrue();
        await Assert.That(result.ContextRemainedInactive).IsTrue();
        await Assert.That(result.GenericContextLifetimeLinked).IsTrue();
        await Assert.That(result.BaseTypeContextRegistered).IsTrue();
        await Assert.That(result.FactoryWasDeferred).IsTrue();
        await Assert.That(result.FactoryInvocationCount).IsEqualTo(1);
        await Assert.That(result.FactoryAliasResolvedSameInstance).IsTrue();
        await Assert.That(result.FactoryResolvedDependency).IsTrue();
        await Assert.That(result.BaseFactoryWasDeferred).IsTrue();
        await Assert.That(result.BaseFactoryInvocationCount).IsEqualTo(1);
        await Assert.That(result.BaseFactoryResolvedApplication).IsTrue();
        await Assert.That(result.ClassicFactoryWasDeferred).IsTrue();
        await Assert.That(result.ClassicFactoryInvocationCount).IsEqualTo(1);
        await Assert.That(result.ClassicFactoryAliasResolvedSameInstance).IsTrue();
        await Assert.That(result.ClassicFactoryResolvedDependency).IsTrue();
        await Assert.That(result.NullFactoryFailedClearly).IsTrue();
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

    /// <summary>Creates coverage values for current application and inactive hosted-service branches.</summary>
    /// <param name="application">The application instance registered with the host.</param>
    /// <returns>The coverage result.</returns>
    private static WpfFactoryRegistrationResult CreateCurrentApplicationCoverage(Application application)
    {
        var applicationBuilder = Host.CreateApplicationBuilder();
        _ = applicationBuilder.ConfigureWpf(wpfBuilder => wpfBuilder.UseCurrentApplication(application));
        using var host = applicationBuilder.Build();
        var context = host.Services.GetRequiredService<IWpfContext>();
        var serviceProvider = new WpfServiceProvider(context, application);
        using var wpfThread = new WpfThread(serviceProvider);
        WpfHostedService hostedService = new(NullLogger<WpfHostedService>.Instance, wpfThread, context);

        var startTask = hostedService.StartAsync(new(canceled: true));
        var stopTask = hostedService.StopAsync(CancellationToken.None);
        var genericContext = CreateGenericContext();

        return new()
        {
            CanceledStartCompleted = startTask.IsCompletedSuccessfully,
            InactiveStopCompleted = stopTask.IsCompletedSuccessfully,
            CurrentApplicationResolved = ReferenceEquals(host.Services.GetRequiredService<Application>(), application),
            ContextRemainedInactive = !context.IsRunning,
            GenericContextLifetimeLinked = genericContext.IsLifetimeLinked,
        };
    }

    /// <summary>Creates a configured generic host WPF context.</summary>
    /// <returns>The configured WPF context.</returns>
    private static IWpfContext CreateGenericContext()
    {
        var genericHostBuilder = new HostBuilder();
        _ = genericHostBuilder.ConfigureWpf(static wpfBuilder =>
            wpfBuilder.ConfigureContext(static configuredContext => configuredContext.IsLifetimeLinked = true));
        _ = genericHostBuilder.ConfigureWpf();
        using var genericHost = genericHostBuilder.Build();
        return genericHost.Services.GetRequiredService<IWpfContext>();
    }

    /// <summary>Reports whether base application type registration configures WPF without resolving an application.</summary>
    /// <returns><see langword="true"/> when the WPF context was registered.</returns>
    private static bool IsBaseTypeContextRegistered()
    {
        var baseTypeBuilder = Host.CreateApplicationBuilder();
        _ = baseTypeBuilder.ConfigureWpf(static wpfBuilder => wpfBuilder.UseApplication(typeof(Application)));
        using var baseTypeHost = baseTypeBuilder.Build();
        return baseTypeHost.Services.GetRequiredService<IWpfContext>() is not null;
    }

    /// <summary>Captures host application builder factory coverage.</summary>
    /// <param name="result">The result to update.</param>
    /// <param name="dependency">The dependency resolved by the factory.</param>
    /// <param name="application">The application returned by the factory.</param>
    private static void CaptureFactoryApplicationBuilderCoverage(
        WpfFactoryRegistrationResult result,
        FactoryDependency dependency,
        FactoryWpfApplication application)
    {
        var builder = Host.CreateApplicationBuilder();
        var tracker = new FactoryTracker();
        _ = builder.Services.AddSingleton(dependency);
        _ = builder.ConfigureWpfApplication(serviceProvider =>
            CreateFactoryApplication(serviceProvider, dependency, application, tracker));

        using var host = builder.Build();
        result.FactoryWasDeferred = tracker.Invocations == 0;
        var concreteApplication = host.Services.GetRequiredService<FactoryWpfApplication>();
        var baseApplication = host.Services.GetRequiredService<Application>();
        result.FactoryInvocationCount = tracker.Invocations;
        result.FactoryAliasResolvedSameInstance = ReferenceEquals(concreteApplication, baseApplication);
        result.FactoryResolvedDependency = ReferenceEquals(concreteApplication.Dependency, dependency);
    }

    /// <summary>Captures host application builder base application factory coverage.</summary>
    /// <param name="result">The result to update.</param>
    /// <param name="dependency">The dependency resolved by the factory.</param>
    /// <param name="application">The application returned by the factory.</param>
    private static void CaptureBaseFactoryApplicationBuilderCoverage(
        WpfFactoryRegistrationResult result,
        FactoryDependency dependency,
        Application application)
    {
        var builder = Host.CreateApplicationBuilder();
        var tracker = new FactoryTracker();
        Func<IServiceProvider, Application> applicationFactory = serviceProvider =>
            CreateFactoryApplication(serviceProvider, dependency, application, tracker);
        _ = builder.Services.AddSingleton(dependency);
        _ = builder.ConfigureWpfApplication(applicationFactory);

        using var host = builder.Build();
        result.BaseFactoryWasDeferred = tracker.Invocations == 0;
        result.BaseFactoryResolvedApplication = ReferenceEquals(host.Services.GetRequiredService<Application>(), application);
        result.BaseFactoryInvocationCount = tracker.Invocations;
    }

    /// <summary>Captures classic host builder factory coverage.</summary>
    /// <param name="result">The result to update.</param>
    /// <param name="dependency">The dependency resolved by the factory.</param>
    /// <param name="application">The application returned by the factory.</param>
    private static void CaptureClassicFactoryCoverage(
        WpfFactoryRegistrationResult result,
        FactoryDependency dependency,
        FactoryWpfApplication application)
    {
        var builder = new HostBuilder();
        var tracker = new FactoryTracker();
        _ = builder
            .ConfigureServices(services => services.AddSingleton(dependency))
            .ConfigureWpfApplication(serviceProvider =>
                CreateFactoryApplication(serviceProvider, dependency, application, tracker));

        using var host = builder.Build();
        result.ClassicFactoryWasDeferred = tracker.Invocations == 0;
        var concreteApplication = host.Services.GetRequiredService<FactoryWpfApplication>();
        var baseApplication = host.Services.GetRequiredService<Application>();
        result.ClassicFactoryInvocationCount = tracker.Invocations;
        result.ClassicFactoryAliasResolvedSameInstance = ReferenceEquals(concreteApplication, baseApplication);
        result.ClassicFactoryResolvedDependency = ReferenceEquals(concreteApplication.Dependency, dependency);
    }

    /// <summary>Creates the application returned by factory coverage tests.</summary>
    /// <typeparam name="TApplication">The application type returned by the factory.</typeparam>
    /// <param name="serviceProvider">The service provider passed to the factory.</param>
    /// <param name="dependency">The expected dependency instance.</param>
    /// <param name="application">The application to return.</param>
    /// <param name="tracker">The factory invocation tracker.</param>
    /// <returns>The application instance.</returns>
    private static TApplication CreateFactoryApplication<TApplication>(
        IServiceProvider serviceProvider,
        FactoryDependency dependency,
        TApplication application,
        FactoryTracker tracker)
        where TApplication : Application
    {
        tracker.Invocations++;
        var resolvedDependency = serviceProvider.GetRequiredService<FactoryDependency>();
        if (!ReferenceEquals(resolvedDependency, dependency))
        {
            throw new InvalidOperationException("The WPF application factory resolved the wrong dependency.");
        }

        return application;
    }

    /// <summary>Reports whether a null factory result fails clearly.</summary>
    /// <returns><see langword="true"/> when resolution throws the expected exception; otherwise, <see langword="false"/>.</returns>
    private static bool NullFactoryFailsClearly()
    {
        var builder = Host.CreateApplicationBuilder();
        Func<IServiceProvider, FactoryWpfApplication> nullFactory = static _ => null!;
        _ = builder.ConfigureWpfApplication(nullFactory);
        using var host = builder.Build();
        return FactoryResolutionFailsClearly(host.Services);
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

    /// <summary>Resolves the WPF application and reports whether a null factory result fails clearly.</summary>
    /// <param name="serviceProvider">The service provider that contains the WPF application registration.</param>
    /// <returns><see langword="true"/> when resolution throws the expected exception; otherwise, <see langword="false"/>.</returns>
    private static bool FactoryResolutionFailsClearly(IServiceProvider serviceProvider)
    {
        try
        {
            _ = serviceProvider.GetRequiredService<FactoryWpfApplication>();
            return false;
        }
        catch (InvalidOperationException exception)
        {
            return exception.Message.Contains("WPF application factory returned null.", StringComparison.Ordinal);
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

    /// <summary>Provides a dependency resolved by WPF application factories.</summary>
    private sealed class FactoryDependency
    {
        /// <summary>Gets the dependency marker used by analyzer-safe tests.</summary>
        public int Marker { get; } = 1;
    }

    /// <summary>Provides a WPF application with a dependency resolved by a caller factory.</summary>
    /// <param name="dependency">The dependency resolved by the caller factory.</param>
    private sealed class FactoryWpfApplication(FactoryDependency dependency) : Application
    {
        /// <summary>Gets the dependency resolved by the caller factory.</summary>
        public FactoryDependency Dependency { get; } = dependency;
    }

    /// <summary>Tracks WPF application factory invocations.</summary>
    private sealed class FactoryTracker
    {
        /// <summary>Gets or sets the number of times the factory was invoked.</summary>
        public int Invocations { get; set; }
    }

    /// <summary>Stores named WPF factory registration coverage results.</summary>
    private sealed class WpfFactoryRegistrationResult
    {
        /// <summary>Gets or sets a value indicating whether canceled startup completed.</summary>
        public bool CanceledStartCompleted { get; set; }

        /// <summary>Gets or sets a value indicating whether inactive stop completed.</summary>
        public bool InactiveStopCompleted { get; set; }

        /// <summary>Gets or sets a value indicating whether the current application resolved.</summary>
        public bool CurrentApplicationResolved { get; set; }

        /// <summary>Gets or sets a value indicating whether the context remained inactive.</summary>
        public bool ContextRemainedInactive { get; set; }

        /// <summary>Gets or sets a value indicating whether the generic context was lifetime linked.</summary>
        public bool GenericContextLifetimeLinked { get; set; }

        /// <summary>Gets or sets a value indicating whether base-type registration configured the context.</summary>
        public bool BaseTypeContextRegistered { get; set; }

        /// <summary>Gets or sets a value indicating whether host application builder factory creation was deferred.</summary>
        public bool FactoryWasDeferred { get; set; }

        /// <summary>Gets or sets the host application builder factory invocation count.</summary>
        public int FactoryInvocationCount { get; set; }

        /// <summary>Gets or sets a value indicating whether concrete and base factory aliases resolved the same instance.</summary>
        public bool FactoryAliasResolvedSameInstance { get; set; }

        /// <summary>Gets or sets a value indicating whether the factory resolved its dependency.</summary>
        public bool FactoryResolvedDependency { get; set; }

        /// <summary>Gets or sets a value indicating whether the base application factory creation was deferred.</summary>
        public bool BaseFactoryWasDeferred { get; set; }

        /// <summary>Gets or sets the base application factory invocation count.</summary>
        public int BaseFactoryInvocationCount { get; set; }

        /// <summary>Gets or sets a value indicating whether the base application factory resolved the application.</summary>
        public bool BaseFactoryResolvedApplication { get; set; }

        /// <summary>Gets or sets a value indicating whether classic host builder factory creation was deferred.</summary>
        public bool ClassicFactoryWasDeferred { get; set; }

        /// <summary>Gets or sets the classic host builder factory invocation count.</summary>
        public int ClassicFactoryInvocationCount { get; set; }

        /// <summary>Gets or sets a value indicating whether classic concrete and base aliases resolved the same instance.</summary>
        public bool ClassicFactoryAliasResolvedSameInstance { get; set; }

        /// <summary>Gets or sets a value indicating whether the classic factory resolved its dependency.</summary>
        public bool ClassicFactoryResolvedDependency { get; set; }

        /// <summary>Gets or sets a value indicating whether a null factory result failed clearly.</summary>
        public bool NullFactoryFailedClearly { get; set; }
    }
}
