// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
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

        var builder = Host.CreateDefaultBuilder()
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
            builder.UseServiceBaseLifetime();
        }
        else
        {
            builder.UseConsoleLifetime();
        }

        var host = builder.Build();
        _logger = host.Services.GetRequiredService<DefaultLogger>();
        configureHost?.Invoke(host);

        return host.RunAsync(new CancellationToken(false));
    }

    /// <summary>
    /// Creates the specified host Application builder.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="args">The arguments.</param>
    /// <param name="hostBuilder">The host Application builder.</param>
    /// <param name="configureHost">The configure host.</param>
    /// <param name="nameSpace">The plugin name space.</param>
    /// <param name="targetRuntime">The target runtime folder for plugins.</param>
    /// <returns>
    /// A Task.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">type.</exception>
    public static Task CreateApplication(
        Type type,
        string[] args,
        Func<IHostApplicationBuilder?, IHostApplicationBuilder?>? hostBuilder = default,
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

        var builder = Host.CreateApplicationBuilder();
        builder
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
            });
        //// Make DefaultLogger available for logging
        builder.Services.AddTransient<DefaultLogger>();

        if (isService)
        {
            builder.UseServiceBaseLifetime();
        }
        else
        {
            builder.UseConsoleLifetime();
        }

        var host = builder.Build();
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

    /// <summary>
    /// Specifies the content root directory to be used by the host. To avoid the content root directory being
    /// overwritten by a default value, ensure this is called after defaults are configured.
    /// </summary>
    /// <param name="hostBuilder">The <see cref="IHostBuilder"/> to configure.</param>
    /// <param name="contentRoot">Path to root directory of the application.</param>
    /// <returns>The <see cref="IHostBuilder"/>.</returns>
    private static IHostApplicationBuilder UseContentRoot(this IHostApplicationBuilder hostBuilder, string contentRoot)
    {
        if (string.IsNullOrEmpty(contentRoot))
        {
            throw new ArgumentNullException(nameof(contentRoot), "Content root cannot be null or empty.");
        }

        hostBuilder.Configuration.AddInMemoryCollection(new[]
        {
                    new KeyValuePair<string, string?>(HostDefaults.ContentRootKey, contentRoot)
        });

        return hostBuilder;
    }

    private static IHostApplicationBuilder? ConfigureExternal(this IHostApplicationBuilder? hostBuilder, Func<IHostApplicationBuilder?, IHostApplicationBuilder?>? hostBuilderFunc)
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

    private static IHostApplicationBuilder? ConfigureLogging(this IHostApplicationBuilder? hostBuilder)
    {
        hostBuilder?.Logging
                    .AddConfiguration(hostBuilder.Configuration.GetSection("Logging"))
                    .AddConsole()
                    .AddEventLog()
                    .AddLog4Net("log4net.config")
                    .AddDebug();

        return hostBuilder;
    }

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

    private static IHostApplicationBuilder? ConfigureConfiguration(this IHostApplicationBuilder? hostBuilder, string[] args)
    {
        if (hostBuilder == null)
        {
            return null;
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Configure Host Start");
        hostBuilder.Configuration.SetBasePath(Directory.GetCurrentDirectory());
        hostBuilder.Configuration.AddJsonFile(HostSettingsFile, optional: true);
        hostBuilder.Configuration.AddEnvironmentVariables(prefix: Prefix);
        hostBuilder.Configuration.AddCommandLine(args);
        Console.WriteLine("Configure Host Complete");
        Console.ResetColor();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Configure App Start");
        hostBuilder.Configuration.AddJsonFile(AppSettingsFilePrefix + ".json", optional: true);
        if (!string.IsNullOrEmpty(hostBuilder.Environment.EnvironmentName))
        {
            hostBuilder.Configuration.AddJsonFile(AppSettingsFilePrefix + $".{hostBuilder.Environment.EnvironmentName}.json", optional: true);
        }

        hostBuilder.Configuration.AddEnvironmentVariables(prefix: Prefix);
        hostBuilder.Configuration.AddCommandLine(args);
        Console.WriteLine("Configure App Complete");
        Console.ResetColor();
        return hostBuilder;
    }
}
