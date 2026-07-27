// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using Avalonia.Controls;
using Microsoft.Extensions.Hosting;

namespace ReactiveMarbles.Extensions.Hosting.Avalonia;

/// <summary>Provides extension methods for configuring Avalonia integration with a host builder, enabling application lifetime management and customization of Avalonia settings.</summary>
/// <remarks>These methods allow developers to integrate Avalonia's application lifecycle and settings into a
/// generic host, ensuring proper shutdown behavior and enabling customization before application startup.</remarks>
public static class HostBuilderAvaloniaExtensions
{
    /// <summary>Provides extension members for this receiver.</summary>
    /// <param name="hostBuilder">The receiver instance.</param>
    extension(IHostBuilder hostBuilder)
    {
        /// <summary>Configures the host builder to use Avalonia application lifetime management, enabling proper shutdown behavior based on the specified shutdown mode.</summary>
        /// <remarks>This method integrates Avalonia's lifetime management with the host builder, ensuring that
        /// the application shuts down according to the selected shutdown mode. Use this extension when hosting Avalonia
        /// applications to control shutdown behavior.</remarks>
        /// <returns>The updated <see cref="IHostBuilder"/> instance, allowing for further configuration.</returns>
        public IHostBuilder UseAvaloniaLifetime() =>
            hostBuilder.UseAvaloniaLifetime(ShutdownMode.OnLastWindowClose);

        /// <summary>Configures the host builder to use Avalonia lifetime management with a shutdown mode.</summary>
        /// <param name="shutdownMode">Specifies the shutdown mode for the Avalonia application.</param>
        /// <returns>The updated <see cref="IHostBuilder"/> instance, allowing for further configuration.</returns>
        public IHostBuilder UseAvaloniaLifetime(ShutdownMode shutdownMode)
        {
            _ = hostBuilder ?? throw new ArgumentNullException(nameof(hostBuilder));

            return InternalBuilderAvaloniaUtility.UseAvaloniaLifetime(hostBuilder, shutdownMode);
        }

        /// <summary>Configures the Avalonia framework for the specified host builder, enabling integration and customization of Avalonia application settings before startup.</summary>
        /// <remarks>Use this method to set up Avalonia-specific configurations and options prior to application
        /// startup. This enables customization of the Avalonia environment within a generic host.</remarks>
        /// <returns>An instance of IHostBuilder that has been configured for Avalonia support.</returns>
        public IHostBuilder ConfigureAvalonia() =>
            hostBuilder.ConfigureAvalonia(null);

        /// <summary>Configures Avalonia and applies the supplied customization.</summary>
        /// <param name="configureDelegate">A delegate that allows further customization of the Avalonia builder.</param>
        /// <returns>An instance of IHostBuilder that has been configured for Avalonia support.</returns>
        public IHostBuilder ConfigureAvalonia(Action<IAvaloniaBuilder>? configureDelegate)
        {
            _ = hostBuilder ?? throw new ArgumentNullException(nameof(hostBuilder));

            return InternalBuilderAvaloniaUtility.ConfigureAvalonia(hostBuilder, configureDelegate);
        }
    }
}
