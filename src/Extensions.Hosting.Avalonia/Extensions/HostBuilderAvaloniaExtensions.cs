// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using Avalonia.Controls;
using Microsoft.Extensions.Hosting;

namespace ReactiveMarbles.Extensions.Hosting.Avalonia;

/// <summary>
/// Provides extension methods for configuring Avalonia integration with a host builder, enabling application lifetime
/// management and customization of Avalonia settings.
/// </summary>
/// <remarks>These methods allow developers to integrate Avalonia's application lifecycle and settings into a
/// generic host, ensuring proper shutdown behavior and enabling customization before application startup.</remarks>
public static class HostBuilderAvaloniaExtensions
{
    /// <summary>
    /// Configures the host builder to use Avalonia application lifetime management, enabling proper shutdown behavior
    /// based on the specified shutdown mode.
    /// </summary>
    /// <remarks>This method integrates Avalonia's lifetime management with the host builder, ensuring that
    /// the application shuts down according to the selected shutdown mode. Use this extension when hosting Avalonia
    /// applications to control shutdown behavior.</remarks>
    /// <param name="hostBuilder">The host builder instance used to configure application services and lifetime management.</param>
    /// <param name="shutdownMode">Specifies the shutdown mode for the Avalonia application. The default is <see
    /// cref="ShutdownMode.OnLastWindowClose"/>, which shuts down the application when the last window is closed.</param>
    /// <returns>The updated <see cref="IHostBuilder"/> instance, allowing for further configuration.</returns>
    public static IHostBuilder UseAvaloniaLifetime(this IHostBuilder hostBuilder, ShutdownMode shutdownMode = ShutdownMode.OnLastWindowClose)
    {
        if (hostBuilder == null)
        {
            throw new ArgumentNullException(nameof(hostBuilder));
        }

        return InternalBuilderAvaloniaUtility.UseAvaloniaLifetime(hostBuilder, shutdownMode);
    }

    /// <summary>
    /// Configures the Avalonia framework for the specified host builder, enabling integration and customization of
    /// Avalonia application settings before startup.
    /// </summary>
    /// <remarks>Use this method to set up Avalonia-specific configurations and options prior to application
    /// startup. This enables customization of the Avalonia environment within a generic host.</remarks>
    /// <param name="hostBuilder">The host builder to configure for Avalonia integration. Cannot be null.</param>
    /// <param name="configureDelegate">An optional delegate that allows further customization of the Avalonia builder. If provided, it is invoked to
    /// configure Avalonia-specific options.</param>
    /// <returns>An instance of IHostBuilder that has been configured for Avalonia support.</returns>
    public static IHostBuilder ConfigureAvalonia(this IHostBuilder hostBuilder, Action<IAvaloniaBuilder>? configureDelegate = null)
    {
        if (hostBuilder == null)
        {
            throw new ArgumentNullException(nameof(hostBuilder));
        }

        return InternalBuilderAvaloniaUtility.ConfigureAvalonia(hostBuilder, configureDelegate);
    }
}
