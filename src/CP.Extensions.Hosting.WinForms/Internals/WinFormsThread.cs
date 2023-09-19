// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Threading;
using CP.Extensions.Hosting.UiThread;
using Microsoft.Extensions.DependencyInjection;

namespace CP.Extensions.Hosting.WinForms.Internals;

/// <summary>
/// This contains the logic for the WinForms thread.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="WinFormsThread"/> class.
/// Constructor which is called from the IWinFormsContext.
/// </remarks>
/// <param name="serviceProvider">IServiceProvider.</param>
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

    /// <summary>
    /// Start Windows Forms.
    /// </summary>
    protected override void UiThreadStart()
    {
        // Use the provided IWinFormsService
        var winFormServices = ServiceProvider.GetServices<IWinFormsService>();
        foreach (var winFormService in winFormServices)
        {
            winFormService.Initialize();
        }

        // Run the WPF application in this thread which was specifically created for it, with the specified shell
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

    private void OnApplicationExit(object? sender, EventArgs eventArgs)
    {
        Application.ApplicationExit -= OnApplicationExit;

        HandleApplicationExit();
    }
}
