// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
#if REACTIVE_SHIM
using ReactiveMarbles.Extensions.Hosting.Reactive.Plugins;
#else
using ReactiveMarbles.Extensions.Hosting.Plugins;
#endif
using ReactiveMarbles.Extensions.Logging;

#if REACTIVE_SHIM
namespace ReactiveMarbles.Extensions.Hosting.Reactive.PluginService;
#else
namespace ReactiveMarbles.Extensions.Hosting.PluginService;
#endif

/// <summary>Provides static methods for configuring and running a host or application host with plugin and logging support.</summary>
/// <remarks>The ServiceHost class simplifies the setup of .NET hosts, including configuration, logging, and
/// plugin loading. It is intended for use in applications that require dynamic plugin discovery and flexible host
/// configuration. All members are thread-safe for typical usage scenarios.</remarks>
public static class ServiceHost
{
    /// <summary>Stores the app settings file prefix value.</summary>
    private const string AppSettingsFilePrefix = "appsettings";

    /// <summary>Stores the host settings file value.</summary>
    private const string HostSettingsFile = "hostsettings.json";

    /// <summary>Stores the prefix value.</summary>
    private const string Prefix = "PREFIX_";

    /// <summary>Stores the logger value.</summary>
    private static DefaultLogger? _logger;

    /// <summary>Gets the current logger instance used for application logging.</summary>
    public static ILogger? Logger => _logger?.Logger;

    /// <summary>Creates and runs a host for the specified plugin type, configuring logging, services, and plugin discovery according to the provided parameters.</summary>
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
        if (type is null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        var executableLocation = Path.GetDirectoryName(type.Assembly.Location);
        var isService = IsServiceMode(args);
        if (isService)
        {
            SetCurrentDirectoryToProcessRoot();
        }

        var builder = Host.CreateDefaultBuilder()
            .UseContentRoot(Directory.GetCurrentDirectory());

        builder = ConfigureLogging(builder)!;
        builder = ConfigureExternal(builder, hostBuilder)!;
        builder = ConfigureConfiguration(builder, args)!;
        builder = builder
            .ConfigurePlugins(pluginBuilder => ConfigurePluginScanning(pluginBuilder, executableLocation, targetRuntime, nameSpace))!
            .ConfigureServices(serviceCollection => _ = serviceCollection.AddTransient<DefaultLogger>());

        ConfigureLifetime(builder, isService);

        var host = builder.Build();
        _logger = host.Services.GetRequiredService<DefaultLogger>();
        configureHost?.Invoke(host);

        return host.RunAsync(CancellationToken.None);
    }

    /// <summary>Initializes and runs a plugin-based .NET application using the specified entry type, command-line arguments, and optional host configuration.</summary>
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
        if (type is null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        var executableLocation = Path.GetDirectoryName(type.Assembly.Location);
        var isService = IsServiceMode(args);
        if (isService)
        {
            SetCurrentDirectoryToProcessRoot();
        }

        var builder = Host.CreateApplicationBuilder();
        _ = UseContentRoot(builder, Directory.GetCurrentDirectory());
        _ = ConfigureLogging(builder);
        _ = ConfigureExternal(builder, hostBuilder);
        _ = ConfigureConfiguration(builder, args);
        _ = builder.ConfigurePlugins(pluginBuilder => ConfigurePluginScanning(pluginBuilder, executableLocation, targetRuntime, nameSpace));
        _ = builder.Services.AddTransient<DefaultLogger>();

        ConfigureLifetime(builder, isService);

        var host = builder.Build();
        _logger = host.Services.GetRequiredService<DefaultLogger>();
        configureHost?.Invoke(host);

        return host.RunAsync(CancellationToken.None);
    }

    /// <summary>Determines whether the current process should run as a service.</summary>
    /// <param name="args">The command-line arguments.</param>
    /// <returns>true when the host should run as a service; otherwise, false.</returns>
    private static bool IsServiceMode(string[] args) =>
        !Debugger.IsAttached && (args.Length == 0 || args[0].IndexOf("--console", StringComparison.InvariantCultureIgnoreCase) < 0);

    /// <summary>Sets the current directory to the executable directory.</summary>
    private static void SetCurrentDirectoryToProcessRoot()
    {
        var pathToExe = Process.GetCurrentProcess().MainModule?.FileName;
        var pathToContentRoot = Path.GetDirectoryName(pathToExe);
        Directory.SetCurrentDirectory(pathToContentRoot!);
    }

    /// <summary>Configures the host lifetime for service or console execution.</summary>
    /// <param name="hostBuilder">The host builder to configure.</param>
    /// <param name="isService">true to use the service lifetime; otherwise, false.</param>
    private static void ConfigureLifetime(IHostBuilder hostBuilder, bool isService)
    {
        if (isService)
        {
            _ = hostBuilder.UseServiceBaseLifetime();
            return;
        }

        _ = hostBuilder.UseConsoleLifetime();
    }

    /// <summary>Configures the host application builder lifetime for service or console execution.</summary>
    /// <param name="hostBuilder">The host application builder to configure.</param>
    /// <param name="isService">true to use the service lifetime; otherwise, false.</param>
    private static void ConfigureLifetime(IHostApplicationBuilder hostBuilder, bool isService)
    {
        if (isService)
        {
            _ = hostBuilder.UseServiceBaseLifetime();
            return;
        }

        _ = hostBuilder.UseConsoleLifetime();
    }

    /// <summary>Configures plugin scanning using process and runtime information.</summary>
    /// <param name="pluginBuilder">The plugin builder to configure.</param>
    /// <param name="executableLocation">The executable location used to infer the runtime.</param>
    /// <param name="targetRuntime">The target runtime override.</param>
    /// <param name="nameSpace">The namespace prefix used to discover plugins.</param>
    private static void ConfigurePluginScanning(IPluginBuilder? pluginBuilder, string? executableLocation, string? targetRuntime, string nameSpace)
    {
        var process = Process.GetCurrentProcess();
        var fullPath = process.MainModule?.FileName?.Replace(process.MainModule.ModuleName!, string.Empty);
        pluginBuilder?.AddScanDirectories(fullPath!);

        pluginBuilder?.IncludeFrameworks(@"\netstandard2.0\*.FrameworkLib.dll");

        var runtime = targetRuntime ?? Path.GetFileName(executableLocation);
        pluginBuilder?.IncludePlugins(@$"\Plugins\{runtime}\{nameSpace}*.dll");
    }

    /// <summary>Sets the content root directory for the application host builder.</summary>
    /// <param name="hostBuilder">The host application builder to configure.</param>
    /// <param name="contentRoot">The absolute path to the directory that should be used as the content root. Cannot be null or empty.</param>
    /// <returns>The same instance of <see cref="IHostApplicationBuilder"/> for chaining further configuration.</returns>
    /// <exception cref="ArgumentNullException">Thrown if contentRoot is null or empty.</exception>
    private static IHostApplicationBuilder UseContentRoot(IHostApplicationBuilder hostBuilder, string contentRoot)
    {
        var checkedContentRoot = !string.IsNullOrEmpty(contentRoot)
            ? contentRoot
            : throw new ArgumentNullException(nameof(contentRoot), "Content root cannot be null or empty.");

        _ = hostBuilder.Configuration.AddInMemoryCollection([new(HostDefaults.ContentRootKey, checkedContentRoot)]);

        return hostBuilder;
    }

    /// <summary>Invokes an external host application builder configuration delegate when one was provided.</summary>
    /// <param name="hostBuilder">The current host application builder.</param>
    /// <param name="hostBuilderFunc">The optional external configuration delegate.</param>
    /// <returns>The configured host application builder.</returns>
    private static IHostApplicationBuilder? ConfigureExternal(IHostApplicationBuilder? hostBuilder, Func<IHostApplicationBuilder?, IHostApplicationBuilder?>? hostBuilderFunc) =>
        hostBuilderFunc is null ? hostBuilder : hostBuilderFunc(hostBuilder);

    /// <summary>Configures logging providers and settings for the specified host application builder.</summary>
    /// <param name="hostBuilder">The host application builder to configure.</param>
    /// <returns>The configured host application builder.</returns>
    private static IHostApplicationBuilder? ConfigureLogging(IHostApplicationBuilder? hostBuilder)
    {
        if (hostBuilder is null)
        {
            return null;
        }

        _ = hostBuilder.Logging
            .AddConfiguration(hostBuilder.Configuration.GetSection("Logging"))
            .AddConsole()
            .AddEventLog()
            .AddLog4Net("log4net.config")
            .AddDebug();

        return hostBuilder;
    }

    /// <summary>Configures application configuration sources for the host application builder.</summary>
    /// <param name="hostBuilder">The host application builder to configure.</param>
    /// <param name="args">The command-line arguments to include as a configuration source.</param>
    /// <returns>The configured host application builder.</returns>
    private static IHostApplicationBuilder? ConfigureConfiguration(IHostApplicationBuilder? hostBuilder, string[] args)
    {
        if (hostBuilder is null)
        {
            return null;
        }

        _ = hostBuilder.Configuration.SetBasePath(Directory.GetCurrentDirectory());
        _ = hostBuilder.Configuration.AddJsonFile(HostSettingsFile, optional: true);
        _ = hostBuilder.Configuration.AddEnvironmentVariables(prefix: Prefix);
        _ = hostBuilder.Configuration.AddCommandLine(args);

        _ = hostBuilder.Configuration.AddJsonFile(AppSettingsFilePrefix + ".json", optional: true);
        if (!string.IsNullOrEmpty(hostBuilder.Environment.EnvironmentName))
        {
            _ = hostBuilder.Configuration.AddJsonFile(AppSettingsFilePrefix + $".{hostBuilder.Environment.EnvironmentName}.json", optional: true);
        }

        _ = hostBuilder.Configuration.AddEnvironmentVariables(prefix: Prefix);
        _ = hostBuilder.Configuration.AddCommandLine(args);
        return hostBuilder;
    }

    /// <summary>Invokes an external host builder configuration delegate when one was provided.</summary>
    /// <param name="hostBuilder">The current host builder.</param>
    /// <param name="hostBuilderFunc">The optional external configuration delegate.</param>
    /// <returns>The configured host builder.</returns>
    private static IHostBuilder? ConfigureExternal(IHostBuilder? hostBuilder, Func<IHostBuilder?, IHostBuilder?>? hostBuilderFunc) =>
        hostBuilderFunc is null ? hostBuilder : hostBuilderFunc(hostBuilder);

    /// <summary>Configures logging providers for the specified host builder.</summary>
    /// <param name="hostBuilder">The host builder to configure.</param>
    /// <returns>The configured host builder.</returns>
    private static IHostBuilder? ConfigureLogging(IHostBuilder? hostBuilder) =>
        hostBuilder?.ConfigureLogging((hostContext, configLogging) =>
        {
            _ = configLogging
                .AddConfiguration(hostContext.Configuration.GetSection("Logging"))
                .AddConsole()
                .AddEventLog()
                .AddLog4Net("log4net.config")
                .AddDebug();
        });

    /// <summary>Configures host and application configuration sources for the host builder.</summary>
    /// <param name="hostBuilder">The host builder to configure.</param>
    /// <param name="args">The command-line arguments to include in the configuration.</param>
    /// <returns>The configured host builder.</returns>
    private static IHostBuilder? ConfigureConfiguration(IHostBuilder? hostBuilder, string[] args) =>
        hostBuilder?
            .ConfigureHostConfiguration(configHost =>
            {
                _ = configHost.SetBasePath(Directory.GetCurrentDirectory());
                _ = configHost.AddJsonFile(HostSettingsFile, optional: true);
                _ = configHost.AddEnvironmentVariables(prefix: Prefix);
                _ = configHost.AddCommandLine(args);
            })
            .ConfigureAppConfiguration((hostContext, configApp) =>
            {
                _ = configApp.AddJsonFile(AppSettingsFilePrefix + ".json", optional: true);
                if (!string.IsNullOrEmpty(hostContext.HostingEnvironment.EnvironmentName))
                {
                    _ = configApp.AddJsonFile(AppSettingsFilePrefix + $".{hostContext.HostingEnvironment.EnvironmentName}.json", optional: true);
                }

                _ = configApp.AddEnvironmentVariables(prefix: Prefix);
                _ = configApp.AddCommandLine(args);
            });
}
