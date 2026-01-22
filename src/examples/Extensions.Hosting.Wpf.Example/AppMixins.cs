// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Extensions.Hosting.Wpf.Example;

/// <summary>
/// AppMixins.
/// </summary>
public static class AppMixins
{
    private const string HostSettingsFile = "hostsettings.json";
    private const string AppSettingsFilePrefix = "appsettings";
    private const string Prefix = "PREFIX_";

    internal static IHostBuilder ConfigureLogging(this IHostBuilder hostBuilder) =>
        hostBuilder.ConfigureLogging((hostContext, configLogging) =>
            configLogging
                .AddConfiguration(hostContext.Configuration.GetSection("Logging"))
                .AddConsole()
                .AddDebug());

    internal static IHostBuilder ConfigureConfiguration(this IHostBuilder hostBuilder, string[] args) =>
        hostBuilder.ConfigureHostConfiguration(configHost => configHost.SetBasePath(Directory.GetCurrentDirectory())
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
