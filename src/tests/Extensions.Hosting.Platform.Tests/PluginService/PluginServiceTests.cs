// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using System.ServiceProcess;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
#if REACTIVE_SHIM
using ReactiveMarbles.Extensions.Hosting.Reactive.PluginService;
#else
using ReactiveMarbles.Extensions.Hosting.PluginService;
#endif

namespace Extensions.Hosting.PluginService.Platform.Tests;

/// <summary>Tests safe PluginService registration and guard behavior.</summary>
public class PluginServiceTests
{
    /// <summary>Stores the expected number of host and service operations in two-operation tests.</summary>
    private const int ExpectedSixOperations = 6;

    /// <summary>Stores the expected number of service stop operations.</summary>
    private const int ExpectedTwoOperations = 2;

    /// <summary>Stores the console switch used to select console host lifetime.</summary>
    private const string ConsoleSwitch = "--console";

    /// <summary>Stores the namespace used to configure test plugin discovery.</summary>
    private const string TestPluginNamespace = "Extensions.Hosting";

    /// <summary>Stores the target runtime supplied to host factory tests.</summary>
    private const string TestRuntime = "test-runtime";

    /// <summary>Verifies that the default logger retains the supplied logger instance.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task DefaultLogger_ExposesInjectedLogger()
    {
        ILogger<DefaultLogger> logger = NullLogger<DefaultLogger>.Instance;

        var defaultLogger = new DefaultLogger(logger);

        await Assert.That(ReferenceEquals(defaultLogger.Logger, logger)).IsTrue();
    }

    /// <summary>Verifies that service lifetime registration preserves the application builder.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task UseServiceBaseLifetime_RegistersServiceLifetime()
    {
        var builder = Host.CreateApplicationBuilder();

        var result = builder.UseServiceBaseLifetime();

        await Assert.That(ReferenceEquals(result, builder)).IsTrue();
        await Assert.That(ContainsRegistration(builder.Services, typeof(IHostLifetime), typeof(ServiceBaseLifetime))).IsTrue();
    }

    /// <summary>Verifies that console lifetime registration preserves the application builder.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task UseConsoleLifetime_RegistersConsoleLifetime()
    {
        var builder = Host.CreateApplicationBuilder();

        var result = builder.UseConsoleLifetime();

        await Assert.That(ReferenceEquals(result, builder)).IsTrue();
        await Assert.That(ContainsRegistration(builder.Services, typeof(IHostLifetime), typeof(ConsoleLifetime))).IsTrue();
    }

    /// <summary>Verifies that a null application builder is rejected for service lifetime registration.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task UseServiceBaseLifetime_WithNullApplicationBuilder_ThrowsArgumentNullException()
    {
        static IHostApplicationBuilder? GetNullBuilder() => null;

        await Assert.That(static () => GetNullBuilder()!.UseServiceBaseLifetime()).Throws<ArgumentNullException>();
    }

    /// <summary>Verifies that a null entry type is rejected before a plugin host is created.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task Create_WithNullEntryType_ThrowsArgumentNullException()
    {
        static Task Act() => ServiceHost.Create(null!, []);

        await Assert.That(Act).Throws<ArgumentNullException>();
    }

    /// <summary>Verifies that a null entry type is rejected before an application host is created.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task CreateApplication_WithNullEntryType_ThrowsArgumentNullException()
    {
        static Task Act() => ServiceHost.CreateApplication(null!, []);

        await Assert.That(Act).Throws<ArgumentNullException>();
    }

    /// <summary>Verifies that service lifetimes require a host application lifetime.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ServiceBaseLifetime_WithNullApplicationLifetime_ThrowsArgumentNullException()
    {
        static ServiceBaseLifetime Act() => new(null!);

        await Assert.That(Act).Throws<ArgumentNullException>();
    }

    /// <summary>Verifies that both public host factories build and pass their hosts to the composed runtime.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ServiceHost_CreateMethods_UseConfiguredRuntimeAndBuildHosts()
    {
        using var runtime = new TestServiceHostRuntime();
        var configureHostCallCount = 0;

        using (ServiceHost.UseRuntime(runtime))
        {
            await ServiceHost.Create(
                typeof(PluginServiceTests),
                [ConsoleSwitch],
                static builder => builder,
                _ => configureHostCallCount++,
                TestPluginNamespace,
                TestRuntime);
            await ServiceHost.CreateApplication(
                typeof(PluginServiceTests),
                [ConsoleSwitch],
                static builder => builder,
                _ => configureHostCallCount++,
                TestPluginNamespace,
                TestRuntime);

            var originalDirectory = Directory.GetCurrentDirectory();
            try
            {
                await ServiceHost.Create(
                    typeof(PluginServiceTests),
                    [],
                    null,
                    null,
                    TestPluginNamespace,
                    TestRuntime);
                await ServiceHost.CreateApplication(
                    typeof(PluginServiceTests),
                    [],
                    null,
                    null,
                    TestPluginNamespace,
                    TestRuntime);
            }
            finally
            {
                Directory.SetCurrentDirectory(originalDirectory);
            }

            var runner = new ServiceHostRunner(runtime);
            await runner.Create(typeof(PluginServiceTests), [ConsoleSwitch]);
            await runner.CreateApplication(typeof(PluginServiceTests), [ConsoleSwitch]);
        }

        await Assert.That(runtime.RunAsyncCallCount).IsEqualTo(ExpectedSixOperations);
        await Assert.That(configureHostCallCount).IsEqualTo(ExpectedTwoOperations);
        await Assert.That(ServiceHost.Logger).IsNotNull();
    }

    /// <summary>Verifies that the runner configuration helpers exercise both selection and null-safe paths.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ServiceHostRunner_ConfigurationHelpers_ConfigureExpectedPaths()
    {
        var serviceLifetimeCallCount = 0;
        var consoleLifetimeCallCount = 0;
        var applicationBuilder = Host.CreateApplicationBuilder();
        var hostBuilder = Host.CreateDefaultBuilder();

        ServiceHostRunner.Configuration.ConfigureLifetime(true, () => serviceLifetimeCallCount++, () => consoleLifetimeCallCount++);
        ServiceHostRunner.Configuration.ConfigureLifetime(false, () => serviceLifetimeCallCount++, () => consoleLifetimeCallCount++);
        _ = ServiceHostRunner.Configuration.ConfigureApplicationLogging(applicationBuilder);
        _ = ServiceHostRunner.Configuration.ConfigureApplicationConfiguration(applicationBuilder, ["--key", "value"]);
        _ = ServiceHostRunner.Configuration.UseContentRoot(applicationBuilder, Directory.GetCurrentDirectory());
        _ = ServiceHostRunner.Configuration.ConfigureHostLogging(hostBuilder);
        _ = ServiceHostRunner.Configuration.ConfigureHostConfiguration(hostBuilder, ["--key", "value"]);

        var configuredBuilder = ServiceHostRunner.Configuration.ConfigureExternal(applicationBuilder, static builder => builder);
        var nullConfiguredBuilder = ServiceHostRunner.Configuration.ConfigureExternal<IHostApplicationBuilder>(null, null);

        ServiceHostRunner.Configuration.ConfigurePluginScanning(null, null, null, TestPluginNamespace);

        await Assert.That(serviceLifetimeCallCount).IsEqualTo(1);
        await Assert.That(consoleLifetimeCallCount).IsEqualTo(1);
        await Assert.That(ReferenceEquals(configuredBuilder, applicationBuilder)).IsTrue();
        await Assert.That(nullConfiguredBuilder).IsNull();
        await Assert.That(ServiceHostRunner.Configuration.ConfigureApplicationLogging(null)).IsNull();
        await Assert.That(ServiceHostRunner.Configuration.ConfigureApplicationConfiguration(null, [])).IsNull();
        await Assert.That(ServiceHostRunner.Configuration.ConfigureHostLogging(null)).IsNull();
        await Assert.That(ServiceHostRunner.Configuration.ConfigureHostConfiguration(null, [])).IsNull();
        await Assert.That(() => ServiceHostRunner.Configuration.UseContentRoot(applicationBuilder, string.Empty)).Throws<ArgumentNullException>();
    }

    /// <summary>Verifies that the composed service runtime drives service start, stop, cancellation, and fault outcomes.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ServiceBaseLifetime_UsesComposedRuntimeForServiceControl()
    {
        using var applicationLifetime = new TestHostApplicationLifetime();
        using var runtime = new TestServiceHostRuntime();
        using var serviceLifetime = new TestServiceBaseLifetime(applicationLifetime, runtime);

        var startTask = serviceLifetime.WaitForStartAsync(CancellationToken.None);
        serviceLifetime.Start([]);
        await startTask;
        await serviceLifetime.StopAsync(CancellationToken.None);
        serviceLifetime.StopFromServiceControlManager();
        applicationLifetime.TriggerStopping();
        runtime.ReleaseServiceRun();

        await Assert.That(runtime.RunServiceCallCount).IsEqualTo(1);
        await Assert.That(runtime.StopServiceCallCount).IsEqualTo(ExpectedTwoOperations);
        await Assert.That(applicationLifetime.StopApplicationCallCount).IsEqualTo(1);
    }

    /// <summary>Verifies that service lifetime startup cancellation and run failures complete its startup task.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ServiceBaseLifetime_StartupCancellationAndFailure_AreReported()
    {
        using var cancellationLifetime = new TestHostApplicationLifetime();
        using var cancellationRuntime = new TestServiceHostRuntime();
        using var cancellationTokenSource = new CancellationTokenSource();
        using var cancellationServiceLifetime = new TestServiceBaseLifetime(cancellationLifetime, cancellationRuntime);

        var cancellationStartTask = cancellationServiceLifetime.WaitForStartAsync(cancellationTokenSource.Token);
        await cancellationTokenSource.CancelAsync();
        await Task.WhenAny(cancellationStartTask, Task.Delay(TimeSpan.FromSeconds(1)));
        cancellationRuntime.ReleaseServiceRun();

        using var failureLifetime = new TestHostApplicationLifetime();
        using var failureRuntime = new TestServiceHostRuntime { RunServiceException = new InvalidOperationException() };
        using var failureServiceLifetime = new TestServiceBaseLifetime(failureLifetime, failureRuntime);
        var failureStartTask = failureServiceLifetime.WaitForStartAsync(CancellationToken.None);
        await Task.WhenAny(failureStartTask, Task.Delay(TimeSpan.FromSeconds(1)));

        await Assert.That(cancellationStartTask.IsCanceled).IsTrue();
        await Assert.That(failureStartTask.IsFaulted).IsTrue();
    }

    /// <summary>Verifies that the production runtime forwards host and service operations to their framework implementations.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ServiceHostRuntime_ForwardsFrameworkOperations()
    {
        var runtime = new ServiceHostRuntime();
        using var cancellationTokenSource = new CancellationTokenSource();
        using var host = Host.CreateApplicationBuilder().Build();
        using var service = new TestServiceBase();

        await cancellationTokenSource.CancelAsync();
        await Assert.That(() => runtime.RunAsync(host, cancellationTokenSource.Token)).Throws<OperationCanceledException>();
        runtime.StopService(service);

        await Assert.That(() => runtime.RunService(null!)).Throws<ArgumentException>();
    }

    /// <summary>Verifies that service-host extension run methods use the composed host runtime instead of directly running hosts.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task RunAsServiceAsync_UsesConfiguredRuntime()
    {
        using var runtime = new TestServiceHostRuntime();

        using (ServiceHost.UseRuntime(runtime))
        {
            var applicationBuilder = Host.CreateApplicationBuilder();
            await applicationBuilder.RunAsServiceAsync();

            var hostBuilder = Host.CreateDefaultBuilder();
            await hostBuilder.RunAsServiceAsync();
        }

        await Assert.That(runtime.RunAsyncCallCount).IsEqualTo(ExpectedTwoOperations);
    }

    /// <summary>Determines whether a collection contains a service registration of the expected types.</summary>
    /// <param name="services">The service registrations to inspect.</param>
    /// <param name="serviceType">The expected service type.</param>
    /// <param name="implementationType">The expected implementation type.</param>
    /// <returns>true when the expected registration exists; otherwise, false.</returns>
    private static bool ContainsRegistration(IEnumerable<ServiceDescriptor> services, Type serviceType, Type implementationType)
    {
        foreach (var descriptor in services)
        {
            if (descriptor.ServiceType == serviceType && descriptor.ImplementationType == implementationType)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Exposes protected service callbacks for deterministic lifetime tests.</summary>
    private sealed class TestServiceBaseLifetime : ServiceBaseLifetime
    {
        /// <summary>Initializes a new instance of the <see cref="TestServiceBaseLifetime"/> class.</summary>
        /// <param name="applicationLifetime">The controllable application lifetime.</param>
        /// <param name="runtime">The deterministic service runtime.</param>
        public TestServiceBaseLifetime(IHostApplicationLifetime applicationLifetime, IServiceHostRuntime runtime)
            : base(applicationLifetime, runtime)
        {
        }

        /// <summary>Signals that the service control manager started this service.</summary>
        /// <param name="args">The service start arguments.</param>
        public void Start(string[] args) => OnStart(args);

        /// <summary>Signals that the service control manager stopped this service.</summary>
        public void StopFromServiceControlManager() => OnStop();
    }

    /// <summary>Provides a deterministic replacement for host execution and service-control-manager operations.</summary>
    private sealed class TestServiceHostRuntime : IServiceHostRuntime, IDisposable
    {
        /// <summary>Stores the service-run completion source.</summary>
        private readonly ManualResetEventSlim _serviceRunRelease = new(false);

        /// <summary>Gets the number of calls to <see cref="RunAsync"/>.</summary>
        public int RunAsyncCallCount { get; private set; }

        /// <summary>Gets the number of calls to <see cref="RunService"/>.</summary>
        public int RunServiceCallCount { get; private set; }

        /// <summary>Gets the number of calls to <see cref="StopService"/>.</summary>
        public int StopServiceCallCount { get; private set; }

        /// <summary>Gets or sets an exception to throw when the simulated service begins running.</summary>
        public Exception? RunServiceException { get; init; }

        /// <inheritdoc/>
        public Task RunAsync(IHost host, CancellationToken cancellationToken)
        {
            RunAsyncCallCount++;
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public void RunService(ServiceBase service)
        {
            RunServiceCallCount++;
            if (RunServiceException is not null)
            {
                throw RunServiceException;
            }

            _serviceRunRelease.Wait();
        }

        /// <inheritdoc/>
        public void StopService(ServiceBase service) => StopServiceCallCount++;

        /// <summary>Releases an in-progress simulated service run.</summary>
        public void ReleaseServiceRun() => _serviceRunRelease.Set();

        /// <inheritdoc />
        public void Dispose() => _serviceRunRelease.Dispose();
    }

    /// <summary>Provides a concrete service base instance for runtime forwarding tests.</summary>
    private sealed class TestServiceBase : ServiceBase;

    /// <summary>Provides a controllable application lifetime for service lifetime tests.</summary>
    private sealed class TestHostApplicationLifetime : IHostApplicationLifetime, IDisposable
    {
        /// <summary>Stores the application-started token source.</summary>
        private readonly CancellationTokenSource _applicationStarted = new();

        /// <summary>Stores the application-stopping token source.</summary>
        private readonly CancellationTokenSource _applicationStopping = new();

        /// <summary>Stores the application-stopped token source.</summary>
        private readonly CancellationTokenSource _applicationStopped = new();

        /// <inheritdoc />
        public CancellationToken ApplicationStarted => _applicationStarted.Token;

        /// <inheritdoc />
        public CancellationToken ApplicationStopping => _applicationStopping.Token;

        /// <inheritdoc />
        public CancellationToken ApplicationStopped => _applicationStopped.Token;

        /// <summary>Gets the number of calls to <see cref="StopApplication"/>.</summary>
        public int StopApplicationCallCount { get; private set; }

        /// <summary>Triggers the application-stopping token.</summary>
        public void TriggerStopping() => _applicationStopping.Cancel();

        /// <inheritdoc />
        public void StopApplication()
        {
            StopApplicationCallCount++;
            TriggerStopping();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _applicationStarted.Dispose();
            _applicationStopping.Dispose();
            _applicationStopped.Dispose();
        }
    }
}
