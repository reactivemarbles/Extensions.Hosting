// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Microsoft.Extensions.DependencyInjection;
using ReactiveMarbles.Extensions.Hosting.UiThread;

namespace ReactiveMarbles.Extensions.Hosting.Avalonia.Internals;

/// <summary>Manages the Avalonia UI thread and application lifetime for desktop applications, ensuring proper initialization and integration of Avalonia services and shell windows.</summary>
/// <remarks>This class is responsible for starting the Avalonia UI thread, initializing the application lifetime,
/// and registering Avalonia-specific services. It ensures that the application is properly set up and that any shell
/// windows are displayed according to the application's startup configuration.</remarks>
/// <param name="serviceProvider">The service provider used to resolve application services and dependencies required by the UI thread.</param>
/// <param name="appBuilder">The application builder used to configure and set up the Avalonia application before the UI thread starts.</param>
public class AvaloniaThread(IServiceProvider serviceProvider, AppBuilder appBuilder) : BaseUiThread<IAvaloniaContext>(serviceProvider, useDedicatedUiThread: false)
{
    /// <inheritdoc />
    protected override void PreUiThreadStart()
    {
    }

    /// <inheritdoc />
    protected override void UiThreadStart()
    {
        _ = appBuilder.StartWithClassicDesktopLifetime([], lifetime =>
        {
            lifetime.ShutdownMode = UiContext.ShutdownMode;
            UiContext.ApplicationLifetime = lifetime;

            lifetime.Exit += (s, e) => HandleApplicationExit();
            lifetime.Startup += (s, e) =>
            {
                UiContext.AvaloniaApplication = Application.Current ?? ServiceProvider.GetService<Application>()
                    ?? throw new InvalidOperationException("Unable to initialize the Avalonia application.");

                foreach (var avaloniaService in ServiceProvider.GetServices<IAvaloniaService>())
                {
                    avaloniaService.Initialize(UiContext.AvaloniaApplication);
                }

                var shellWindows = ServiceProvider.GetServices<IAvaloniaShell>().Cast<Window>().ToList();

                switch (shellWindows.Count)
                {
                    case 1:
                    {
                        lifetime.MainWindow = shellWindows[0];
                        shellWindows[0].Show();
                        break;
                    }

                    case 0:
                    {
                        break;
                    }

                    default:
                    {
                        lifetime.MainWindow = shellWindows[0];

                        for (var i = 0; i < shellWindows.Count; i++)
                        {
                            shellWindows[i]?.Show();
                        }

                        break;
                    }
                }

                UiContext.IsRunning = true;
            };
        });

        HandleApplicationExit();
    }
}
