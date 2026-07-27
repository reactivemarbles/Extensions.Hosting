// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using Avalonia.Controls;
using Microsoft.Extensions.Hosting;

namespace ReactiveMarbles.Extensions.Hosting.Avalonia;

/// <summary>Provides extension methods for configuring Avalonia support in a host application builder.</summary>
/// <remarks>These methods enable integration of Avalonia's lifetime management and application builder
/// customization within a generic host application. Use them to ensure proper shutdown behavior and to apply
/// Avalonia-specific settings.</remarks>
public static class HostApplicationBuilderAvaloniaExtensions
{
    /// <summary>Provides extension members for this receiver.</summary>
    /// <param name="hostApplicationBuilder">The receiver instance.</param>
    extension(IHostApplicationBuilder hostApplicationBuilder)
    {
        /// <summary>Configures Avalonia lifetime management for the application, enabling controlled shutdown behavior based on the specified shutdown mode.</summary>
        /// <remarks>This method integrates Avalonia's lifetime management into the host application, ensuring
        /// that the application shuts down according to the selected mode. Use this extension when building Avalonia
        /// applications with generic host support.</remarks>
        /// <returns>The updated <see cref="IHostApplicationBuilder"/> instance, allowing for further configuration.</returns>
        public IHostApplicationBuilder UseAvaloniaLifetime() =>
            hostApplicationBuilder.UseAvaloniaLifetime(ShutdownMode.OnLastWindowClose);

        /// <summary>Configures Avalonia lifetime management with the specified shutdown mode.</summary>
        /// <param name="shutdownMode">Specifies the shutdown behavior for the application.</param>
        /// <returns>The updated <see cref="IHostApplicationBuilder"/> instance, allowing for further configuration.</returns>
        public IHostApplicationBuilder UseAvaloniaLifetime(ShutdownMode shutdownMode)
        {
            _ = hostApplicationBuilder ?? throw new ArgumentNullException(nameof(hostApplicationBuilder));

            return InternalBuilderAvaloniaUtility.UseAvaloniaLifetime(hostApplicationBuilder, shutdownMode);
        }

        /// <summary>Configures the Avalonia application builder for integration with the host application builder, optionally allowing further customization.</summary>
        /// <remarks>Use this method to enable Avalonia support in your application's host builder. The optional
        /// delegate allows you to modify Avalonia-specific settings before the application is built.</remarks>
        /// <returns>An instance of IHostApplicationBuilder that is configured to support Avalonia.</returns>
        public IHostApplicationBuilder ConfigureAvalonia() =>
            hostApplicationBuilder.ConfigureAvalonia(null);

        /// <summary>Configures Avalonia and applies the supplied customization.</summary>
        /// <param name="configureDelegate">A delegate that provides additional customization for the Avalonia builder.</param>
        /// <returns>An instance of IHostApplicationBuilder that is configured to support Avalonia.</returns>
        public IHostApplicationBuilder ConfigureAvalonia(Action<IAvaloniaBuilder>? configureDelegate) =>
            InternalBuilderAvaloniaUtility.ConfigureAvalonia(hostApplicationBuilder, configureDelegate);
    }
}
