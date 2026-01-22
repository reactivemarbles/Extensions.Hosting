// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using ReactiveMarbles.Extensions.Hosting.UiThread;
using WinRT;

namespace ReactiveMarbles.Extensions.Hosting.WinUI.Internals;

/// <summary>
/// Provides a UI thread implementation for WinUI applications, enabling initialization and management of WinUI-specific
/// services and application context.
/// </summary>
/// <remarks>This class is intended for scenarios where a dedicated UI thread is required to host a WinUI
/// application. It ensures that the necessary WinUI services and synchronization context are initialized on the correct
/// thread. The WinUI application and its main window are created and activated as part of the thread startup
/// process.</remarks>
/// <param name="serviceProvider">The service provider used to resolve WinUI application services and dependencies. Cannot be null.</param>
public class WinUIThread(IServiceProvider serviceProvider) : BaseUiThread<IWinUIContext>(serviceProvider)
{
    /// <inheritdoc />
    protected override void PreUiThreadStart() =>
        ComWrappersSupport.InitializeComWrappers();

    /// <inheritdoc />
    protected override void UiThreadStart()
    {
        Application.Start(_ =>
        {
            UiContext!.Dispatcher = DispatcherQueue.GetForCurrentThread();
            var context = new DispatcherQueueSynchronizationContext(UiContext.Dispatcher);
            SynchronizationContext.SetSynchronizationContext(context);

            UiContext.WinUIApplication = ServiceProvider.GetRequiredService<Application>();

            // Use the provided IWinUIService
            var winUIServices = ServiceProvider.GetServices<IWinUIService>();
            if (winUIServices != null)
            {
                foreach (var winUIService in winUIServices)
                {
                    winUIService.Initialize(UiContext.WinUIApplication);
                }
            }

            UiContext.AppWindow = (Window)ActivatorUtilities.CreateInstance(ServiceProvider, UiContext.AppWindowType!);
            UiContext.AppWindow.Activate();
        });
        HandleApplicationExit();
    }
}
