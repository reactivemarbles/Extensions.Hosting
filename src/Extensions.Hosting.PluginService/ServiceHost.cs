﻿// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReactiveMarbles.Extensions.Hosting.Plugins;

namespace ReactiveMarbles.Extensions.Hosting.PluginService;

/// <summary>
/// ServiceHost.
/// </summary>
public static class ServiceHost
{
    private const string AppSettingsFilePrefix = "appsettings";
    private const string HostSettingsFile = "hostsettings.json";
    private const string Prefix = "PREFIX_";
    private static IHostBuilder? _builder;
    private static DefaultLogger? _logger;

    /// <summary>
    /// Gets the logger.
    /// </summary>
    /// <value>
    /// The logger.
    /// </value>
    public static ILogger? Logger => _logger?.Logger;

    /// <summary>
    /// Creates the specified host builder.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="args">The arguments.</param>
    /// <param name="hostBuilder">The host builder.</param>
    /// <param name="configureHost">The configure host.</param>
    /// <param name="nameSpace">The plugin name space.</param>
    /// <param name="targetRuntime">The target runtime folder for plugins.</param>
    /// <returns>
    /// A Task.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">type.</exception>
    public static Task Create(
        Type type,
        string[] args,
        Func<IHostBuilder?, IHostBuilder?>? hostBuilder = default,
        Action<IHost>? configureHost = default,
        string nameSpace = "ReactiveMarbles.Plugin",
        string? targetRuntime = null)
    {
        if (type == null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        var executableLocation = Path.GetDirectoryName(type.Assembly.Location);
        var isService = (!Debugger.IsAttached) && (args?.Length == 0 || args![0].IndexOf("--console", StringComparison.InvariantCultureIgnoreCase) < 0);
        if (isService)
        {
            var pathToExe = Process.GetCurrentProcess().MainModule?.FileName;
            var pathToContentRoot = Path.GetDirectoryName(pathToExe);
            Directory.SetCurrentDirectory(pathToContentRoot!);
        }

        _builder = Host.CreateDefaultBuilder()
            .UseContentRoot(Directory.GetCurrentDirectory())
            .ConfigureLogging()
            .ConfigureExternal(hostBuilder)
            .ConfigureConfiguration(args)
            .ConfigurePlugins(pluginBuilder =>
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Running using dotNet {0}", Environment.Version);

                //// Specify the location from where the Dll's are "globbed"
                var process = Process.GetCurrentProcess();
                var fullPath = process.MainModule?.FileName?.Replace(process.MainModule.ModuleName!, string.Empty);
                Console.WriteLine("Add Scan Directories: {0}", fullPath);
                pluginBuilder?.AddScanDirectories(fullPath!);

                //// Add the framework libraries which can be found with the specified globs
                pluginBuilder?.IncludeFrameworks(@"\netstandard2.0\*.FrameworkLib.dll");

                //// Add the plugins which can be found with the specified globs
                var runtime = targetRuntime ?? Path.GetFileName(executableLocation);
                Console.WriteLine(@"Include Plugins from: \Plugins\{0}\{1}*.dll", runtime, nameSpace);
                pluginBuilder?.IncludePlugins(@$"\Plugins\{runtime}\{nameSpace}*.dll");
                Console.ResetColor();
            })!
            .ConfigureServices(serviceCollection =>
                //// Make DefaultLogger available for logging
                serviceCollection.AddTransient<DefaultLogger>());

        if (isService)
        {
            _builder.UseServiceBaseLifetime();
        }
        else
        {
            _builder.UseConsoleLifetime();
        }

        var host = _builder.Build();
        _logger = host.Services.GetRequiredService<DefaultLogger>();
        configureHost?.Invoke(host);

        return host.RunAsync(new CancellationToken(false));
    }

    private static IHostBuilder? ConfigureExternal(this IHostBuilder? hostBuilder, Func<IHostBuilder?, IHostBuilder?>? hostBuilderFunc)
    {
        if (hostBuilderFunc == null)
        {
            return hostBuilder;
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Configure External Start");
        Console.ResetColor();
        var builder = hostBuilderFunc(hostBuilder);
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Configure External Complete");
        Console.ResetColor();
        return builder;
    }

    private static IHostBuilder? ConfigureLogging(this IHostBuilder? hostBuilder) =>
        hostBuilder?.ConfigureLogging((hostContext, configLogging) =>
            configLogging
                    .AddConfiguration(hostContext.Configuration.GetSection("Logging"))
                    .AddConsole()
                    .AddEventLog()
                    .AddLog4Net("log4net.config")
                    .AddDebug());

    private static IHostBuilder? ConfigureConfiguration(this IHostBuilder? hostBuilder, string[] args) =>
        hostBuilder?
            .ConfigureHostConfiguration(configHost =>
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Configure Host Start");
                configHost.SetBasePath(Directory.GetCurrentDirectory());
                configHost.AddJsonFile(HostSettingsFile, optional: true);
                configHost.AddEnvironmentVariables(prefix: Prefix);
                configHost.AddCommandLine(args);
                Console.WriteLine("Configure Host Complete");
                Console.ResetColor();
            })
            .ConfigureAppConfiguration((hostContext, configApp) =>
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Configure App Start");
                configApp.AddJsonFile(AppSettingsFilePrefix + ".json", optional: true);
                if (!string.IsNullOrEmpty(hostContext.HostingEnvironment.EnvironmentName))
                {
                    configApp.AddJsonFile(AppSettingsFilePrefix + $".{hostContext.HostingEnvironment.EnvironmentName}.json", optional: true);
                }

                configApp.AddEnvironmentVariables(prefix: Prefix);
                configApp.AddCommandLine(args);
                Console.WriteLine("Configure App Complete");
                Console.ResetColor();
            });
}
