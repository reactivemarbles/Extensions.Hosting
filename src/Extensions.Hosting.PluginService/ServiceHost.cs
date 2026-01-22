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
/// Provides static methods for configuring and running a host or application host with plugin and logging support.
/// </summary>
/// <remarks>The ServiceHost class simplifies the setup of .NET hosts, including configuration, logging, and
/// plugin loading. It is intended for use in applications that require dynamic plugin discovery and flexible host
/// configuration. All members are thread-safe for typical usage scenarios.</remarks>
public static class ServiceHost
{
    private const string AppSettingsFilePrefix = "appsettings";
    private const string HostSettingsFile = "hostsettings.json";
    private const string Prefix = "PREFIX_";
    private static DefaultLogger? _logger;

    /// <summary>
    /// Gets the current logger instance used for application logging.
    /// </summary>
    public static ILogger? Logger => _logger?.Logger;

    /// <summary>
    /// Creates and runs a host for the specified plugin type, configuring logging, services, and plugin discovery
    /// according to the provided parameters.
    /// </summary>
    /// <remarks>This method sets up a .NET host with default configuration, logging, and plugin scanning
    /// based on the specified parameters. It supports both service and console lifetimes, automatically selecting the
    /// appropriate mode based on the execution context and command-line arguments. Plugins are discovered using the
    /// provided namespace and runtime information. Additional host or host builder configuration can be supplied via
    /// the optional delegates.</remarks>
    /// <param name="type">The type representing the plugin to host. Cannot be null.</param>
    /// <param name="args">The command-line arguments to use for host configuration and plugin startup.</param>
    /// <param name="hostBuilder">An optional delegate to further configure the <see cref="IHostBuilder"/> before building the host. If null, no
    /// additional configuration is applied.</param>
    /// <param name="configureHost">An optional action to perform additional configuration on the built <see cref="IHost"/> before it is run. If
    /// null, no additional configuration is performed.</param>
    /// <param name="nameSpace">The namespace pattern used to locate plugin assemblies. Defaults to "ReactiveMarbles.Plugin".</param>
    /// <param name="targetRuntime">The target runtime identifier used to determine the plugin directory. If null, the runtime is inferred from the
    /// plugin assembly location.</param>
    /// <returns>A task that represents the asynchronous operation of running the configured host.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="type"/> is null.</exception>
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
    /// Initializes and runs a plugin-based .NET application using the specified entry type, command-line arguments, and
    /// optional host configuration.
    /// </summary>
    /// <remarks>This method configures the application host, logging, plugin discovery, and lifetime
    /// management based on the provided parameters. It supports both console and Windows service modes, automatically
    /// selecting the appropriate lifetime based on the execution context and command-line arguments. Plugins are loaded
    /// from directories determined by the specified or inferred runtime and namespace. Use the optional delegates to
    /// customize host building and post-build configuration as needed.</remarks>
    /// <param name="type">The entry point type for the application. This type's assembly is used to determine the application location and
    /// plugin scanning context. Cannot be null.</param>
    /// <param name="args">The command-line arguments to pass to the application. Used for configuration and to determine the application
    /// mode (console or service).</param>
    /// <param name="hostBuilder">An optional delegate to further configure the <see cref="IHostApplicationBuilder"/> before building the host. If
    /// null, no additional configuration is applied.</param>
    /// <param name="configureHost">An optional action to configure the <see cref="IHost"/> after it is built but before it is run. If null, no
    /// additional configuration is performed.</param>
    /// <param name="nameSpace">The namespace pattern used to locate and include plugin assemblies. Defaults to "ReactiveMarbles.Plugin".</param>
    /// <param name="targetRuntime">The target runtime identifier used to determine the plugin directory. If null, the runtime is inferred from the
    /// entry assembly location.</param>
    /// <returns>A task that represents the asynchronous operation of running the application. The task completes when the
    /// application shuts down.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="type"/> is null.</exception>
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

    /// <summary>
    /// Invokes the specified delegate to configure the provided host builder, if the delegate is not null.
    /// </summary>
    /// <remarks>This method writes informational messages to the console before and after invoking the
    /// configuration delegate.</remarks>
    /// <param name="hostBuilder">The host builder instance to configure. Can be null.</param>
    /// <param name="hostBuilderFunc">A delegate that receives the current host builder and returns a configured host builder. If null, no
    /// configuration is performed.</param>
    /// <returns>The configured host builder returned by the delegate, or the original host builder if the delegate is null.</returns>
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
    /// Sets the content root directory for the application host builder.
    /// </summary>
    /// <remarks>The content root determines the base path for content files, such as configuration files and
    /// static assets, used by the application.</remarks>
    /// <param name="hostBuilder">The application host builder to configure.</param>
    /// <param name="contentRoot">The absolute path to the directory that should be used as the content root. Cannot be null or empty.</param>
    /// <returns>The same instance of <see cref="IHostApplicationBuilder"/> for chaining further configuration.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="contentRoot"/> is null or empty.</exception>
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

    /// <summary>
    /// Invokes the specified delegate to configure the provided host application builder, if the delegate is not null.
    /// </summary>
    /// <remarks>This method writes informational messages to the console before and after invoking the
    /// configuration delegate. Use this method to apply external configuration logic to the host builder in a
    /// composable manner.</remarks>
    /// <param name="hostBuilder">The current <see cref="IHostApplicationBuilder"/> instance to be configured. Can be null.</param>
    /// <param name="hostBuilderFunc">A delegate that receives the current <see cref="IHostApplicationBuilder"/> and returns a configured instance. If
    /// null, no configuration is performed.</param>
    /// <returns>The configured <see cref="IHostApplicationBuilder"/> instance returned by <paramref name="hostBuilderFunc"/>, or
    /// the original <paramref name="hostBuilder"/> if <paramref name="hostBuilderFunc"/> is null.</returns>
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

    /// <summary>
    /// Configures logging providers for the specified host builder, including console, event log, Log4Net, and debug
    /// logging, using settings from the "Logging" configuration section.
    /// </summary>
    /// <remarks>This method adds multiple logging providers to the host, including console, event log,
    /// Log4Net (using the "log4net.config" file), and debug output. Logging settings are read from the "Logging"
    /// section of the application's configuration. Call this method during host setup to enable these logging
    /// providers.</remarks>
    /// <param name="hostBuilder">The host builder to configure logging for. Can be null.</param>
    /// <returns>The same <see cref="IHostBuilder"/> instance with logging configured, or null if <paramref name="hostBuilder"/>
    /// is null.</returns>
    private static IHostBuilder? ConfigureLogging(this IHostBuilder? hostBuilder) =>
        hostBuilder?.ConfigureLogging((hostContext, configLogging) =>
            configLogging
                    .AddConfiguration(hostContext.Configuration.GetSection("Logging"))
                    .AddConsole()
                    .AddEventLog()
                    .AddLog4Net("log4net.config")
                    .AddDebug());

    /// <summary>
    /// Configures logging providers and settings for the specified host application builder.
    /// </summary>
    /// <remarks>This method adds configuration from the "Logging" section, and registers console, event log,
    /// Log4Net (using "log4net.config"), and debug logging providers. Call this method during application startup to
    /// enable these logging mechanisms.</remarks>
    /// <param name="hostBuilder">The host application builder to configure. Can be null.</param>
    /// <returns>The same <see cref="IHostApplicationBuilder"/> instance with logging configured, or null if <paramref
    /// name="hostBuilder"/> is null.</returns>
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

    /// <summary>
    /// Configures the host and application configuration for the specified host builder using JSON files, environment
    /// variables, and command-line arguments.
    /// </summary>
    /// <remarks>This method adds configuration sources for both the host and application, including JSON
    /// files, environment variables with a specific prefix, and command-line arguments. It is intended to be used as an
    /// extension method during host setup to ensure consistent configuration across environments.</remarks>
    /// <param name="hostBuilder">The host builder to configure. Can be null.</param>
    /// <param name="args">The command-line arguments to include in the configuration. Cannot be null.</param>
    /// <returns>The configured host builder instance, or null if the input host builder is null.</returns>
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

    /// <summary>
    /// Configures the application's configuration sources for the specified host builder using JSON files, environment
    /// variables, and command-line arguments.
    /// </summary>
    /// <remarks>This method adds configuration sources in a layered order, including base and
    /// environment-specific JSON files, environment variables with a specific prefix, and command-line arguments. It is
    /// intended to be called during application startup to ensure consistent configuration loading.</remarks>
    /// <param name="hostBuilder">The host application builder to configure. If null, no configuration is applied and null is returned.</param>
    /// <param name="args">The command-line arguments to include as a configuration source.</param>
    /// <returns>The configured host application builder, or null if the provided host builder is null.</returns>
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
