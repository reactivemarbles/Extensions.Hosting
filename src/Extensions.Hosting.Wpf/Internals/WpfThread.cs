// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using ReactiveMarbles.Extensions.Hosting.UiThread;

namespace ReactiveMarbles.Extensions.Hosting.Wpf.Internals;

/// <summary>Provides a dedicated UI thread for running a Windows Presentation Foundation (WPF) application, managing its lifecycle and synchronization context.</summary>
/// <remarks>WpfThread is intended for scenarios where a WPF application needs to be hosted on a separate thread,
/// such as in multi-threaded or headless environments. It sets up the necessary synchronization context and manages the
/// startup and shutdown of the WPF application. The type expects services implementing IWpfService and IWpfShell to be
/// registered with the provided IServiceProvider. Thread safety and correct service registration are the responsibility
/// of the caller.</remarks>
/// <param name="serviceProvider">The service provider used to resolve WPF application services, shell windows, and related dependencies required for
/// initializing and running the WPF UI thread. Cannot be null.</param>
public class WpfThread(IServiceProvider serviceProvider) : BaseUiThread<IWpfContext>(serviceProvider)
{
    /// <inheritdoc />
    protected override void PreUiThreadStart()
    {
        // Create our SynchronizationContext, and install it:
        SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher));

        // Create the new WPF application
        var wpfApplication = ServiceProvider.GetService<Application>() ?? new() { ShutdownMode = UiContext!.ShutdownMode };

        // Register to the WPF application exit to stop the host application
        _ = wpfApplication.Dispatcher.InvokeAsync(() => wpfApplication.Exit += (s, e) => HandleApplicationExit());

        // Store the application for others to interact
        UiContext!.WpfApplication = wpfApplication;
    }

    /// <inheritdoc />
    protected override void UiThreadStart()
    {
        var wpfApplication = UiContext.WpfApplication;
        if (wpfApplication is null)
        {
            return;
        }

        var currentThreadOwnsApplicationDispatcher = wpfApplication.Dispatcher.Thread == Thread.CurrentThread;

        wpfApplication.Dispatcher.Invoke(
            () => RunApplicationOnDispatcher(wpfApplication, currentThreadOwnsApplicationDispatcher));
    }

    /// <summary>Runs the configured WPF application from its dispatcher.</summary>
    /// <param name="wpfApplication">The WPF application to run.</param>
    /// <param name="currentThreadOwnsApplicationDispatcher">Whether the startup thread owns the application dispatcher.</param>
    private void RunApplicationOnDispatcher(Application wpfApplication, bool currentThreadOwnsApplicationDispatcher)
    {
        // Mark the application as running
        UiContext!.IsRunning = true;

        // Use the provided IWpfService
        foreach (var wpfService in ServiceProvider.GetServices<IWpfService>())
        {
            wpfService.Initialize(wpfApplication);
        }

        // Run the WPF application in this thread which was specifically created for it, with the specified shell
        var shellWindows = GetShellWindows();

        if (shellWindows.Count == 1)
        {
            if (currentThreadOwnsApplicationDispatcher)
            {
                _ = wpfApplication.Run(shellWindows[0]);
            }
            else if (wpfApplication.StartupUri is not null)
            {
                _ = MessageBox.Show("Please remove the StartupUri configuration in App.xaml");
            }
            else
            {
                shellWindows[0].Show();
            }

            return;
        }

        if (shellWindows.Count == 0)
        {
            if (currentThreadOwnsApplicationDispatcher)
            {
                _ = wpfApplication.Run();
            }
            else if (wpfApplication.MainWindow is not null)
            {
                wpfApplication.MainWindow.Show();
            }
            else
            {
                throw new InvalidOperationException("Please inherit from IWpfShell in a Window to use the required IWpfShell interface");
            }

            return;
        }

        wpfApplication.Startup += (sender, args) =>
        {
            foreach (var window in shellWindows)
            {
                window.Show();
            }
        };

        if (!currentThreadOwnsApplicationDispatcher)
        {
            return;
        }

        _ = wpfApplication.Run();
    }

    /// <summary>Resolves the registered WPF shell windows.</summary>
    /// <returns>The registered shell windows.</returns>
    private List<Window> GetShellWindows()
    {
        var shellWindows = new List<Window>();
        foreach (var shell in ServiceProvider.GetServices<IWpfShell>())
        {
            if (shell is Window window)
            {
                shellWindows.Add(window);
            }
        }

        return shellWindows;
    }
}
