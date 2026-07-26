// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReactiveMarbles.Extensions.Hosting.AppServices;
using ReactiveMarbles.Extensions.Hosting.Avalonia;
using ReactiveMarbles.Extensions.Hosting.ReactiveUI;
using ReactiveUI;

namespace Extensions.Hosting.ReactiveAvalonia.Example;

/// <summary>Provides the entry point for the application and contains methods for configuring the host, logging, and application settings.</summary>
/// <remarks>The Program class is responsible for initializing and starting the Avalonia application using a
/// generic host. It configures logging, application configuration sources, and sets up the Avalonia application
/// lifecycle, including the main window. This class is intended to be the starting point for the application and should
/// not be instantiated.</remarks>
public static class Program
{
    /// <summary>Logs that another instance is already running.</summary>
    private static readonly Action<ILogger, string, Exception?> ApplicationAlreadyRunning =
        LoggerMessage.Define<string>(LogLevel.Warning, new(1, nameof(ApplicationAlreadyRunning)), "Application {ApplicationName} already running.");

    /// <summary>Logs that the application is running.</summary>
    private static readonly Action<ILogger, Exception?> ApplicationRunning =
        LoggerMessage.Define(LogLevel.Information, new(2, nameof(ApplicationRunning)), "Application running.");

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
            .ConfigureSplatForMicrosoftDependencyResolver()
            .ConfigureLogging()
            .ConfigureConfiguration(args)
            .ConfigureSingleInstance(static builder =>
            {
                builder.MutexId = "{691A4D6D-2CE0-4D47-B7F4-D99D8C02161E}";
                builder.WhenNotFirstInstance = static (hostingEnvironment, logger) =>
                    ApplicationAlreadyRunning(logger, hostingEnvironment.ApplicationName, null);
            })
            .ConfigureServices(static services => services.AddTransient<IViewFor<NugetDetailsViewModel>, NugetDetailsView>())
            .ConfigureAvalonia(static avaloniaBuilder =>
            {
                _ = avaloniaBuilder.UseApplication<App>();
                _ = avaloniaBuilder.UseWindow<MainWindow>();
            })
            .UseAvaloniaLifetime()
            .UseConsoleLifetime()
            .Build();

        var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(Program));
        ApplicationRunning(logger, null);

        host.Run();

        return 0;
    }
}
