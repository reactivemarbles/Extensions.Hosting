// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Maui.Controls;
using ReactiveMarbles.Extensions.Hosting.Maui.Internals;

namespace ReactiveMarbles.Extensions.Hosting.Maui;

/// <summary>Provides extension methods for configuring and integrating .NET MAUI applications with generic host builders.</summary>
/// <remarks>These extensions enable seamless setup of MAUI applications using dependency injection and host-based
/// lifecycles. They support both traditional IHostBuilder and the newer IHostApplicationBuilder APIs, allowing
/// developers to configure MAUI services, set the application shell, and link the application lifetime to the host. Use
/// these methods to simplify MAUI app initialization and service registration when building cross-platform applications
/// with .NET MAUI and the generic host.</remarks>
public static class HostBuilderMauiExtensions
{
    /// <summary>Stores the maui context key value.</summary>
    private const string MauiContextKey = nameof(MauiContext);

    /// <summary>Attempts to retrieve an existing IMauiContext instance from the specified property dictionary.</summary>
    /// <remarks>If the IMauiContext is not present in the dictionary, this method creates a new instance,
    /// assigns it to the out parameter, and adds it to the dictionary for future retrieval.</remarks>
    /// <param name="properties">The property dictionary used to store the context.</param>
    /// <param name="mauiContext">When this method returns, contains the IMauiContext instance retrieved from the dictionary if found; otherwise,
    /// a new IMauiContext instance.</param>
    /// <returns>true if an existing IMauiContext was found in the dictionary; otherwise, false.</returns>
    private static bool TryRetrieveMauiContext(IDictionary<object, object> properties, out IMauiContext mauiContext)
    {
        if (properties.TryGetValue(MauiContextKey, out var mauiContextAsObject))
        {
            mauiContext = (IMauiContext)mauiContextAsObject;
            return true;
        }

        mauiContext = new MauiContext();
        properties[MauiContextKey] = mauiContext;
        return false;
    }

    /// <summary>Uses the currently running MAUI application instance when it matches the configured application type.</summary>
    /// <param name="mauiBuilder">The builder that contains application configuration.</param>
    private static void CaptureCurrentMauiApplication(MauiBuilder mauiBuilder)
    {
        if (mauiBuilder.ApplicationType is null || mauiBuilder.Application is not null || Application.Current?.GetType() != mauiBuilder.ApplicationType)
        {
            return;
        }

        mauiBuilder.Application = Application.Current;
    }

    /// <summary>Registers the core MAUI hosting services.</summary>
    /// <param name="services">The service collection to register services into.</param>
    /// <param name="mauiContext">The MAUI context instance to register.</param>
    private static void RegisterMauiHostingServices(IServiceCollection services, IMauiContext mauiContext) =>
        _ = services
            .AddSingleton(mauiContext)
            .AddSingleton(serviceProvider => new MauiThread(serviceProvider))
            .AddHostedService<MauiHostedService>();

    /// <summary>Registers a configured MAUI application type or instance.</summary>
    /// <param name="services">The service collection to register services into.</param>
    /// <param name="mauiBuilder">The builder that contains the application configuration.</param>
    /// <param name="parameterName">The public parameter name used for exception reporting.</param>
    /// <exception cref="ArgumentException">Thrown if the configured application type does not derive from <see cref="Application"/>.</exception>
    private static void RegisterMauiApplication(IServiceCollection services, MauiBuilder mauiBuilder, string parameterName)
    {
        if (mauiBuilder.ApplicationType is null)
        {
            return;
        }

        var baseApplicationType = typeof(Application);
        if (!baseApplicationType.IsAssignableFrom(mauiBuilder.ApplicationType))
        {
            throw new ArgumentException("The registered Application type must inherit Microsoft.Maui.Controls.Application", parameterName);
        }

        if (mauiBuilder.Application is not null)
        {
            _ = services.AddSingleton(mauiBuilder.ApplicationType, mauiBuilder.Application);
        }
        else
        {
            _ = services.AddSingleton(mauiBuilder.ApplicationType);
        }

        if (mauiBuilder.ApplicationType == baseApplicationType)
        {
            return;
        }

        _ = services.AddSingleton(serviceProvider => (Application)serviceProvider.GetRequiredService(mauiBuilder.ApplicationType));
    }

    /// <summary>Registers configured MAUI pages and shell mappings.</summary>
    /// <param name="services">The service collection to register services into.</param>
    /// <param name="mauiBuilder">The builder that contains the page configuration.</param>
    private static void RegisterMauiPages(IServiceCollection services, MauiBuilder mauiBuilder)
    {
        foreach (var mauiPageType in mauiBuilder.PageTypes)
        {
            _ = services.AddSingleton(mauiPageType);

            var shellInterfaceType = typeof(IMauiShell);
            if (!shellInterfaceType.IsAssignableFrom(mauiPageType))
            {
                continue;
            }

            _ = services.AddSingleton(shellInterfaceType, serviceProvider => serviceProvider.GetRequiredService(mauiPageType));
        }
    }

    /// <summary>Provides extension members for this receiver.</summary>
    /// <param name="hostBuilder">The receiver instance.</param>
    extension(IHostApplicationBuilder hostBuilder)
    {
        /// <summary>Enables the MAUI-specific application lifetime integration for the specified host builder.</summary>
        /// <remarks>This method configures the host builder to use the MAUI application lifetime, which manages
        /// the application's startup and shutdown events in a manner compatible with .NET MAUI. Call this method when
        /// building a MAUI app to ensure correct lifetime management.</remarks>
        /// <returns>The same instance of <see cref="IHostApplicationBuilder"/> for chaining further configuration.</returns>
        public IHostApplicationBuilder UseMauiLifetime()
        {
            ArgumentNullException.ThrowIfNull(hostBuilder);

            _ = TryRetrieveMauiContext(hostBuilder.Properties, out var mauiContext);
            mauiContext.IsLifetimeLinked = true;
            return hostBuilder;
        }

        /// <summary>Configures the .NET MAUI services and application types for the specified host builder.</summary>
        /// <remarks>This method should be called during application startup to ensure that all required MAUI
        /// services and application types are registered with the dependency injection container. If an existing
        /// Application instance is available, it will be registered; otherwise, the Application type will be registered for
        /// instantiation by the container.</remarks>
        /// <param name="configureDelegate">An optional delegate to further configure the MAUI builder before services are registered. If null, default
        /// configuration is applied.</param>
        /// <returns>The same host builder instance, configured with .NET MAUI services and application types.</returns>
        /// <exception cref="ArgumentException">Thrown if the registered Application type does not inherit from Microsoft.Maui.Controls.Application.</exception>
        public IHostApplicationBuilder ConfigureMaui(Action<IMauiBuilder>? configureDelegate = null)
        {
            ArgumentNullException.ThrowIfNull(hostBuilder);

            var mauiBuilder = new MauiBuilder();
            configureDelegate?.Invoke(mauiBuilder);

            CaptureCurrentMauiApplication(mauiBuilder);

            if (!TryRetrieveMauiContext(hostBuilder.Properties, out var mauiContext))
            {
                RegisterMauiHostingServices(hostBuilder.Services, mauiContext);
            }

            mauiBuilder.ConfigureContextAction?.Invoke(mauiContext);
            RegisterMauiApplication(hostBuilder.Services, mauiBuilder, nameof(configureDelegate));
            RegisterMauiPages(hostBuilder.Services, mauiBuilder);

            var app = mauiBuilder.MauiAppBuilder.Build();
            _ = hostBuilder.Services.AddSingleton(app);

            return hostBuilder;
        }

        /// <summary>Configures the specified host builder to use a custom shell page as the application's main navigation shell.</summary>
        /// <remarks>This method registers the specified shell page type as a singleton and sets it as the root
        /// navigation shell for the application. Use this method to customize the application's navigation structure by
        /// providing your own implementation of IMauiShell.</remarks>
        /// <typeparam name="TShell">The type of the shell page to use as the application's main navigation shell. Must implement the IMauiShell
        /// interface and derive from Page.</typeparam>
        /// <returns>The same IHostApplicationBuilder instance, enabling further configuration.</returns>
        public IHostApplicationBuilder ConfigureMauiShell<TShell>()
            where TShell : Page, IMauiShell
            => hostBuilder.ConfigureMaui(maui => maui.AddSingletonPage<TShell>());
    }

    /// <summary>Provides extension members for this receiver.</summary>
    /// <param name="hostBuilder">The receiver instance.</param>
    extension(IHostBuilder hostBuilder)
    {
        /// <summary>Configures the host builder to use the .NET MAUI application lifetime, enabling integration with the MAUI app lifecycle.</summary>
        /// <remarks>Call this method during host configuration to ensure that the application's lifetime is
        /// managed according to .NET MAUI conventions. This is typically required for MAUI apps to handle startup and
        /// shutdown events correctly.</remarks>
        /// <returns>The same instance of <see cref="IHostBuilder"/> with MAUI lifetime services configured, or <see
        /// langword="null"/> if <paramref name="hostBuilder"/> is null.</returns>
        public IHostBuilder? UseMauiLifetime() =>
            hostBuilder?.ConfigureServices((context, services) =>
            {
                _ = TryRetrieveMauiContext(hostBuilder.Properties, out var mauiContext);
                mauiContext.IsLifetimeLinked = true;
            });

        /// <summary>Configures the .NET MAUI application and related services for the specified host builder.</summary>
        /// <remarks>Call this method to add .NET MAUI support to a generic host builder, enabling dependency
        /// injection and service registration for MAUI applications. This method should be called before building the host.
        /// If an application type is registered, it must inherit from <see
        /// cref="Microsoft.Maui.Controls.Application"/>.</remarks>
        /// <param name="configureDelegate">An optional delegate to further configure the MAUI application, pages, and services before they are registered
        /// with the host builder. May be null.</param>
        /// <returns>The same instance of <see cref="IHostBuilder"/> with .NET MAUI services and configuration applied. This enables
        /// further chaining of host builder configuration methods.</returns>
        /// <exception cref="ArgumentException">Thrown if the application type registered via <paramref name="configureDelegate"/> does not inherit from <see
        /// cref="Microsoft.Maui.Controls.Application"/>.</exception>
        public IHostBuilder ConfigureMaui(Action<IMauiBuilder>? configureDelegate = null)
        {
            ArgumentNullException.ThrowIfNull(hostBuilder);

            var mauiBuilder = new MauiBuilder();
            configureDelegate?.Invoke(mauiBuilder);

            CaptureCurrentMauiApplication(mauiBuilder);

            _ = hostBuilder.ConfigureServices((context, serviceCollection) =>
            {
                if (!TryRetrieveMauiContext(hostBuilder.Properties, out var mauiContext))
                {
                    RegisterMauiHostingServices(serviceCollection, mauiContext);
                }

                mauiBuilder.ConfigureContextAction?.Invoke(mauiContext);
            });

            if (mauiBuilder.ApplicationType is not null)
            {
                _ = hostBuilder.ConfigureServices((context, serviceCollection) => RegisterMauiApplication(serviceCollection, mauiBuilder, nameof(configureDelegate)));
            }

            if (mauiBuilder.PageTypes.Count > 0)
            {
                _ = hostBuilder.ConfigureServices(serviceCollection => RegisterMauiPages(serviceCollection, mauiBuilder));
            }

            return hostBuilder;
        }

        /// <summary>Configures the specified host builder to use a singleton instance of the specified shell page type as the application's main shell.</summary>
        /// <remarks>This method registers the specified shell page type as a singleton in the dependency
        /// injection container, enabling it to serve as the application's main navigation shell. Use this method during
        /// application startup to set up the shell for a .NET MAUI app.</remarks>
        /// <typeparam name="TShell">The type of the shell page to use as the application's main shell. Must implement the IMauiShell interface and
        /// derive from Page.</typeparam>
        /// <returns>The configured host builder instance, or null if the input host builder is null.</returns>
        public IHostBuilder? ConfigureMauiShell<TShell>()
            where TShell : Page, IMauiShell
            => hostBuilder?.ConfigureMaui(maui => maui.AddSingletonPage<TShell>());
    }
}
