// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
    /// Runs as service asynchronous.
    /// </summary>
    /// <param name="hostBuilder">The host builder.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Task.</returns>
    public static Task RunAsServiceAsync(this IHostBuilder hostBuilder, CancellationToken cancellationToken = default) =>
        hostBuilder.UseServiceBaseLifetime()!.Build().RunAsync(cancellationToken);
}
