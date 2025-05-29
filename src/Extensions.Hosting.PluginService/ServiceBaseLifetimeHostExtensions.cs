// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;

namespace ReactiveMarbles.Extensions.Hosting.PluginService;

/// <summary>
/// ServiceBaseLifetimeHostExtensions.
/// </summary>
public static class ServiceBaseLifetimeHostExtensions
{
    /// <summary>
    /// Uses the service base lifetime.
    /// </summary>
    /// <param name="hostBuilder">The host builder.</param>
    /// <returns>IHostBilder.</returns>
    public static IHostBuilder? UseServiceBaseLifetime(this IHostBuilder hostBuilder) =>
        hostBuilder?.ConfigureServices(services => services.AddSingleton<IHostLifetime, ServiceBaseLifetime>());

    /// <summary>
    /// Uses the service base lifetime.
    /// </summary>
    /// <param name="hostBuilder">The host builder.</param>
    /// <returns>IHostBilder.</returns>
    public static HostApplicationBuilder? UseServiceBaseLifetime(this IHostApplicationBuilder hostBuilder)
    {
        hostBuilder?.Services.AddSingleton<IHostLifetime, ServiceBaseLifetime>();
        return hostBuilder as HostApplicationBuilder;
    }

    /// <summary>
    /// Listens for Ctrl+C or SIGTERM and calls <see cref="IHostApplicationLifetime.StopApplication"/> to start the shutdown process.
    /// This will unblock extensions like RunAsync and WaitForShutdownAsync.
    /// </summary>
    /// <param name="hostBuilder">The <see cref="IHostBuilder" /> to configure.</param>
    /// <returns>The same instance of the <see cref="IHostBuilder"/> for chaining.</returns>
    public static IHostApplicationBuilder UseConsoleLifetime(this IHostApplicationBuilder hostBuilder)
    {
        if (hostBuilder is null)
        {
            throw new ArgumentNullException(nameof(hostBuilder));
        }

        hostBuilder.Services.AddSingleton<IHostLifetime, ConsoleLifetime>();
        return hostBuilder;
    }

    /// <summary>
    /// Runs as service asynchronous.
    /// </summary>
    /// <param name="hostBuilder">The host builder.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Task.</returns>
    public static Task RunAsServiceAsync(this IHostBuilder hostBuilder, CancellationToken cancellationToken = default) =>
        hostBuilder.UseServiceBaseLifetime()!.Build().RunAsync(cancellationToken);

    /// <summary>
    /// Runs as service asynchronous.
    /// </summary>
    /// <param name="hostBuilder">The host builder.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Task.</returns>
    public static Task RunAsServiceAsync(this HostApplicationBuilder hostBuilder, CancellationToken cancellationToken = default) =>
        hostBuilder.UseServiceBaseLifetime()!.Build().RunAsync(cancellationToken);
}
