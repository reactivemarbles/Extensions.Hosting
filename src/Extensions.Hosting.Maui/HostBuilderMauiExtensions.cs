// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Maui.Controls;
using ReactiveMarbles.Extensions.Hosting.Maui.Internals;

namespace ReactiveMarbles.Extensions.Hosting.Maui;

/// <summary>
/// This contains the MAUI extensions for Microsoft.Extensions.Hosting.
/// </summary>
public static class HostBuilderMauiExtensions
{
    private const string MauiContextKey = nameof(MauiContext);

    /// <summary>
    /// Defines that stopping the MAUI application also stops the host (application).
    /// </summary>
    /// <param name="hostBuilder">IHostBuilder.</param>
    /// <returns>A IHostBuilder.</returns>
    public static IHostBuilder? UseMauiLifetime(this IHostBuilder hostBuilder) =>
        hostBuilder?.ConfigureServices((_, __) =>
        {
            TryRetrieveMauiContext(hostBuilder.Properties, out var mauiContext);
            mauiContext.IsLifetimeLinked = true;
        });

    /// <summary>
    /// Defines that stopping the MAUI application also stops the host (application).
    /// </summary>
    /// <param name="hostBuilder">IHostApplicationBuilder.</param>
    /// <returns>The same IHostApplicationBuilder instance.</returns>
    public static IHostApplicationBuilder UseMauiLifetime(this IHostApplicationBuilder hostBuilder)
    {
        ArgumentNullException.ThrowIfNull(hostBuilder);

        TryRetrieveMauiContext(hostBuilder.Properties, out var mauiContext);
        mauiContext.IsLifetimeLinked = true;
        return hostBuilder;
    }

    /// <summary>
    /// Configure an MAUI application.
    /// </summary>
    /// <param name="hostBuilder">IHostBuilder.</param>
    /// <param name="configureDelegate">Action to configure Maui.</param>
    /// <returns>A IHostBuilder.</returns>
    public static IHostBuilder ConfigureMaui(this IHostBuilder hostBuilder, Action<IMauiBuilder>? configureDelegate = null)
    {
        ArgumentNullException.ThrowIfNull(hostBuilder);

        var mauiBuilder = new MauiBuilder();
        configureDelegate?.Invoke(mauiBuilder);

        hostBuilder.ConfigureServices((_, serviceCollection) =>
        {
            if (!TryRetrieveMauiContext(hostBuilder.Properties, out var mauiContext))
            {
                serviceCollection
                    .AddSingleton(mauiContext)
                    .AddSingleton(serviceProvider => new MauiThread(serviceProvider))
                    .AddHostedService<MauiHostedService>();
            }

            mauiBuilder.ConfigureContextAction?.Invoke(mauiContext);
        });

        if (mauiBuilder.ApplicationType != null)
        {
            // Check if the registered application does inherit Microsoft.Maui.Controls.Application
            var baseApplicationType = typeof(Application);
            if (!baseApplicationType.IsAssignableFrom(mauiBuilder.ApplicationType))
            {
                throw new ArgumentException("The registered Application type must inherit Microsoft.Maui.Controls.Application", nameof(configureDelegate));
            }

            hostBuilder.ConfigureServices((_, serviceCollection) =>
            {
                if (mauiBuilder.Application != null)
                {
                    // Add existing Application
                    serviceCollection.AddSingleton(mauiBuilder.ApplicationType, mauiBuilder.Application);
                }
                else
                {
                    serviceCollection.AddSingleton(mauiBuilder.ApplicationType);
                }

                if (mauiBuilder.ApplicationType != baseApplicationType)
                {
                    serviceCollection.AddSingleton(serviceProvider => (Application)serviceProvider.GetRequiredService(mauiBuilder.ApplicationType));
                }
            });
        }

        if (mauiBuilder.PageTypes.Count > 0)
        {
            hostBuilder.ConfigureServices((_, serviceCollection) =>
            {
                foreach (var mauiPageType in mauiBuilder.PageTypes)
                {
                    serviceCollection.AddSingleton(mauiPageType);

                    // Check if it also implements IMauiShell so we can register it as this
                    var shellInterfaceType = typeof(IMauiShell);
                    if (shellInterfaceType.IsAssignableFrom(mauiPageType))
                    {
                        serviceCollection.AddSingleton(shellInterfaceType, serviceProvider => serviceProvider.GetRequiredService(mauiPageType));
                    }
                }
            });
        }

        return hostBuilder;
    }

    /// <summary>
    /// Configure a MAUI application for the new builder API.
    /// </summary>
    /// <param name="hostBuilder">The IHostApplicationBuilder.</param>
    /// <param name="configureDelegate">Action to configure Maui.</param>
    /// <returns>The same IHostApplicationBuilder instance.</returns>
    public static IHostApplicationBuilder ConfigureMaui(this IHostApplicationBuilder hostBuilder, Action<IMauiBuilder>? configureDelegate = null)
    {
        ArgumentNullException.ThrowIfNull(hostBuilder);

        var mauiBuilder = new MauiBuilder();
        configureDelegate?.Invoke(mauiBuilder);

        if (!TryRetrieveMauiContext(hostBuilder.Properties, out var mauiContext))
        {
            hostBuilder.Services
                .AddSingleton(mauiContext)
                .AddSingleton(serviceProvider => new MauiThread(serviceProvider))
                .AddHostedService<MauiHostedService>();
        }

        mauiBuilder.ConfigureContextAction?.Invoke(mauiContext);

        if (mauiBuilder.ApplicationType != null)
        {
            // Check if the registered application does inherit Microsoft.Maui.Controls.Application
            var baseApplicationType = typeof(Application);
            if (!baseApplicationType.IsAssignableFrom(mauiBuilder.ApplicationType))
            {
                throw new ArgumentException("The registered Application type must inherit Microsoft.Maui.Controls.Application", nameof(configureDelegate));
            }

            if (mauiBuilder.Application != null)
            {
                // Add existing Application
                hostBuilder.Services.AddSingleton(mauiBuilder.ApplicationType, mauiBuilder.Application);
            }
            else
            {
                hostBuilder.Services.AddSingleton(mauiBuilder.ApplicationType);
            }

            if (mauiBuilder.ApplicationType != baseApplicationType)
            {
                hostBuilder.Services.AddSingleton(serviceProvider => (Application)serviceProvider.GetRequiredService(mauiBuilder.ApplicationType));
            }
        }

        if (mauiBuilder.PageTypes.Count > 0)
        {
            foreach (var mauiPageType in mauiBuilder.PageTypes)
            {
                hostBuilder.Services.AddSingleton(mauiPageType);

                // Check if it also implements IMauiShell so we can register it as this
                var shellInterfaceType = typeof(IMauiShell);
                if (shellInterfaceType.IsAssignableFrom(mauiPageType))
                {
                    hostBuilder.Services.AddSingleton(shellInterfaceType, serviceProvider => serviceProvider.GetRequiredService(mauiPageType));
                }
            }
        }

        return hostBuilder;
    }

    /// <summary>
    /// Specify a shell, the primary Page, to start.
    /// </summary>
    /// <param name="hostBuilder">IHostBuilder.</param>
    /// <typeparam name="TShell">Type for the shell, must derive from Page and implement IMauiShell.</typeparam>
    /// <returns>A IHostBuilder.</returns>
    public static IHostBuilder? ConfigureMauiShell<TShell>(this IHostBuilder hostBuilder)
        where TShell : Page, IMauiShell
        => hostBuilder?.ConfigureMaui(maui => maui.UsePage<TShell>());

    /// <summary>
    /// Specify a shell, the primary Page, to start. (IHostApplicationBuilder).
    /// </summary>
    /// <param name="hostBuilder">IHostApplicationBuilder.</param>
    /// <typeparam name="TShell">Type for the shell, must derive from Page and implement IMauiShell.</typeparam>
    /// <returns>The same IHostApplicationBuilder instance.</returns>
    public static IHostApplicationBuilder ConfigureMauiShell<TShell>(this IHostApplicationBuilder hostBuilder)
        where TShell : Page, IMauiShell
        => hostBuilder.ConfigureMaui(maui => maui.UsePage<TShell>());

    /// <summary>
    /// Helper method to retrieve the IMauiContext.
    /// </summary>
    /// <param name="properties">IDictionary.</param>
    /// <param name="mauiContext">IMauiContext out value.</param>
    /// <returns>bool if there was already an IMauiContext.</returns>
    private static bool TryRetrieveMauiContext(this IDictionary<object, object> properties, out IMauiContext mauiContext)
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
}
