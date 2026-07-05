// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using Avalonia;
using Microsoft.Extensions.Hosting;
using ReactiveMarbles.Extensions.Hosting.Avalonia;

namespace Extensions.Hosting.Avalonia.Example;

/// <summary>Provides the entry point for the application and contains methods for configuring the host, logging, and application settings.</summary>
/// <remarks>The Program class is responsible for initializing and starting the Avalonia application using a
/// generic host. It configures logging, application configuration sources, and sets up the Avalonia application
/// lifecycle, including the main window. This class is intended to be the starting point for the application and should
/// not be instantiated.</remarks>
public static class Program
{
    /// <summary>Initializes and runs the Avalonia application with configured logging, application settings, and main window.</summary>
    /// <remarks>This method sets up the application host with logging, configuration, and Avalonia-specific
    /// settings, including the main application and window. The application runs until it is closed or
    /// terminated.</remarks>
    /// <param name="args">An array of command-line arguments that configure application behavior at startup.</param>
    /// <returns>The application exit code.</returns>
    [STAThread]
    public static int Main(string[] args)
    {
        var host = new HostBuilder()
            .ConfigureLogging()
            .ConfigureConfiguration(args)
            .ConfigureAvalonia(avaloniaBuilder =>
            {
                // Use the custom application
                _ = avaloniaBuilder.UseApplication<App>();

                // Use the main window
                _ = avaloniaBuilder.UseWindow<MainWindow>();
            })
            .UseAvaloniaLifetime()
            .UseConsoleLifetime()
            .Build();

        Console.WriteLine("Run!");

        host.Run();

        return 0;
    }
}
