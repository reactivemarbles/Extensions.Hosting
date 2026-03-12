// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReactiveMarbles.Extensions.Hosting.Avalonia;

namespace Extensions.Hosting.Avalonia.Example;

/// <summary>
/// Provides the entry point for the application and contains methods for configuring the host, logging, and application
/// settings.
/// </summary>
/// <remarks>The Program class is responsible for initializing and starting the Avalonia application using a
/// generic host. It configures logging, application configuration sources, and sets up the Avalonia application
/// lifecycle, including the main window. This class is intended to be the starting point for the application and should
/// not be instantiated.</remarks>
public static class Program
{
    private const string AppSettingsFilePrefix = "appsettings";
    private const string HostSettingsFile = "hostsettings.json";
    private const string Prefix = "PREFIX_";

    /// <summary>
    /// Initializes and runs the Avalonia application with configured logging, application settings, and main window.
    /// </summary>
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
                avaloniaBuilder.UseApplication<App>();

                // Use the main window
                avaloniaBuilder.UseWindow<MainWindow>();
            })
            .UseAvaloniaLifetime()
            .UseConsoleLifetime()
            .Build();

        Console.WriteLine("Run!");

        host.Run();

        return 0;
    }

    /// <summary>
    /// Configures logging for the host builder using settings from the application's configuration.
    /// </summary>
    /// <remarks>This method adds logging configuration from the "Logging" section of the application's
    /// configuration and enables both console and debug logging providers.</remarks>
    /// <param name="hostBuilder">The host builder to configure logging for. This parameter cannot be null.</param>
    /// <returns>The same host builder instance, enabling method chaining.</returns>
    private static IHostBuilder ConfigureLogging(this IHostBuilder hostBuilder) =>
        hostBuilder.ConfigureLogging((hostContext, configLogging) =>
            configLogging
                .AddConfiguration(hostContext.Configuration.GetSection("Logging"))
                .AddConsole()
                .AddDebug());

    /// <summary>
    /// Configures the host and application settings for the host builder using JSON files, environment variables,
    /// and command-line arguments.
    /// </summary>
    /// <remarks>This method sets the base path for configuration files to the current directory and
    /// supports optional JSON files for both host and application settings based on the hosting
    /// environment.</remarks>
    /// <param name="hostBuilder">The host builder instance to configure with the specified settings.</param>
    /// <param name="args">An array of command-line arguments that can influence the configuration of the application.</param>
    /// <returns>The configured host builder instance, allowing for further configuration or execution.</returns>
    private static IHostBuilder ConfigureConfiguration(this IHostBuilder hostBuilder, string[] args) =>
        hostBuilder.ConfigureHostConfiguration(configHost =>
            configHost.SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile(HostSettingsFile, optional: true)
                    .AddEnvironmentVariables(prefix: Prefix)
                    .AddCommandLine(args))
            .ConfigureAppConfiguration((hostContext, configApp) =>
            {
                configApp.AddJsonFile(AppSettingsFilePrefix + ".json", optional: true);
                if (!string.IsNullOrEmpty(hostContext.HostingEnvironment.EnvironmentName))
                {
                    configApp.AddJsonFile(AppSettingsFilePrefix + $".{hostContext.HostingEnvironment.EnvironmentName}.json", optional: true);
                }

                configApp.AddEnvironmentVariables(prefix: Prefix)
                    .AddCommandLine(args);
            });
}
