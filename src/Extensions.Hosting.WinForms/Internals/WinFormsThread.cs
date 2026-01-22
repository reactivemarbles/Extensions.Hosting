// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using ReactiveMarbles.Extensions.Hosting.UiThread;

namespace ReactiveMarbles.Extensions.Hosting.WinForms.Internals;

/// <summary>
/// Provides a dedicated UI thread for running Windows Forms applications with dependency injection support.
/// </summary>
/// <remarks>This class manages the lifecycle of a Windows Forms message loop on a separate thread, allowing for
/// integration with dependency injection and service-based architectures. It is typically used in scenarios where
/// Windows Forms UI components need to be hosted or controlled from a background or non-main thread context. The thread
/// automatically configures the synchronization context and visual styles as needed. Thread safety and correct disposal
/// of UI resources are the responsibility of the caller.</remarks>
/// <param name="serviceProvider">The service provider used to resolve Windows Forms services and shells required by the UI thread. Cannot be null.</param>
public class WinFormsThread(IServiceProvider serviceProvider) : BaseUiThread<IWinFormsContext>(serviceProvider)
{
    /// <inheritdoc />
    protected override void PreUiThreadStart()
    {
        var currentDispatcher = Dispatcher.CurrentDispatcher;
        UiContext!.Dispatcher = currentDispatcher;

        // Create our SynchronizationContext, and install it:
        SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(currentDispatcher));

        if (UiContext.EnableVisualStyles)
        {
            Application.EnableVisualStyles();
        }

        // Register to the WinForms application exit to stop the host application
        Application.ApplicationExit += OnApplicationExit;
    }

    /// <inheritdoc />
    protected override void UiThreadStart()
    {
        // Use the provided IWinFormsService
        var winFormServices = ServiceProvider.GetServices<IWinFormsService>();
        foreach (var winFormService in winFormServices)
        {
            winFormService.Initialize();
        }

        // Run the WinForms application in this thread which was specifically created for it, with the specified shell
        var shells = ServiceProvider.GetServices<IWinFormsShell>().Cast<Form>().ToArray();

        switch (shells.Length)
        {
            case 1:
                Application.Run(shells[0]);
                break;
            case 0:
                Application.Run();
                break;
            default:
                var multiShellContext = new MultiShellContext(shells);
                Application.Run(multiShellContext);
                break;
        }
    }

    /// <summary>
    /// Handles the application exit event to perform necessary cleanup operations before the application shuts down.
    /// </summary>
    /// <param name="sender">The source of the event, typically the application object.</param>
    /// <param name="eventArgs">An object that contains the event data associated with the application exit event.</param>
    private void OnApplicationExit(object? sender, EventArgs eventArgs)
    {
        Application.ApplicationExit -= OnApplicationExit;

        HandleApplicationExit();
    }
}
