// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using ReactiveMarbles.Extensions.Hosting.Plugins;

namespace Extensions.Hosting.Tests;

/// <summary>Contains unit tests for the hosted service base lifecycle implementation.</summary>
public class HostedServiceBaseTests
{
    /// <summary>Verifies that lifetime callbacks invoke the overridable lifecycle methods.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task LifetimeCallbacks_InvokeLifecycleMethods()
    {
        using var lifetime = new TestHostApplicationLifetime();
        var service = new TestHostedService(lifetime);

        await service.StartAsync(CancellationToken.None).ConfigureAwait(false);
        lifetime.TriggerStarted();
        await service.Started.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);

        await Assert.That(service.StartedCount).IsEqualTo(1);
        await Assert.That(service.CleanupDisposable).IsNotNull();
        await Assert.That(service.CleanupDisposable!.IsDisposed).IsFalse();

        lifetime.TriggerStopping();
        await Assert.That(service.StoppingCount).IsEqualTo(1);
        await Assert.That(service.CleanupDisposable.IsDisposed).IsTrue();
        await Assert.That(service.IsDisposed).IsTrue();

        lifetime.TriggerStopped();
        await Assert.That(service.StoppedCount).IsEqualTo(1);
    }

    /// <summary>Verifies that disposing the service is idempotent and disposes cleanup resources.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task Dispose_IsIdempotentAndDisposesCleanup()
    {
        using var lifetime = new TestHostApplicationLifetime();
        var service = new TestHostedService(lifetime);

        await service.StartAsync(CancellationToken.None).ConfigureAwait(false);
        lifetime.TriggerStarted();
        await service.Started.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);

        service.Dispose();
        service.Dispose();

        await Assert.That(service.CleanupDisposable).IsNotNull();
        await Assert.That(service.CleanupDisposable!.IsDisposed).IsTrue();
        await Assert.That(service.IsDisposed).IsTrue();
    }

    /// <summary>Verifies that StopAsync returns a completed task.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task StopAsync_ReturnsCompletedTask()
    {
        using var lifetime = new TestHostApplicationLifetime();
        var service = new DefaultHostedService(lifetime);

        var stopTask = service.StopAsync(CancellationToken.None);

        await Assert.That(stopTask.IsCompletedSuccessfully).IsTrue();
    }

    /// <summary>Verifies that the default OnStarted implementation completes successfully.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task OnStarted_DefaultImplementation_CompletesSuccessfully()
    {
        using var lifetime = new TestHostApplicationLifetime();
        var service = new DefaultHostedService(lifetime);

        await service.OnStarted().ConfigureAwait(false);

        await Assert.That(service).IsNotNull();
    }

    /// <summary>Verifies that an OnStarted exception is observed and contained by the lifetime callback.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task LifetimeStarted_WhenOnStartedThrows_DoesNotThrowFromCallback()
    {
        using var lifetime = new TestHostApplicationLifetime();
        var service = new ThrowingStartedHostedService(lifetime);

        await service.StartAsync(CancellationToken.None).ConfigureAwait(false);
        lifetime.TriggerStarted();
        await service.Started.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        await Task.Delay(TimeSpan.FromMilliseconds(50)).ConfigureAwait(false);

        await Assert.That(service.StartedCount).IsEqualTo(1);
    }

    /// <summary>Test hosted service used to observe lifecycle callback execution.</summary>
    /// <param name="lifetime">The test lifetime source.</param>
    private sealed class TestHostedService(IHostApplicationLifetime lifetime)
        : HostedServiceBase<TestHostedService>(NullLogger<TestHostedService>.Instance, lifetime)
    {
        /// <summary>Gets the started signal.</summary>
        public TaskCompletionSource<bool> Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        /// <summary>Gets the cleanup disposable.</summary>
        public TrackingDisposable? CleanupDisposable { get; private set; }

        /// <summary>Gets the number of started callbacks.</summary>
        public int StartedCount { get; private set; }

        /// <summary>Gets the number of stopping callbacks.</summary>
        public int StoppingCount { get; private set; }

        /// <summary>Gets the number of stopped callbacks.</summary>
        public int StoppedCount { get; private set; }

        /// <inheritdoc />
        public override Task OnStarted()
        {
            StartedCount++;
            CleanupDisposable = new();
            CleanUp.Add(CleanupDisposable);
            _ = Started.TrySetResult(true);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public override void OnStopping()
        {
            StoppingCount++;
            base.OnStopping();
        }

        /// <inheritdoc />
        public override void OnStopped()
        {
            StoppedCount++;
            base.OnStopped();
        }
    }

    /// <summary>Hosted service that uses the base lifecycle implementations.</summary>
    /// <param name="lifetime">The test lifetime source.</param>
    private sealed class DefaultHostedService(IHostApplicationLifetime lifetime)
        : HostedServiceBase<DefaultHostedService>(NullLogger<DefaultHostedService>.Instance, lifetime);

    /// <summary>Hosted service that throws from OnStarted.</summary>
    /// <param name="lifetime">The test lifetime source.</param>
    private sealed class ThrowingStartedHostedService(IHostApplicationLifetime lifetime)
        : HostedServiceBase<ThrowingStartedHostedService>(NullLogger<ThrowingStartedHostedService>.Instance, lifetime)
    {
        /// <summary>Gets the started signal.</summary>
        public TaskCompletionSource<bool> Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        /// <summary>Gets the number of started callbacks.</summary>
        public int StartedCount { get; private set; }

        /// <inheritdoc />
        public override Task OnStarted()
        {
            StartedCount++;
            _ = Started.TrySetResult(true);
            throw new InvalidOperationException("Start failure for test coverage.");
        }
    }

    /// <summary>Disposable used to verify cleanup disposal.</summary>
    private sealed class TrackingDisposable : IDisposable
    {
        /// <summary>Gets a value indicating whether this instance was disposed.</summary>
        public bool IsDisposed { get; private set; }

        /// <inheritdoc />
        public void Dispose() => IsDisposed = true;
    }

    /// <summary>Test application lifetime that exposes manual trigger methods.</summary>
    private sealed class TestHostApplicationLifetime : IHostApplicationLifetime, IDisposable
    {
        /// <summary>Stores the started token source.</summary>
        private readonly CancellationTokenSource _started = new();

        /// <summary>Stores the stopping token source.</summary>
        private readonly CancellationTokenSource _stopping = new();

        /// <summary>Stores the stopped token source.</summary>
        private readonly CancellationTokenSource _stopped = new();

        /// <inheritdoc />
        public CancellationToken ApplicationStarted => _started.Token;

        /// <inheritdoc />
        public CancellationToken ApplicationStopping => _stopping.Token;

        /// <inheritdoc />
        public CancellationToken ApplicationStopped => _stopped.Token;

        /// <summary>Triggers the started token.</summary>
        public void TriggerStarted() => _started.Cancel();

        /// <summary>Triggers the stopping token.</summary>
        public void TriggerStopping() => _stopping.Cancel();

        /// <summary>Triggers the stopped token.</summary>
        public void TriggerStopped() => _stopped.Cancel();

        /// <inheritdoc />
        public void StopApplication() => TriggerStopping();

        /// <inheritdoc />
        public void Dispose()
        {
            _started.Dispose();
            _stopping.Dispose();
            _stopped.Dispose();
        }
    }
}
