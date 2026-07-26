// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using ReactiveMarbles.Extensions.Hosting.Maui;
using ReactiveMarbles.Extensions.Hosting.Maui.Internals;

namespace Extensions.Hosting.Maui.Platform.Tests;

/// <summary>Tests MAUI thread initialization through a deterministic dispatcher.</summary>
public class MauiThreadTests
{
    /// <summary>Defines the maximum time to await deterministic UI-thread initialization.</summary>
    private static readonly TimeSpan InitializationTimeout = TimeSpan.FromSeconds(5);

    /// <summary>Verifies the UI-thread callback creates an application and initializes registered MAUI services.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task Start_InitializesApplicationAndRegisteredMauiServices()
    {
        var context = new TestMauiContext(new ImmediateDispatcher());
        var initialized = new TaskCompletionSource<Application?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var services = new ServiceCollection()
            .AddSingleton<IMauiContext>(context)
            .AddSingleton<IMauiService>(new TestMauiService(initialized));
        await using var serviceProvider = services.BuildServiceProvider();
        using var mauiThread = new MauiThread(serviceProvider, new TestMauiApplicationStarter(), useDedicatedUiThread: false);

        mauiThread.Start();

        _ = await initialized.Task.WaitAsync(InitializationTimeout);
        await Assert.That(context.MauiApplication).IsNull();
        await Assert.That(context.IsRunning).IsTrue();
    }

    /// <summary>Verifies the concrete MAUI context tolerates the absence of a current application.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task MauiContext_WithoutCurrentApplication_HasNoDispatcher()
    {
        var context = new MauiContext();

        await Assert.That(context.Dispatcher).IsNull();
    }

    /// <summary>Provides an MAUI context backed by a deterministic dispatcher.</summary>
    /// <param name="dispatcher">The dispatcher used by the context.</param>
    private sealed class TestMauiContext(IDispatcher dispatcher) : IMauiContext
    {
        /// <inheritdoc />
        public bool IsLifetimeLinked { get; set; }

        /// <inheritdoc />
        public bool IsRunning { get; set; }

        /// <inheritdoc />
        public Application? MauiApplication { get; set; }

        /// <inheritdoc />
        public IDispatcher? Dispatcher { get; } = dispatcher;
    }

    /// <summary>Completes a task when the MAUI application service is initialized.</summary>
    /// <param name="initialized">The completion source for the initialized application.</param>
    private sealed class TestMauiService(TaskCompletionSource<Application?> initialized) : IMauiService
    {
        /// <inheritdoc />
        public void Initialize(Application application) =>
            initialized.SetResult(application);
    }

    /// <summary>Provides an uninitialized application instance without starting a platform UI runtime.</summary>
    private sealed class TestMauiApplicationStarter : IMauiApplicationStarter
    {
        /// <inheritdoc />
        public Application Create(IServiceProvider serviceProvider) =>
            null!;

        /// <inheritdoc />
        public void RegisterApplicationExit(Application mauiApplication, Action onApplicationExit)
        {
        }
    }

    /// <summary>Dispatches callbacks synchronously for deterministic tests.</summary>
    private sealed class ImmediateDispatcher : IDispatcher
    {
        /// <inheritdoc />
        public bool IsDispatchRequired => false;

        /// <inheritdoc />
        public bool Dispatch(Action action)
        {
            action();
            return true;
        }

        /// <inheritdoc />
        public bool DispatchDelayed(TimeSpan delay, Action action) =>
            Dispatch(action);

        /// <inheritdoc />
        public IDispatcherTimer CreateTimer() =>
            throw new NotSupportedException();
    }
}
