// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
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
        /// <param name="shutdownMode">Specifies the shutdown mode for the Avalonia application. The default is <see
        /// cref="ShutdownMode.OnLastWindowClose"/>, which shuts down the application when the last window is closed.</param>
        /// <returns>The updated <see cref="IHostBuilder"/> instance, allowing for further configuration.</returns>
        public IHostBuilder UseAvaloniaLifetime(ShutdownMode shutdownMode = ShutdownMode.OnLastWindowClose)
        {
            if (hostBuilder is null)
            {
                throw new ArgumentNullException(nameof(hostBuilder));
            }

            return InternalBuilderAvaloniaUtility.UseAvaloniaLifetime(hostBuilder, shutdownMode);
        }

        /// <summary>Configures the Avalonia framework for the specified host builder, enabling integration and customization of Avalonia application settings before startup.</summary>
        /// <remarks>Use this method to set up Avalonia-specific configurations and options prior to application
        /// startup. This enables customization of the Avalonia environment within a generic host.</remarks>
        /// <param name="configureDelegate">An optional delegate that allows further customization of the Avalonia builder. If provided, it is invoked to
        /// configure Avalonia-specific options.</param>
        /// <returns>An instance of IHostBuilder that has been configured for Avalonia support.</returns>
        public IHostBuilder ConfigureAvalonia(Action<IAvaloniaBuilder>? configureDelegate = null)
        {
            if (hostBuilder is null)
            {
                throw new ArgumentNullException(nameof(hostBuilder));
            }

            return InternalBuilderAvaloniaUtility.ConfigureAvalonia(hostBuilder, configureDelegate);
        }
    }
}
