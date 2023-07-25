// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using CP.Extensions.Hosting.UiThread;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using WinRT;

namespace CP.Extensions.Hosting.WinUI.Internals;

/// <summary>
/// This contains the logic for the WinUI thread.
/// </summary>
public class WinUIThread : BaseUiThread<IWinUIContext>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WinUIThread"/> class.
    /// This will create the WinUIThread.
    /// </summary>
    /// <param name="serviceProvider">IServiceProvider.</param>
    public WinUIThread(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }

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
