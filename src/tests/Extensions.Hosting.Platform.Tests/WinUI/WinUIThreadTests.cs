// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using ReactiveMarbles.Extensions.Hosting.WinUI;
using ReactiveMarbles.Extensions.Hosting.WinUI.Internals;

namespace Extensions.Hosting.WinUI.Platform.Tests;

/// <summary>Tests the WinUI-thread startup flow without starting a native WinUI event loop.</summary>
public class WinUIThreadTests
{
    /// <summary>Defines the dispatcher polling interval in milliseconds.</summary>
    private const int DispatcherPollIntervalMilliseconds = 10;

    /// <summary>Defines the dispatcher polling interval.</summary>
    private static readonly TimeSpan DispatcherPollInterval = TimeSpan.FromMilliseconds(DispatcherPollIntervalMilliseconds);

    /// <summary>Defines the maximum time to await native WinUI-loop completion.</summary>
    private static readonly TimeSpan NativeLoopTimeout = TimeSpan.FromSeconds(15);

    /// <summary>Verifies that the testable constructor validates its supplied runtime.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task Constructor_WithNullRuntime_ThrowsArgumentNullException()
    {
        var services = new ServiceCollection().AddSingleton<IWinUIContext>(new TestWinUIContext());
        await using var serviceProvider = services.BuildServiceProvider();

        await Assert.That(() => new WinUIThread(serviceProvider, null!, useDedicatedUiThread: false)).Throws<ArgumentNullException>();
    }

    /// <summary>Verifies that the UI-thread startup flow initializes WinUI services and activates the configured window.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task Start_InitializesApplicationServicesAndMainWindow()
    {
        var context = new TestWinUIContext { AppWindowType = typeof(Window) };
        var initializedApplications = new List<Application?>();
        var runtime = new TestWinUIThreadRuntime();
        var services = new ServiceCollection()
            .AddSingleton<IWinUIContext>(context)
            .AddSingleton<IWinUIService>(new TestWinUIService(initializedApplications));
        await using var serviceProvider = services.BuildServiceProvider();
        using var winUIThread = new WinUIThread(serviceProvider, runtime, useDedicatedUiThread: false);
        var originalSynchronizationContext = SynchronizationContext.Current;

        try
        {
            winUIThread.Start();
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(originalSynchronizationContext);
        }

        await Assert.That(runtime.InitializedComWrappers).IsTrue();
        await Assert.That(runtime.Started).IsTrue();
        await Assert.That(initializedApplications).Count().IsEqualTo(1);
        await Assert.That(context.WinUIApplication).IsNull();
        await Assert.That(context.AppWindow).IsNull();
        await Assert.That(context.IsRunning).IsFalse();
    }

    /// <summary>Verifies the production WinUI runtime can own a complete native application-loop lifecycle.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    [NotInParallel]
    public async Task WinUIThreadRuntime_StartsAndStopsNativeApplicationLoop()
    {
        var completion = new TaskCompletionSource<Exception?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var runtime = new WinUIThreadRuntime();
        var configuredApplicationPreserved = false;
        Task? stopTask = null;
        var thread = new Thread(() =>
        {
            try
            {
                runtime.InitializeComWrappers();
                runtime.Start((dispatcher, synchronizationContext) =>
                {
                    var application = new RuntimeTestApplication();
                    var services = new ServiceCollection().AddSingleton<Application>(application);
                    using var serviceProvider = services.BuildServiceProvider();
                    var resolvedApplication = runtime.GetApplication(serviceProvider);
                    var window = runtime.CreateWindow(serviceProvider, typeof(RuntimeTestWindow));
                    var hostBuilder = Host.CreateApplicationBuilder();
                    _ = hostBuilder.ConfigureWinUI(typeof(RuntimeTestApplication), typeof(RuntimeTestWindow));
                    _ = hostBuilder.Services.AddSingleton(application);
                    using var hostServices = hostBuilder.Services.BuildServiceProvider();
                    configuredApplicationPreserved = ReferenceEquals(
                        hostServices.GetRequiredService<Application>(),
                        application);

                    runtime.ActivateWindow(window);
                    window.Close();

                    var context = new TestWinUIContext { Dispatcher = dispatcher, IsRunning = true, WinUIApplication = resolvedApplication };
                    var hostedService = new WinUIHostedService(
                        NullLogger<WinUIHostedService>.Instance,
                        new TestUiThreadStarter(),
                        context);
                    stopTask = hostedService.StopAsync(CancellationToken.None);
                });
                completion.SetResult(null);
            }
            catch (Exception exception)
            {
                completion.SetResult(exception);
            }
        });
        thread.IsBackground = true;
        thread.SetApartmentState(ApartmentState.STA);

        thread.Start();
        var exception = await completion.Task.WaitAsync(NativeLoopTimeout);

        await Assert.That(exception).IsNull();
        await Assert.That(configuredApplicationPreserved).IsTrue();
        await Assert.That(stopTask).IsNotNull();
        await Assert.That(stopTask!.IsCompletedSuccessfully).IsTrue();
    }

    /// <summary>Verifies that the public WinUI thread constructor starts through the default native runtime.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    [NotInParallel]
    public async Task Start_WithPublicConstructor_InitializesAndStopsNativeApplicationLoop()
    {
        var completion = new TaskCompletionSource<Exception?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var initializedApplications = new List<Application?>();
        var context = new WinUIContext { AppWindowType = typeof(RuntimeTestWindow) };
        var services = new ServiceCollection()
            .AddSingleton<IWinUIContext>(context)
            .AddSingleton<Application, RuntimeTestApplication>()
            .AddSingleton<IWinUIService>(new TestWinUIService(initializedApplications));
        await using var serviceProvider = services.BuildServiceProvider();
        using var winUIThread = new WinUIThread(serviceProvider);

        winUIThread.Start();

        _ = Task.Run(async () =>
        {
            try
            {
                using var timer = new PeriodicTimer(DispatcherPollInterval);
                while (context.Dispatcher is null)
                {
                    _ = await timer.WaitForNextTickAsync();
                }

                var dispatcher = context.Dispatcher;
                if (dispatcher is null)
                {
                    completion.SetResult(new InvalidOperationException("The dispatcher was not created."));
                    return;
                }

                if (!dispatcher.TryEnqueue(() =>
                {
                    try
                    {
                        context.AppWindow?.Close();
                        context.WinUIApplication?.Exit();
                        completion.SetResult(null);
                    }
                    catch (Exception exception)
                    {
                        completion.SetResult(exception);
                    }
                }))
                {
                    completion.SetResult(new InvalidOperationException("The dispatcher rejected the shutdown callback."));
                }
            }
            catch (Exception exception)
            {
                completion.SetResult(exception);
            }
        });

        var exception = await completion.Task.WaitAsync(NativeLoopTimeout);

        await Assert.That(exception).IsNull();
        await Assert.That(initializedApplications).Count().IsEqualTo(1);
        await Assert.That(context.WinUIApplication).IsNotNull();
        await Assert.That(context.AppWindow).IsNotNull();
        await Assert.That(context.Dispatcher).IsNotNull();
        await Assert.That(context.IsRunning).IsTrue();
    }

    /// <summary>Verifies the concrete WinUI context exposes default lifecycle and UI component state.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task WinUIContext_DefaultState_IsEmpty()
    {
        var context = new WinUIContext();

        await Assert.That(context.AppWindow).IsNull();
        await Assert.That(context.Dispatcher).IsNull();
        await Assert.That(context.IsRunning).IsFalse();
        await Assert.That(context.WinUIApplication).IsNull();
    }

    /// <summary>Provides a WinUI window for the production runtime lifecycle test.</summary>
    public sealed class RuntimeTestWindow : Window;

    /// <summary>Provides an in-memory WinUI application context for UI-thread tests.</summary>
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

    /// <summary>Provides a deterministic application-loop runtime for testing.</summary>
    private sealed class TestWinUIThreadRuntime : IWinUIThreadRuntime
    {
        /// <summary>Gets a value indicating whether COM wrappers were initialized.</summary>
        public bool InitializedComWrappers { get; private set; }

        /// <summary>Gets a value indicating whether the application loop was started.</summary>
        public bool Started { get; private set; }

        /// <inheritdoc />
        public void InitializeComWrappers() =>
            InitializedComWrappers = true;

        /// <inheritdoc />
        public void Start(Action<DispatcherQueue?, SynchronizationContext> initialize)
        {
            Started = true;
            initialize(null, new());
        }

        /// <inheritdoc />
        public Application GetApplication(IServiceProvider serviceProvider) =>
            GetNull<Application>();

        /// <inheritdoc />
        public Window CreateWindow(IServiceProvider serviceProvider, Type windowType) =>
            GetNull<Window>();

        /// <inheritdoc />
        public void ActivateWindow(Window window)
        {
        }

        /// <summary>Returns a null test double for a WinUI runtime component.</summary>
        /// <typeparam name="T">The component type.</typeparam>
        /// <returns>A null test double.</returns>
        private static T GetNull<T>()
            where T : class =>
            null!;
    }

    /// <summary>Provides a no-op UI-thread starter for native shutdown verification.</summary>
    private sealed class TestUiThreadStarter : IUiThreadStarter
    {
        /// <inheritdoc />
        public void Start()
        {
        }
    }

    /// <summary>Provides a WinUI application for the production runtime lifecycle test.</summary>
    private sealed class RuntimeTestApplication : Application;

    /// <summary>Records initialized WinUI application instances.</summary>
    /// <param name="initializedApplications">The collection that receives initialized applications.</param>
    private sealed class TestWinUIService(List<Application?> initializedApplications) : IWinUIService
    {
        /// <inheritdoc />
        public void Initialize(Application application) =>
            initializedApplications.Add(application);
    }
}
