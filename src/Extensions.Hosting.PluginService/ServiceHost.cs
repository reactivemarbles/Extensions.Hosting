// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Threading;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

#if REACTIVE_SHIM
namespace ReactiveMarbles.Extensions.Hosting.Reactive.PluginService;
#else
namespace ReactiveMarbles.Extensions.Hosting.PluginService;
#endif

/// <summary>Provides static methods for configuring and running a host or application host with plugin and logging support.</summary>
public static class ServiceHost
{
    /// <summary>Stores the runtime used to interact with the host and service control manager.</summary>
    private static IServiceHostRuntime _runtime = new ServiceHostRuntime();

    /// <summary>Stores the logger value.</summary>
    private static DefaultLogger? _logger;

    /// <summary>Gets the current logger instance used for application logging.</summary>
    public static ILogger? Logger => _logger?.Logger;

    /// <summary>Gets the runtime used to interact with the host and service control manager.</summary>
    internal static IServiceHostRuntime Runtime => Volatile.Read(ref _runtime);

    /// <summary>Creates and runs a host for the specified plugin type using the default host configuration.</summary>
    /// <param name="type">The type representing the plugin to host.</param>
    /// <param name="args">The command-line arguments.</param>
    /// <returns>A task that represents the host lifetime.</returns>
    public static Task Create(Type type, string[] args) =>
        new ServiceHostRunner(Runtime).Create(type, args, null, null, "ReactiveMarbles.Plugin", null);

    /// <summary>Creates and runs a host for the specified plugin type with the supplied configuration.</summary>
    /// <param name="type">The type representing the plugin to host.</param>
    /// <param name="args">The command-line arguments.</param>
    /// <param name="hostBuilder">An optional host-builder configuration callback.</param>
    /// <param name="configureHost">An optional built-host configuration callback.</param>
    /// <param name="nameSpace">The namespace pattern used to locate plugin assemblies.</param>
    /// <param name="targetRuntime">The target runtime identifier.</param>
    /// <returns>A task that represents the host lifetime.</returns>
    public static Task Create(Type type, string[] args, Func<IHostBuilder?, IHostBuilder?>? hostBuilder, Action<IHost>? configureHost, string nameSpace, string? targetRuntime) =>
        new ServiceHostRunner(Runtime).Create(type, args, hostBuilder, configureHost, nameSpace, targetRuntime);

    /// <summary>Creates and runs an application host using the default host configuration.</summary>
    /// <param name="type">The application entry type.</param>
    /// <param name="args">The command-line arguments.</param>
    /// <returns>A task that represents the application lifetime.</returns>
    public static Task CreateApplication(Type type, string[] args) =>
        new ServiceHostRunner(Runtime).CreateApplication(type, args, null, null, "ReactiveMarbles.Plugin", null);

    /// <summary>Creates and runs an application host with the supplied configuration.</summary>
    /// <param name="type">The application entry type.</param>
    /// <param name="args">The command-line arguments.</param>
    /// <param name="hostBuilder">An optional application-host-builder configuration callback.</param>
    /// <param name="configureHost">An optional built-host configuration callback.</param>
    /// <param name="nameSpace">The namespace pattern used to locate plugin assemblies.</param>
    /// <param name="targetRuntime">The target runtime identifier.</param>
    /// <returns>A task that represents the host lifetime.</returns>
    public static Task CreateApplication(
        Type type,
        string[] args,
        Func<IHostApplicationBuilder?, IHostApplicationBuilder?>? hostBuilder,
        Action<IHost>? configureHost,
        string nameSpace,
        string? targetRuntime) =>
        new ServiceHostRunner(Runtime).CreateApplication(type, args, hostBuilder, configureHost, nameSpace, targetRuntime);

    /// <summary>Runs a built host through the configured runtime.</summary>
    /// <param name="host">The host to run.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the host stops.</returns>
    internal static Task RunAsync(IHost host, CancellationToken cancellationToken) => Runtime.RunAsync(host, cancellationToken);

    /// <summary>Sets the current default logger.</summary>
    /// <param name="logger">The logger to retain.</param>
    internal static void SetLogger(DefaultLogger logger) => _logger = logger;

    /// <summary>Temporarily replaces the operating-system runtime.</summary>
    /// <param name="runtime">The runtime to use while the returned scope is active.</param>
    /// <returns>A scope that restores the previous runtime when disposed.</returns>
    internal static IDisposable UseRuntime(IServiceHostRuntime runtime) =>
        new RuntimeScope(Interlocked.Exchange(ref _runtime, runtime ?? throw new ArgumentNullException(nameof(runtime))));

    /// <summary>Restores the runtime that was active before a test scope.</summary>
    /// <param name="previousRuntime">The runtime to restore.</param>
    private sealed class RuntimeScope(IServiceHostRuntime previousRuntime) : IDisposable
    {
        /// <inheritdoc/>
        public void Dispose() => Volatile.Write(ref _runtime, previousRuntime);
    }
}
