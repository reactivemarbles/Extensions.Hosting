// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactiveMarbles.Extensions.Hosting.UiThread;

namespace Extensions.Hosting.Tests;

/// <summary>Contains line-coverage tests for <see cref="BaseUiThread{T}"/>.</summary>
public class BaseUiThreadCoverageTests
{
    /// <summary>The maximum duration to wait for a UI-thread test signal.</summary>
    private static readonly TimeSpan _uiThreadSignalTimeout = TimeSpan.FromSeconds(5);

    /// <summary>The duration used to confirm that a disposed UI thread does not begin execution.</summary>
    private static readonly TimeSpan _disposedUiThreadSignalTimeout = TimeSpan.FromMilliseconds(100);

    /// <summary>Verifies that the dedicated-thread constructor runs the complete startup sequence.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task Start_WithDedicatedUiThread_RunsInitializationAndUiThread()
    {
        using var context = new TestUiContext();
        await using var provider = CreateServiceProvider(context);
        using var uiThread = new TestUiThread(provider);

        await Assert.That(uiThread.PreUiThreadStarted.Wait(_uiThreadSignalTimeout)).IsTrue();

        uiThread.Start();

        await Assert.That(uiThread.UiThreadStarted.Wait(_uiThreadSignalTimeout)).IsTrue();
        await Assert.That(context.IsRunning).IsTrue();
    }

    /// <summary>Verifies that the caller-thread startup mode completes synchronously.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task Start_WithoutDedicatedUiThread_RunsInitializationAndUiThread()
    {
        using var context = new TestUiContext();
        await using var provider = CreateServiceProvider(context);
        using var uiThread = new TestUiThread(provider, useDedicatedUiThread: false);

        uiThread.Start();

        await Assert.That(uiThread.PreUiThreadStarted.IsSet).IsTrue();
        await Assert.That(uiThread.UiThreadStarted.IsSet).IsTrue();
        await Assert.That(context.IsRunning).IsTrue();
    }

    /// <summary>Verifies that disposing a waiting dedicated UI thread ends its startup safely.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task Dispose_WhileDedicatedUiThreadWaitsForStart_EndsStartupSafely()
    {
        using var context = new TestUiContext { BlockPreUiThreadStart = true };
        await using var provider = CreateServiceProvider(context);
        var uiThread = new TestUiThread(provider);

        await Assert.That(uiThread.PreUiThreadStarted.Wait(_uiThreadSignalTimeout)).IsTrue();

        uiThread.Dispose();
        context.ContinuePreUiThreadStart();

        await Assert.That(uiThread.UiThreadStarted.Wait(_disposedUiThreadSignalTimeout)).IsFalse();
    }

    /// <summary>Verifies that constructing the thread without a service provider fails.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task Constructor_WithNullServiceProvider_ThrowsArgumentNullException()
    {
        static TestUiThread Act() => new(null!, useDedicatedUiThread: false);

        await Assert.That(Act).Throws<ArgumentNullException>();
    }

    /// <summary>Verifies that constructing the thread without its UI context registration fails.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task Constructor_WithoutUiContextRegistration_ThrowsInvalidOperationException()
    {
        await using var provider = new ServiceCollection().BuildServiceProvider();
        static TestUiThread Act(IServiceProvider serviceProvider) => new(serviceProvider, useDedicatedUiThread: false);

        await Assert.That(() => Act(provider)).Throws<InvalidOperationException>();
    }

    /// <summary>Verifies that application exit stops an active linked host lifetime.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task HandleApplicationExit_WithActiveLinkedLifetime_StopsApplication()
    {
        using var context = new TestUiContext { IsLifetimeLinked = true, IsRunning = true };
        using var lifetime = new TestHostApplicationLifetime();
        await using var provider = CreateServiceProvider(context, lifetime);
        using var uiThread = new TestUiThread(provider, useDedicatedUiThread: false);

        uiThread.ExitApplication();

        await Assert.That(context.IsRunning).IsFalse();
        await Assert.That(lifetime.StopApplicationCallCount).IsEqualTo(1);
    }

    /// <summary>Verifies that application exit does not stop an unlinked or unavailable lifetime.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task HandleApplicationExit_WithUnlinkedOrUnavailableLifetime_DoesNotStopApplication()
    {
        using var unlinkedContext = new TestUiContext { IsRunning = true };
        await using var unlinkedProvider = CreateServiceProvider(unlinkedContext);
        using var unlinkedUiThread = new TestUiThread(unlinkedProvider, useDedicatedUiThread: false);

        unlinkedUiThread.ExitApplication();

        await Assert.That(unlinkedContext.IsRunning).IsFalse();

        using var linkedContext = new TestUiContext { IsLifetimeLinked = true, IsRunning = true };
        await using var linkedProvider = CreateServiceProvider(linkedContext);
        using var linkedUiThread = new TestUiThread(linkedProvider, useDedicatedUiThread: false);

        linkedUiThread.ExitApplication();

        await Assert.That(linkedContext.IsRunning).IsFalse();
    }

    /// <summary>Verifies that application exit does not stop a lifetime that is already stopping or stopped.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task HandleApplicationExit_WithStoppingOrStoppedLifetime_DoesNotStopApplication()
    {
        using var stoppingLifetime = new TestHostApplicationLifetime();
        stoppingLifetime.TriggerStopping();
        using var stoppingContext = new TestUiContext { IsLifetimeLinked = true, IsRunning = true };
        await using var stoppingProvider = CreateServiceProvider(stoppingContext, stoppingLifetime);
        using var stoppingUiThread = new TestUiThread(stoppingProvider, useDedicatedUiThread: false);

        stoppingUiThread.ExitApplication();

        await Assert.That(stoppingLifetime.StopApplicationCallCount).IsEqualTo(0);

        using var stoppedLifetime = new TestHostApplicationLifetime();
        stoppedLifetime.TriggerStopped();
        using var stoppedContext = new TestUiContext { IsLifetimeLinked = true, IsRunning = true };
        await using var stoppedProvider = CreateServiceProvider(stoppedContext, stoppedLifetime);
        using var stoppedUiThread = new TestUiThread(stoppedProvider, useDedicatedUiThread: false);

        stoppedUiThread.ExitApplication();

        await Assert.That(stoppedLifetime.StopApplicationCallCount).IsEqualTo(0);
    }

    /// <summary>Verifies that repeated disposal is harmless after unmanaged disposal.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task Dispose_AfterUnmanagedDisposal_IsIdempotent()
    {
        using var context = new TestUiContext();
        await using var provider = CreateServiceProvider(context);
        var uiThread = new TestUiThread(provider, useDedicatedUiThread: false);

        uiThread.DisposeUnmanagedResources();
        uiThread.Dispose();

        await Assert.That(uiThread).IsNotNull();
    }

    /// <summary>Creates a service provider for a UI thread test.</summary>
    /// <param name="context">The UI context to register.</param>
    /// <param name="lifetime">The optional host lifetime to register.</param>
    /// <returns>A configured service provider.</returns>
    private static ServiceProvider CreateServiceProvider(TestUiContext context, IHostApplicationLifetime? lifetime = null)
    {
        var services = new ServiceCollection();
        _ = services.AddSingleton(context);

        if (lifetime is not null)
        {
            _ = services.AddSingleton(lifetime);
        }

        return services.BuildServiceProvider();
    }

    /// <summary>Provides a UI context for test instances.</summary>
    private sealed class TestUiContext : IUiContext, IDisposable
    {
        /// <summary>Stores the event that releases a blocked pre-UI initialization.</summary>
        private readonly ManualResetEventSlim _continuePreUiThreadStart = new(false);

        /// <summary>Gets or sets a value indicating whether pre-UI initialization waits for explicit release.</summary>
        public bool BlockPreUiThreadStart { get; set; }

        /// <inheritdoc />
        public bool IsLifetimeLinked { get; set; }

        /// <inheritdoc />
        public bool IsRunning { get; set; }

        /// <summary>Releases a blocked pre-UI initialization.</summary>
        public void ContinuePreUiThreadStart() => _continuePreUiThreadStart.Set();

        /// <summary>Waits for permission to finish pre-UI initialization.</summary>
        public void WaitForPreUiThreadStartContinuation() => _continuePreUiThreadStart.Wait();

        /// <inheritdoc />
        public void Dispose() => _continuePreUiThreadStart.Dispose();
    }

    /// <summary>Provides public hooks around the protected members of <see cref="BaseUiThread{T}"/>.</summary>
    private sealed class TestUiThread : BaseUiThread<TestUiContext>
    {
        /// <summary>Initializes a new instance of the <see cref="TestUiThread"/> class with a dedicated UI thread.</summary>
        /// <param name="serviceProvider">The service provider to use.</param>
        public TestUiThread(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="TestUiThread"/> class.</summary>
        /// <param name="serviceProvider">The service provider to use.</param>
        /// <param name="useDedicatedUiThread">Whether to use a dedicated UI thread.</param>
        public TestUiThread(IServiceProvider serviceProvider, bool useDedicatedUiThread)
            : base(serviceProvider, useDedicatedUiThread)
        {
        }

        /// <summary>Gets the event signaled when pre-UI initialization begins.</summary>
        public ManualResetEventSlim PreUiThreadStarted { get; } = new(false);

        /// <summary>Gets the event signaled when the UI thread begins.</summary>
        public ManualResetEventSlim UiThreadStarted { get; } = new(false);

        /// <summary>Invokes the protected application-exit handler.</summary>
        public void ExitApplication() => HandleApplicationExit();

        /// <summary>Invokes disposal without managed resource cleanup.</summary>
        public void DisposeUnmanagedResources() => Dispose(disposing: false);

        /// <inheritdoc />
        protected override void PreUiThreadStart()
        {
            PreUiThreadStarted.Set();

            if (!UiContext.BlockPreUiThreadStart)
            {
                return;
            }

            UiContext.WaitForPreUiThreadStartContinuation();
        }

        /// <inheritdoc />
        protected override void UiThreadStart() => UiThreadStarted.Set();
    }

    /// <summary>Provides a controllable host lifetime for test instances.</summary>
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

        /// <summary>Triggers the application-stopped token.</summary>
        public void TriggerStopped() => _applicationStopped.Cancel();

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
