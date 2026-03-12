// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactiveMarbles.Extensions.Hosting.Avalonia.Internals;

namespace ReactiveMarbles.Extensions.Hosting.Avalonia;

/// <summary>
/// Provides utility methods for configuring Avalonia integration within a host application, enabling proper lifetime
/// management and service registration.
/// </summary>
/// <remarks>This class contains static methods that facilitate the setup of Avalonia in a generic host
/// environment, ensuring that the application can manage its lifecycle and dependencies effectively.</remarks>
internal static class InternalBuilderAvaloniaUtility
{
    private const string AvaloniaContextKey = "AvaloniaContext";

    /// <summary>
    /// Configures Avalonia lifetime management for the application host, enabling proper shutdown behavior based on the
    /// specified shutdown mode.
    /// </summary>
    /// <remarks>This method integrates Avalonia's window management with the application's lifetime, ensuring
    /// that the application shuts down according to user interactions with the UI. Use this method to enable
    /// Avalonia-specific lifetime handling in a generic host environment.</remarks>
    /// <param name="hostBuilder">The host builder used to configure application services and lifetime management.</param>
    /// <param name="shutdownMode">Specifies the application's shutdown behavior. The default is <see cref="ShutdownMode.OnLastWindowClose"/>,
    /// which shuts down the application when the last window is closed.</param>
    /// <returns>The host builder instance, allowing for further configuration of the application host.</returns>
    internal static IHostBuilder UseAvaloniaLifetime(IHostBuilder hostBuilder, ShutdownMode shutdownMode = ShutdownMode.OnLastWindowClose) =>
        hostBuilder.ConfigureServices((hostBuilderContext, serviceCollection) => InnerUseAvaloniaLifetime(hostBuilder.Properties, shutdownMode));

    /// <summary>
    /// Configures the Avalonia application by adding Avalonia services to the host builder and applying an optional
    /// configuration delegate.
    /// </summary>
    /// <remarks>This method integrates Avalonia into the application's dependency injection system, enabling
    /// Avalonia UI functionality within a generic host environment. Use the configuration delegate to customize
    /// Avalonia-specific options as needed.</remarks>
    /// <param name="hostBuilder">The host builder used to configure application services and properties. Cannot be null.</param>
    /// <param name="configureDelegate">An optional delegate that allows further customization of the Avalonia builder during configuration. If
    /// provided, it will be invoked to modify Avalonia-specific settings.</param>
    /// <returns>An instance of IHostBuilder that has been configured to support Avalonia features and services.</returns>
    internal static IHostBuilder ConfigureAvalonia(IHostBuilder hostBuilder, Action<IAvaloniaBuilder>? configureDelegate = null) =>
        hostBuilder.ConfigureServices((hostBuilderContext, serviceCollection) => InnerConfigureAvalonia(hostBuilder.Properties, serviceCollection, configureDelegate));

    /// <summary>
    /// Configures Avalonia lifetime management for the application host, enabling proper shutdown behavior according to
    /// the specified shutdown mode.
    /// </summary>
    /// <remarks>This method integrates Avalonia's lifetime management into the application's host, ensuring
    /// that the application shuts down correctly based on the defined shutdown mode.</remarks>
    /// <param name="hostApplicationBuilder">The application builder used to configure the host for the Avalonia application.</param>
    /// <param name="shutdownMode">Specifies the shutdown behavior of the application. The default is <see cref="ShutdownMode.OnLastWindowClose"/>,
    /// which shuts down the application when the last window is closed.</param>
    /// <returns>The updated application builder instance for further configuration.</returns>
    internal static IHostApplicationBuilder UseAvaloniaLifetime(IHostApplicationBuilder hostApplicationBuilder, ShutdownMode shutdownMode = ShutdownMode.OnLastWindowClose)
    {
        InnerUseAvaloniaLifetime(hostApplicationBuilder.Properties, shutdownMode);

        return hostApplicationBuilder;
    }

    /// <summary>
    /// Configures the Avalonia application within the specified host application builder, optionally allowing further
    /// customization through a delegate.
    /// </summary>
    /// <remarks>This method sets up Avalonia integration for the host application builder and applies any
    /// custom configuration provided by the delegate. Use this method to prepare the application builder for
    /// Avalonia-specific features before building the host.</remarks>
    /// <param name="hostApplicationBuilder">The application builder used to configure and initialize the Avalonia application. Cannot be null.</param>
    /// <param name="configureDelegate">An optional delegate that provides additional customization for the Avalonia builder. If specified, it is
    /// invoked during configuration.</param>
    /// <returns>The same instance of the host application builder, configured for Avalonia, to enable further chaining of setup
    /// operations.</returns>
    internal static IHostApplicationBuilder ConfigureAvalonia(IHostApplicationBuilder hostApplicationBuilder, Action<IAvaloniaBuilder>? configureDelegate = null)
    {
        InnerConfigureAvalonia(hostApplicationBuilder.Properties, hostApplicationBuilder.Services, configureDelegate);

        return hostApplicationBuilder;
    }

    /// <summary>
    /// Helper method to retrieve the IAvaloniaContext.
    /// </summary>
    /// <param name="properties">IDictionary.</param>
    /// <param name="avaloniaContext">IAvaloniaContext out value.</param>
    /// <returns>bool if there was already an IAvaloniaContext.</returns>
    private static bool TryRetrieveAvaloniaContext(IDictionary<object, object> properties, out IAvaloniaContext avaloniaContext)
    {
        if (properties.TryGetValue(AvaloniaContextKey, out var avaloniaContextAsObject))
        {
            avaloniaContext = (IAvaloniaContext)avaloniaContextAsObject;
            return true;
        }

        avaloniaContext = new AvaloniaContext();
        properties[AvaloniaContextKey] = avaloniaContext;
        return false;
    }

    private static void InnerUseAvaloniaLifetime(IDictionary<object, object> properties, ShutdownMode shutdownMode)
    {
        if (!TryRetrieveAvaloniaContext(properties, out var avaloniaContext))
        {
            throw new NotSupportedException("Please configure Avalonia first!");
        }

        avaloniaContext.ShutdownMode = shutdownMode;
        avaloniaContext.IsLifetimeLinked = true;
    }

    /// <summary>
    /// Configures the Avalonia application and registers its services with the provided service collection using the
    /// specified properties and optional configuration delegate.
    /// </summary>
    /// <remarks>This method sets up the Avalonia application context and registers necessary services for
    /// dependency injection, including hosted services and window types. It ensures that the Application type is valid
    /// and allows for custom configuration through the provided delegate. Window types implementing IAvaloniaShell are
    /// also registered for shell functionality.</remarks>
    /// <param name="properties">A dictionary containing properties used to configure the Avalonia context and related services. Must not be
    /// null.</param>
    /// <param name="serviceCollection">The service collection to which Avalonia services, including application and window types, will be added. Must
    /// not be null.</param>
    /// <param name="configureDelegate">An optional delegate that allows custom configuration of the Avalonia builder before services are registered.</param>
    /// <exception cref="ArgumentException">Thrown if the registered Application type does not inherit from Avalonia.Application.</exception>
    private static void InnerConfigureAvalonia(IDictionary<object, object> properties, IServiceCollection serviceCollection, Action<IAvaloniaBuilder>? configureDelegate = null)
    {
        var avaloniaBuilder = new AvaloniaBuilder();
        configureDelegate?.Invoke(avaloniaBuilder);

        if (!TryRetrieveAvaloniaContext(properties, out var avaloniaContext))
        {
            serviceCollection.AddSingleton(avaloniaContext);

            // The AppBuilder will be created by resolving the Application from DI
            // and applying any custom configuration
            serviceCollection.AddSingleton(serviceProvider =>
            {
                var appBuilder = AppBuilder.Configure(() => serviceProvider.GetService<Application>() ?? new Application())
                    .UsePlatformDetect()
                    .LogToTrace();

                // Allow custom configuration of the AppBuilder
                avaloniaBuilder.ConfigureAppBuilderAction?.Invoke(appBuilder);

                return new AvaloniaThread(serviceProvider, appBuilder);
            });

            serviceCollection.AddHostedService<AvaloniaHostedService>();
        }

        avaloniaBuilder.ConfigureContextAction?.Invoke(avaloniaContext);

        if (avaloniaBuilder.ApplicationType != null)
        {
            // Check if the registered application does inherit Avalonia.Application
            var baseApplicationType = typeof(Application);
            if (!baseApplicationType.IsAssignableFrom(avaloniaBuilder.ApplicationType))
            {
                throw new ArgumentException("The registered Application type must inherit Avalonia.Application", nameof(configureDelegate));
            }

            if (avaloniaBuilder.Application != null)
            {
                // Add existing Application
                serviceCollection.AddSingleton(avaloniaBuilder.ApplicationType, avaloniaBuilder.Application);
            }
            else
            {
                serviceCollection.AddSingleton(avaloniaBuilder.ApplicationType);
            }

            if (avaloniaBuilder.ApplicationType != baseApplicationType)
            {
                serviceCollection.AddSingleton(serviceProvider => (Application)serviceProvider.GetRequiredService(avaloniaBuilder.ApplicationType));
            }
        }

        if (avaloniaBuilder.WindowTypes.Count > 0)
        {
            foreach (var avaloniaWindowType in avaloniaBuilder.WindowTypes)
            {
                serviceCollection.AddSingleton(avaloniaWindowType);

                // Check if it also implements IAvaloniaShell so we can register it as this
                var shellInterfaceType = typeof(IAvaloniaShell);
                if (shellInterfaceType.IsAssignableFrom(avaloniaWindowType))
                {
                    serviceCollection.AddSingleton(shellInterfaceType, serviceProvider => serviceProvider.GetRequiredService(avaloniaWindowType));
                }
            }
        }
    }
}
