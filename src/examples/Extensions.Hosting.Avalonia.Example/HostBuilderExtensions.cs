// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Extensions.Hosting.Avalonia.Example;

/// <summary>Provides host builder extension methods for the sample Avalonia application.</summary>
public static class HostBuilderExtensions
{
    /// <summary>Stores the app settings file prefix value.</summary>
    private const string AppSettingsFilePrefix = "appsettings";

    /// <summary>Stores the host settings file value.</summary>
    private const string HostSettingsFile = "hostsettings.json";

    /// <summary>Stores the prefix value.</summary>
    private const string Prefix = "PREFIX_";

    /// <summary>Provides extension members for this receiver.</summary>
    /// <param name="hostBuilder">The receiver instance.</param>
    extension(IHostBuilder hostBuilder)
    {
        /// <summary>Configures logging for the host builder using settings from the application's configuration.</summary>
        /// <remarks>This method adds logging configuration from the "Logging" section of the application's
        /// configuration and enables both console and debug logging providers.</remarks>
        /// <returns>The same host builder instance, enabling method chaining.</returns>
        internal IHostBuilder ConfigureLogging() =>
            hostBuilder.ConfigureLogging((hostContext, configLogging) =>
                configLogging
                    .AddConfiguration(hostContext.Configuration.GetSection("Logging"))
                    .AddConsole()
                    .AddDebug());

        /// <summary>Configures the host and application settings for the host builder using JSON files, environment variables, and command-line arguments.</summary>
        /// <remarks>This method sets the base path for configuration files to the current directory and
        /// supports optional JSON files for both host and application settings based on the hosting
        /// environment.</remarks>
        /// <param name="args">An array of command-line arguments that can influence the configuration of the application.</param>
        /// <returns>The configured host builder instance, allowing for further configuration or execution.</returns>
        internal IHostBuilder ConfigureConfiguration(string[] args) =>
            hostBuilder.ConfigureHostConfiguration(configHost =>
                configHost.SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile(HostSettingsFile, optional: true)
                    .AddEnvironmentVariables(prefix: Prefix)
                    .AddCommandLine(args))
                .ConfigureAppConfiguration((hostContext, configApp) =>
                {
                    _ = configApp.AddJsonFile(AppSettingsFilePrefix + ".json", optional: true);
                    if (!string.IsNullOrEmpty(hostContext.HostingEnvironment.EnvironmentName))
                    {
                        _ = configApp.AddJsonFile(AppSettingsFilePrefix + $".{hostContext.HostingEnvironment.EnvironmentName}.json", optional: true);
                    }

                    _ = configApp.AddEnvironmentVariables(prefix: Prefix)
                        .AddCommandLine(args);
                });
    }
}
