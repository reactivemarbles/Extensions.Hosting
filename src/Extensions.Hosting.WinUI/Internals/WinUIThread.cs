// Copyright (c) 2019-2024 ReactiveUI Association Incorporated. All rights reserved.
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
/// This contains the logic for the WinUI thread.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="WinUIThread"/> class.
/// This will create the WinUIThread.
/// </remarks>
/// <param name="serviceProvider">IServiceProvider.</param>
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
