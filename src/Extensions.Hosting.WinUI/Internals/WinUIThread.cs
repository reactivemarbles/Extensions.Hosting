// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using ReactiveMarbles.Extensions.Hosting.UiThread;

namespace ReactiveMarbles.Extensions.Hosting.WinUI.Internals;

/// <summary>Provides a UI thread implementation for WinUI applications, enabling initialization and management of WinUI-specific services and application context.</summary>
/// <remarks>This class is intended for scenarios where a dedicated UI thread is required to host a WinUI
/// application. It ensures that the necessary WinUI services and synchronization context are initialized on the correct
/// thread. The WinUI application and its main window are created and activated as part of the thread startup
/// process.</remarks>
public class WinUIThread : BaseUiThread<IWinUIContext>
{
    /// <summary>Stores the default platform application-loop runtime.</summary>
    private static readonly IWinUIThreadRuntime DefaultRuntime = new WinUIThreadRuntime();

    /// <summary>Stores the platform application-loop runtime.</summary>
    private readonly IWinUIThreadRuntime? _runtime;

    /// <summary>Initializes a new instance of the <see cref="WinUIThread"/> class.</summary>
    /// <param name="serviceProvider">The service provider used to resolve WinUI application services and dependencies.</param>
    public WinUIThread(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="WinUIThread"/> class for controlled platform-runtime execution.</summary>
    /// <param name="serviceProvider">The service provider used to resolve WinUI application services and dependencies.</param>
    /// <param name="runtime">The platform application-loop runtime.</param>
    /// <param name="useDedicatedUiThread">Whether to start work on a dedicated UI thread.</param>
    internal WinUIThread(IServiceProvider serviceProvider, IWinUIThreadRuntime runtime, bool useDedicatedUiThread)
        : base(serviceProvider, useDedicatedUiThread)
    {
        _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
    }

    /// <inheritdoc />
    protected override void PreUiThreadStart() =>
        (_runtime ?? DefaultRuntime).InitializeComWrappers();

    /// <inheritdoc />
    protected override void UiThreadStart()
    {
        (_runtime ?? DefaultRuntime).Start((dispatcher, synchronizationContext) =>
        {
            UiContext!.Dispatcher = dispatcher;
            SynchronizationContext.SetSynchronizationContext(synchronizationContext);

            var application = (_runtime ?? DefaultRuntime).GetApplication(ServiceProvider);
            UiContext.WinUIApplication = application;

            // Use the provided IWinUIService
            var winUIServices = ServiceProvider.GetServices<IWinUIService>();
            if (winUIServices is not null)
            {
                foreach (var winUIService in winUIServices)
                {
                    winUIService.Initialize(application);
                }
            }

            var appWindow = (_runtime ?? DefaultRuntime).CreateWindow(ServiceProvider, UiContext.AppWindowType!);
            UiContext.AppWindow = appWindow;
            (_runtime ?? DefaultRuntime).ActivateWindow(appWindow);
        });
        HandleApplicationExit();
    }
}
