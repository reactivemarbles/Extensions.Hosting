// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using ReactiveMarbles.Extensions.Hosting.Maui;
using ReactiveMarbles.Extensions.Hosting.Maui.Internals;

namespace Extensions.Hosting.Maui.Platform.Tests;

/// <summary>Tests MAUI hosted-service orchestration without a device UI loop.</summary>
public class MauiHostedServiceTests
{
    /// <summary>Verifies that starting a non-cancelled host starts its composed UI-thread starter.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task StartAsync_WhenNotCancelled_StartsComposedThreadStarter()
    {
        var starts = 0;
        var service = new MauiHostedService(
            NullLogger<MauiHostedService>.Instance,
            new MauiThreadStarter(() => starts++),
            new TestMauiContext());

        await service.StartAsync(CancellationToken.None);

        await Assert.That(starts).IsEqualTo(1);
    }

    /// <summary>Verifies that starting a cancelled host does not start its composed UI-thread starter.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task StartAsync_WhenCancelled_DoesNotStartComposedThreadStarter()
    {
        var starts = 0;
        var service = new MauiHostedService(
            NullLogger<MauiHostedService>.Instance,
            new MauiThreadStarter(() => starts++),
            new TestMauiContext());
        using var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();

        await service.StartAsync(cancellationTokenSource.Token);

        await Assert.That(starts).IsEqualTo(0);
    }

    /// <summary>Verifies that stopping an inactive host completes without requesting UI shutdown.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task StopAsync_WhenNotRunning_Completes()
    {
        var service = new MauiHostedService(
            NullLogger<MauiHostedService>.Instance,
            new MauiThreadStarter(static () => { }),
            new TestMauiContext());

        var completed = false;
        await service.StopAsync(CancellationToken.None);
        completed = true;

        await Assert.That(completed).IsTrue();
    }

    /// <summary>Verifies that stopping a running host without a dispatcher fails immediately.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task StopAsync_WhenRunningWithoutDispatcher_ThrowsInvalidOperationException()
    {
        var service = CreateRunningService(new());

        await Assert.That(() => service.StopAsync(CancellationToken.None)).Throws<InvalidOperationException>();
    }

    /// <summary>Verifies that an already-cancelled stop request does not dispatch a UI shutdown callback.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task StopAsync_WhenCancelledBeforeDispatch_DoesNotDispatch()
    {
        var dispatchCount = 0;
        using var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();
        var service = CreateRunningService(new(), callback =>
        {
            dispatchCount++;
            callback();
            return true;
        });

        await Assert.That(() => service.StopAsync(cancellationTokenSource.Token)).Throws<OperationCanceledException>();
        await Assert.That(dispatchCount).IsEqualTo(0);
    }

    /// <summary>Verifies that a dispatcher which rejects the shutdown callback fails immediately.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task StopAsync_WhenDispatcherRejectsCallback_ThrowsInvalidOperationException()
    {
        var service = CreateRunningService(new(), static _ => false);

        await Assert.That(() => service.StopAsync(CancellationToken.None)).Throws<InvalidOperationException>();
    }

    /// <summary>Verifies that stopping a running host honours cancellation while its accepted callback remains pending.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task StopAsync_WhenCancelledWhileCallbackIsPending_ThrowsOperationCanceledException()
    {
        Action? callback = null;
        using var cancellationTokenSource = new CancellationTokenSource();
        var service = CreateRunningService(new(), dispatchedCallback =>
        {
            callback = dispatchedCallback;
            return true;
        });

        var stopTask = service.StopAsync(cancellationTokenSource.Token);
        await cancellationTokenSource.CancelAsync();

        await Assert.That(() => stopTask).Throws<OperationCanceledException>();
        await Assert.That(callback is not null).IsTrue();
    }

    /// <summary>Verifies that an accepted dispatcher callback completes application shutdown.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task StopAsync_WhenDispatcherAcceptsCallback_Completes()
    {
        var dispatchCount = 0;
        var service = CreateRunningService(new(), callback =>
        {
            dispatchCount++;
            callback();
            return true;
        });

        await service.StopAsync(CancellationToken.None);

        await Assert.That(dispatchCount).IsEqualTo(1);
    }

    /// <summary>Verifies that the thread-starter adapter validates its supplied start action.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task MauiThreadStarter_WithNullAction_ThrowsArgumentNullException() =>
        await Assert.That(static () => new MauiThreadStarter((Action)null!)).Throws<ArgumentNullException>();

    /// <summary>Verifies that the thread-starter adapter validates its supplied MAUI thread.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task MauiThreadStarter_WithNullMauiThread_ThrowsArgumentNullException() =>
        await Assert.That(static () => new MauiThreadStarter((MauiThread)null!)).Throws<ArgumentNullException>();

    /// <summary>Verifies that the composed hosted-service constructor validates required dependencies.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task Constructor_WithNullDependencies_ThrowsArgumentNullException()
    {
        var context = new TestMauiContext();
        var starter = new MauiThreadStarter(static () => { });

        await Assert.That(() => new MauiHostedService(null!, starter, context)).Throws<ArgumentNullException>();
        await Assert.That(() => new MauiHostedService(NullLogger<MauiHostedService>.Instance, (IUiThreadStarter)null!, context)).Throws<ArgumentNullException>();
        await Assert.That(() => new MauiHostedService(NullLogger<MauiHostedService>.Instance, starter, null!)).Throws<ArgumentNullException>();
    }

    /// <summary>Verifies that the public constructor continues to compose the MAUI thread implementation.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task Constructor_WithMauiThread_ComposesPublicThreadStarter()
    {
        var context = new TestMauiContext();
        var services = new ServiceCollection().AddSingleton<IMauiContext>(context);
        await using var serviceProvider = services.BuildServiceProvider();
        using var mauiThread = new MauiThread(serviceProvider);

        var service = new MauiHostedService(NullLogger<MauiHostedService>.Instance, mauiThread, context);

        await Assert.That(service).IsNotNull();
    }

    /// <summary>Creates a hosted service whose context reports an active MAUI application.</summary>
    /// <param name="context">The context to provide to the hosted service.</param>
    /// <param name="dispatch">The callback used to schedule shutdown work.</param>
    /// <returns>A hosted service configured for shutdown testing.</returns>
    private static MauiHostedService CreateRunningService(TestMauiContext context, Func<Action, bool>? dispatch = null)
    {
        context.IsRunning = true;
        context.Dispatcher = dispatch is null ? null : new TestDispatcher(dispatch);
        return new(NullLogger<MauiHostedService>.Instance, new MauiThreadStarter(static () => { }), context);
    }

    /// <summary>Provides a dispatcher-free context for lifecycle orchestration tests.</summary>
    private sealed class TestMauiContext : IMauiContext
    {
        /// <inheritdoc />
        public bool IsLifetimeLinked { get; set; }

        /// <inheritdoc />
        public bool IsRunning { get; set; }

        /// <inheritdoc />
        public Application? MauiApplication { get; set; }

        /// <inheritdoc />
        public IDispatcher? Dispatcher { get; set; }
    }

    /// <summary>Provides controllable dispatch behavior for hosted-service tests.</summary>
    private sealed class TestDispatcher : IDispatcher
    {
        /// <summary>Stores the configured dispatch behavior.</summary>
        private readonly Func<Action, bool> _dispatch;

        /// <summary>Initializes a new instance of the <see cref="TestDispatcher"/> class.</summary>
        /// <param name="dispatch">The callback used to implement dispatch behavior.</param>
        public TestDispatcher(Func<Action, bool> dispatch) =>
            _dispatch = dispatch ?? throw new ArgumentNullException(nameof(dispatch));

        /// <inheritdoc />
        public bool IsDispatchRequired => true;

        /// <inheritdoc />
        public bool Dispatch(Action action) =>
            _dispatch(action);

        /// <inheritdoc />
        public bool DispatchDelayed(TimeSpan delay, Action action) =>
            throw new NotSupportedException();

        /// <inheritdoc />
        public IDispatcherTimer CreateTimer() =>
            throw new NotSupportedException();
    }
}
