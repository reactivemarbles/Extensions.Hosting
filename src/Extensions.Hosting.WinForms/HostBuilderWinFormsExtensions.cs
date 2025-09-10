// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactiveMarbles.Extensions.Hosting.WinForms.Internals;

namespace ReactiveMarbles.Extensions.Hosting.WinForms;

/// <summary>
/// This contains the WinForms extensions for Microsoft.Extensions.Hosting.
/// </summary>
public static class HostBuilderWinFormsExtensions
{
    private const string WinFormsContextKey = nameof(WinFormsContext);

    /// <summary>
    /// Defines that stopping the WinForms application also stops the host (application).
    /// </summary>
    /// <param name="hostBuilder">IHostBuilder.</param>
    /// <returns>A IHostBuilder.</returns>
    public static IHostBuilder? UseWinFormsLifetime(this IHostBuilder hostBuilder) =>
        hostBuilder?.ConfigureServices((_, __) =>
        {
            TryRetrieveWinFormsContext(hostBuilder.Properties, out var winFormsContext);
            winFormsContext.IsLifetimeLinked = true;
        });

    /// <summary>
    /// Defines that stopping the WinForms application also stops the host (application).
    /// </summary>
    /// <param name="hostBuilder">IHostApplicationBuilder.</param>
    /// <returns>The same IHostApplicationBuilder instance.</returns>
    public static IHostApplicationBuilder UseWinFormsLifetime(this IHostApplicationBuilder hostBuilder)
    {
        if (hostBuilder == null)
        {
            throw new ArgumentNullException(nameof(hostBuilder));
        }

        TryRetrieveWinFormsContext(hostBuilder.Properties, out var winFormsContext);
        winFormsContext.IsLifetimeLinked = true;
        return hostBuilder;
    }

    /// <summary>
    /// Configure an WinForms application.
    /// </summary>
    /// <param name="hostBuilder">IHostBuilder.</param>
    /// <param name="configureAction">Action to configure the Application.</param>
    /// <returns>A IHostBuilder.</returns>
    public static IHostBuilder? ConfigureWinForms(this IHostBuilder hostBuilder, Action<IWinFormsContext>? configureAction = null) =>
        hostBuilder?.ConfigureServices((_, serviceCollection) =>
        {
            if (!TryRetrieveWinFormsContext(hostBuilder.Properties, out var winFormsContext))
            {
                serviceCollection
                    .AddSingleton(winFormsContext)
                    .AddSingleton(serviceProvider => new WinFormsThread(serviceProvider))
                    .AddHostedService<WinFormsHostedService>();
            }

            configureAction?.Invoke(winFormsContext);
        });

    /// <summary>
    /// Configure an WinForms application.
    /// </summary>
    /// <param name="hostBuilder">IHostApplicationBuilder.</param>
    /// <param name="configureAction">Action to configure the Application.</param>
    /// <returns>The same IHostApplicationBuilder instance.</returns>
    public static IHostApplicationBuilder ConfigureWinForms(this IHostApplicationBuilder hostBuilder, Action<IWinFormsContext>? configureAction = null)
    {
        if (hostBuilder == null)
        {
            throw new ArgumentNullException(nameof(hostBuilder));
        }

        if (!TryRetrieveWinFormsContext(hostBuilder.Properties, out var winFormsContext))
        {
            hostBuilder.Services
                .AddSingleton(winFormsContext)
                .AddSingleton(serviceProvider => new WinFormsThread(serviceProvider))
                .AddHostedService<WinFormsHostedService>();
        }

        configureAction?.Invoke(winFormsContext);
        return hostBuilder;
    }

    /// <summary>
    /// Configure an WinForms application.
    /// </summary>
    /// <param name="hostBuilder">IHostBuilder.</param>
    /// <param name="configureAction">Action to configure the Application.</param>
    /// <typeparam name="TView">Type for the View.</typeparam>
    /// <returns>A IHostBuilder.</returns>
    public static IHostBuilder? ConfigureWinForms<TView>(this IHostBuilder hostBuilder, Action<IWinFormsContext>? configureAction = null)
        where TView : Form
        =>
        hostBuilder?
            .ConfigureWinForms(configureAction)?
            .ConfigureServices((_, serviceCollection) =>
            {
                serviceCollection.AddSingleton<TView>();

                // Check if it also implements IWinFormsShell so we can register it as this
                var viewType = typeof(TView);
                var shellInterfaceType = typeof(IWinFormsShell);
                if (shellInterfaceType.IsAssignableFrom(viewType))
                {
                    serviceCollection.AddSingleton(shellInterfaceType, serviceProvider => serviceProvider.GetRequiredService<TView>());
                }
            });

    /// <summary>
    /// Configure an WinForms application for IHostApplicationBuilder.
    /// </summary>
    /// <param name="hostBuilder">IHostApplicationBuilder.</param>
    /// <param name="configureAction">Action to configure the Application.</param>
    /// <typeparam name="TView">Type for the View.</typeparam>
    /// <returns>The same IHostApplicationBuilder instance.</returns>
    public static IHostApplicationBuilder ConfigureWinForms<TView>(this IHostApplicationBuilder hostBuilder, Action<IWinFormsContext>? configureAction = null)
        where TView : Form
    {
        if (hostBuilder == null)
        {
            throw new ArgumentNullException(nameof(hostBuilder));
        }

        hostBuilder.ConfigureWinForms(configureAction);

        hostBuilder.Services.AddSingleton<TView>();

        // Check if it also implements IWinFormsShell so we can register it as this
        var viewType = typeof(TView);
        var shellInterfaceType = typeof(IWinFormsShell);
        if (shellInterfaceType.IsAssignableFrom(viewType))
        {
            hostBuilder.Services.AddSingleton(shellInterfaceType, serviceProvider => serviceProvider.GetRequiredService<TView>());
        }

        return hostBuilder;
    }

    /// <summary>
    /// Specify a shell, the primary Form, to start.
    /// </summary>
    /// <param name="hostBuilder">IHostBuilder.</param>
    /// <typeparam name="TShell">Type for the shell, must derive from Form and implement IWinFormsShell.</typeparam>
    /// <returns>A IHostBuilder.</returns>
    public static IHostBuilder? ConfigureWinFormsShell<TShell>(this IHostBuilder hostBuilder)
        where TShell : Form, IWinFormsShell
        => hostBuilder?.ConfigureWinForms<TShell>();

    /// <summary>
    /// Specify a shell, the primary Form, to start. (IHostApplicationBuilder).
    /// </summary>
    /// <param name="hostBuilder">IHostApplicationBuilder.</param>
    /// <typeparam name="TShell">Type for the shell, must derive from Form and implement IWinFormsShell.</typeparam>
    /// <returns>The same IHostApplicationBuilder instance.</returns>
    public static IHostApplicationBuilder ConfigureWinFormsShell<TShell>(this IHostApplicationBuilder hostBuilder)
        where TShell : Form, IWinFormsShell
        => hostBuilder.ConfigureWinForms<TShell>();

    /// <summary>
    /// Helper method to retrieve the IWinFormsContext.
    /// </summary>
    /// <param name="properties">IDictionary.</param>
    /// <param name="winFormsContext">IWinFormsContext out value.</param>
    /// <returns>bool if there was already an IWinFormsContext.</returns>
    private static bool TryRetrieveWinFormsContext(this IDictionary<object, object> properties, out IWinFormsContext winFormsContext)
    {
        if (properties.TryGetValue(WinFormsContextKey, out var winFormsContextAsObject))
        {
            winFormsContext = (IWinFormsContext)winFormsContextAsObject;
            return true;
        }

        winFormsContext = new WinFormsContext();
        properties[WinFormsContextKey] = winFormsContext;
        return false;
    }
}
