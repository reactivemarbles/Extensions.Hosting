// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using ReactiveMarbles.Extensions.Hosting.UiThread;

namespace ReactiveMarbles.Extensions.Hosting.Wpf.Internals;

/// <summary>
/// This contains the logic for the WPF thread.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="WpfThread"/> class.
/// This will create the WpfThread.
/// </remarks>
/// <param name="serviceProvider">IServiceProvider.</param>
public class WpfThread(IServiceProvider serviceProvider) : BaseUiThread<IWpfContext>(serviceProvider)
{
    /// <inheritdoc />
    protected override void PreUiThreadStart()
    {
        // Create our SynchronizationContext, and install it:
        SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher));

        // Create the new WPF application
        var wpfApplication = ServiceProvider.GetService<Application>() ?? new()
        {
            ShutdownMode = UiContext!.ShutdownMode
        };

        // Register to the WPF application exit to stop the host application
        wpfApplication.Dispatcher.InvokeAsync(() => wpfApplication.Exit += (s, e) => HandleApplicationExit());

        // Store the application for others to interact
        UiContext!.WpfApplication = wpfApplication;
    }

    /// <inheritdoc />
    protected override void UiThreadStart() =>
        UiContext?.WpfApplication?.Dispatcher.Invoke(() =>
        {
            // Mark the application as running
            UiContext.IsRunning = true;

            // Use the provided IWpfService
            var wpfServices = ServiceProvider.GetServices<IWpfService>();
            foreach (var wpfService in wpfServices)
            {
                wpfService.Initialize(UiContext.WpfApplication);
            }

            // Run the WPF application in this thread which was specifically created for it, with the specified shell
            var shellWindows = ServiceProvider.GetServices<IWpfShell>().Cast<Window>().ToList();

            switch (shellWindows.Count)
            {
                case 1:
                    if (UiContext.WpfApplication.Dispatcher.Thread.ThreadState != ThreadState.Running)
                    {
                        UiContext.WpfApplication.Run(shellWindows[0]);
                    }
                    else if (UiContext.WpfApplication.StartupUri != null)
                    {
                        MessageBox.Show("Please remove the StartupUri configuration in App.xaml");
                    }
                    else
                    {
                        shellWindows[0].Show();
                    }

                    break;
                case 0:
                    if (UiContext.WpfApplication.Dispatcher.Thread.ThreadState != ThreadState.Running)
                    {
                        UiContext.WpfApplication.Run();
                    }
                    else if (UiContext.WpfApplication.MainWindow != null)
                    {
                        // show window if possible
                        UiContext.WpfApplication.MainWindow.Show();
                    }
                    else
                    {
                        throw new InvalidOperationException("Please inherit from IWpfShell in a Window to use the required IWpfShell interface");
                    }

                    break;
                default:
                    UiContext.WpfApplication.Startup += (sender, args) =>
                    {
                        foreach (var window in shellWindows)
                        {
                            window?.Show();
                        }
                    };

                    if (UiContext.WpfApplication.Dispatcher.Thread.ThreadState != ThreadState.Running)
                    {
                        UiContext.WpfApplication.Run();
                    }

                    break;
            }
        });
}
