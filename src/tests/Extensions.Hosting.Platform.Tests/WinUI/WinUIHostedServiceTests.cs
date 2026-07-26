// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using ReactiveMarbles.Extensions.Hosting.WinUI;
using ReactiveMarbles.Extensions.Hosting.WinUI.Internals;

namespace Extensions.Hosting.WinUI.Platform.Tests;

/// <summary>Tests WinUI hosted-service orchestration without an App SDK UI loop.</summary>
public class WinUIHostedServiceTests
{
    /// <summary>Verifies that starting a non-cancelled host starts its composed UI-thread starter.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task StartAsync_WhenNotCancelled_StartsComposedThreadStarter()
    {
        var starts = 0;
        var service = new WinUIHostedService(
            NullLogger<WinUIHostedService>.Instance,
            new WinUIThreadStarter(() => starts++),
            new TestWinUIContext());

        await service.StartAsync(CancellationToken.None);

        await Assert.That(starts).IsEqualTo(1);
    }

    /// <summary>Verifies that starting a cancelled host does not start its composed UI-thread starter.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task StartAsync_WhenCancelled_DoesNotStartComposedThreadStarter()
    {
        var starts = 0;
        var service = new WinUIHostedService(
            NullLogger<WinUIHostedService>.Instance,
            new WinUIThreadStarter(() => starts++),
            new TestWinUIContext());
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
        var service = new WinUIHostedService(
            NullLogger<WinUIHostedService>.Instance,
            new WinUIThreadStarter(static () => { }),
            new TestWinUIContext());

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

    /// <summary>Verifies that an already-cancelled stop request does not enqueue a UI shutdown callback.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task StopAsync_WhenCancelledBeforeEnqueue_DoesNotEnqueue()
    {
        var enqueueCount = 0;
        using var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();
        var service = CreateRunningService(new(), callback =>
        {
            enqueueCount++;
            callback();
            return true;
        });

        await Assert.That(() => service.StopAsync(cancellationTokenSource.Token)).Throws<OperationCanceledException>();
        await Assert.That(enqueueCount).IsEqualTo(0);
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
    [NotInParallel]
    public async Task StopAsync_WhenCancelledWhileCallbackIsPending_ThrowsOperationCanceledException()
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        var dispatcherQueueController = EnsureDispatcherQueue();
        var service = CreateRunningService(new() { Dispatcher = DispatcherQueue.GetForCurrentThread() });

        var stopTask = service.StopAsync(cancellationTokenSource.Token);
        await cancellationTokenSource.CancelAsync();

        await Assert.That(() => stopTask).Throws<OperationCanceledException>();
        GC.KeepAlive(dispatcherQueueController);
    }

    /// <summary>Verifies that an accepted dispatcher callback completes shutdown after requesting application exit.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task StopAsync_WhenDispatcherAcceptsCallback_Completes()
    {
        var service = CreateRunningService(new(), static callback =>
        {
            callback();
            return true;
        });

        await service.StopAsync(CancellationToken.None);
    }

    /// <summary>Verifies that the thread-starter adapter validates its supplied start action.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task WinUIThreadStarter_WithNullAction_ThrowsArgumentNullException() =>
        await Assert.That(static () => new WinUIThreadStarter((Action)null!)).Throws<ArgumentNullException>();

    /// <summary>Verifies that the composed constructor rejects each required dependency.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task Constructor_WithNullRequiredDependency_ThrowsArgumentNullException()
    {
        var starter = new WinUIThreadStarter(static () => { });
        var context = new TestWinUIContext();

        await Assert.That(() => new WinUIHostedService(null!, starter, context)).Throws<ArgumentNullException>();
        await Assert.That(() => new WinUIHostedService(NullLogger<WinUIHostedService>.Instance, (IUiThreadStarter)null!, context)).Throws<ArgumentNullException>();
        await Assert.That(() => new WinUIHostedService(NullLogger<WinUIHostedService>.Instance, starter, null!)).Throws<ArgumentNullException>();
    }

    /// <summary>Verifies that the public constructor continues to compose the WinUI thread implementation.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task Constructor_WithWinUIThread_ComposesPublicThreadStarter()
    {
        var context = new TestWinUIContext();
        var services = new ServiceCollection().AddSingleton<IWinUIContext>(context);
        await using var serviceProvider = services.BuildServiceProvider();
        using var winUIThread = new WinUIThread(serviceProvider);

        var service = new WinUIHostedService(NullLogger<WinUIHostedService>.Instance, winUIThread, context);

        await Assert.That(service).IsNotNull();
    }

    /// <summary>Creates a hosted service whose context reports an active WinUI application.</summary>
    /// <param name="context">The context to provide to the hosted service.</param>
    /// <param name="tryEnqueue">The callback used to schedule shutdown work.</param>
    /// <returns>A hosted service configured for shutdown testing.</returns>
    private static WinUIHostedService CreateRunningService(TestWinUIContext context, Func<Action, bool>? tryEnqueue = null)
    {
        context.IsRunning = true;
        return tryEnqueue is null
            ? new(NullLogger<WinUIHostedService>.Instance, new WinUIThreadStarter(static () => { }), context)
            : new(NullLogger<WinUIHostedService>.Instance, new WinUIThreadStarter(static () => { }), context, tryEnqueue);
    }

    /// <summary>Creates a dispatcher queue for the current test thread when one is not already present.</summary>
    /// <returns>The controller that owns a newly created queue, or <see langword="null"/> when the thread already has a queue.</returns>
    private static DispatcherQueueController? EnsureDispatcherQueue() =>
        DispatcherQueue.GetForCurrentThread() is null ? DispatcherQueueController.CreateOnCurrentThread() : null;

    /// <summary>Provides a dispatcher-free context for lifecycle orchestration tests.</summary>
    private sealed class TestWinUIContext : IWinUIContext
    {
        /// <inheritdoc />
        public Window? AppWindow { get; set; }

        /// <inheritdoc />
        public Type? AppWindowType { get; set; }

        /// <inheritdoc />
        public DispatcherQueue? Dispatcher { get; set; }

        /// <inheritdoc />
        public bool IsLifetimeLinked { get; set; }

        /// <inheritdoc />
        public bool IsRunning { get; set; }

        /// <inheritdoc />
        public Application? WinUIApplication { get; set; }
    }
}
