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
/// Provides extension methods for configuring and integrating Windows Forms (WinForms) application lifetime and
/// services with .NET Generic Host builders.
/// </summary>
/// <remarks>These extensions enable seamless integration of WinForms applications with the .NET hosting model,
/// allowing developers to configure WinForms services, specify the main form (shell), and control application lifetime
/// in relation to the host. Methods are provided for both IHostBuilder and IHostApplicationBuilder to support different
/// hosting scenarios. Use these extensions to register WinForms components, configure the application context, and
/// ensure proper startup and shutdown coordination between the WinForms UI and the host.</remarks>
public static class HostBuilderWinFormsExtensions
{
    private const string WinFormsContextKey = nameof(WinFormsContext);

    /// <summary>
    /// Configures the host to use Windows Forms lifetime management, enabling the application to start and stop in
    /// coordination with the WinForms message loop.
    /// </summary>
    /// <remarks>This method links the application's lifetime to the Windows Forms message loop, so the host
    /// will start when the message loop begins and stop when it ends. Use this method when building WinForms
    /// applications that require integration with the generic host infrastructure.</remarks>
    /// <param name="hostBuilder">The host builder to configure for Windows Forms lifetime integration. Cannot be null.</param>
    /// <returns>The same instance of <see cref="IHostBuilder"/> for chaining further configuration, or null if <paramref
    /// name="hostBuilder"/> is null.</returns>
    public static IHostBuilder? UseWinFormsLifetime(this IHostBuilder hostBuilder) =>
        hostBuilder?.ConfigureServices((_, __) =>
        {
            TryRetrieveWinFormsContext(hostBuilder.Properties, out var winFormsContext);
            winFormsContext.IsLifetimeLinked = true;
        });

    /// <summary>
    /// Enables WinForms lifetime management for the application, allowing the host to be controlled by the WinForms
    /// message loop.
    /// </summary>
    /// <remarks>When WinForms lifetime is enabled, the application's lifetime is tied to the WinForms message
    /// loop. This is typically used in WinForms applications to ensure that the host shuts down when the main form
    /// closes.</remarks>
    /// <param name="hostBuilder">The host builder to configure with WinForms lifetime support. Cannot be null.</param>
    /// <returns>The same instance of <see cref="IHostApplicationBuilder"/> for chaining further configuration.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="hostBuilder"/> is null.</exception>
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
    /// Configures WinForms support for the specified host builder, enabling integration of Windows Forms message loop
    /// and services into the application's hosting environment.
    /// </summary>
    /// <remarks>This method registers the necessary services to run a Windows Forms message loop within a
    /// generic host. Call this method before building the host to ensure WinForms integration is available throughout
    /// the application's lifetime.</remarks>
    /// <param name="hostBuilder">The host builder to configure for WinForms support. Cannot be null.</param>
    /// <param name="configureAction">An optional delegate to further configure the WinForms context. If null, no additional configuration is
    /// performed.</param>
    /// <returns>The same instance of <see cref="IHostBuilder"/> with WinForms services configured, or null if <paramref
    /// name="hostBuilder"/> is null.</returns>
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
    /// Configures WinForms support for the specified host application builder and optionally allows additional
    /// WinForms-specific configuration.
    /// </summary>
    /// <remarks>This method registers the necessary services to enable WinForms integration in a generic host
    /// application. If called multiple times, WinForms services are only registered once. Use the <paramref
    /// name="configureAction"/> parameter to customize the WinForms context as needed.</remarks>
    /// <param name="hostBuilder">The host application builder to configure for WinForms support. Cannot be null.</param>
    /// <param name="configureAction">An optional delegate to configure the WinForms context after it is created. If null, no additional configuration
    /// is performed.</param>
    /// <returns>The same instance of <see cref="IHostApplicationBuilder"/> with WinForms services configured.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="hostBuilder"/> is null.</exception>
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
    /// Configures WinForms support for the host builder and registers the specified main view form as a singleton
    /// service.
    /// </summary>
    /// <remarks>If <typeparamref name="TView"/> also implements <see cref="IWinFormsShell"/>, it is
    /// registered as a singleton service for that interface as well. This method enables dependency injection and
    /// configuration for WinForms applications using the generic host.</remarks>
    /// <typeparam name="TView">The type of the main WinForms form to register. Must inherit from <see cref="Form"/>.</typeparam>
    /// <param name="hostBuilder">The <see cref="IHostBuilder"/> to configure for WinForms support.</param>
    /// <param name="configureAction">An optional action to further configure the WinForms context. May be <see langword="null"/>.</param>
    /// <returns>The same <see cref="IHostBuilder"/> instance for chaining, or <see langword="null"/> if <paramref
    /// name="hostBuilder"/> is <see langword="null"/>.</returns>
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
    /// Configures WinForms support for the application and registers the specified main form type as a singleton
    /// service.
    /// </summary>
    /// <remarks>If <typeparamref name="TView"/> also implements <see cref="IWinFormsShell"/>, it is
    /// registered as a singleton service for that interface as well. This enables dependency injection of the main form
    /// and shell interface throughout the application.</remarks>
    /// <typeparam name="TView">The type of the main form to register. Must inherit from <see cref="Form"/>.</typeparam>
    /// <param name="hostBuilder">The host application builder to configure.</param>
    /// <param name="configureAction">An optional delegate to configure additional WinForms services or settings. Can be <see langword="null"/>.</param>
    /// <returns>The same <see cref="IHostApplicationBuilder"/> instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="hostBuilder"/> is <see langword="null"/>.</exception>
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
    /// Configures the specified host builder to use a WinForms shell of the given type as the application's main
    /// window.
    /// </summary>
    /// <typeparam name="TShell">The type of the WinForms shell form to use as the application's main window. Must inherit from <see
    /// cref="Form"/> and implement <see cref="IWinFormsShell"/>.</typeparam>
    /// <param name="hostBuilder">The host builder to configure for WinForms shell integration. Cannot be null.</param>
    /// <returns>The same <see cref="IHostBuilder"/> instance for chaining, or <see langword="null"/> if <paramref
    /// name="hostBuilder"/> is null.</returns>
    public static IHostBuilder? ConfigureWinFormsShell<TShell>(this IHostBuilder hostBuilder)
        where TShell : Form, IWinFormsShell
        => hostBuilder?.ConfigureWinForms<TShell>();

    /// <summary>
    /// Configures the WinForms shell for the application using the specified shell form type.
    /// </summary>
    /// <typeparam name="TShell">The type of the main shell form to use for the application. Must inherit from Form and implement IWinFormsShell.</typeparam>
    /// <param name="hostBuilder">The host application builder to configure.</param>
    /// <returns>The same IHostApplicationBuilder instance for chaining further configuration.</returns>
    public static IHostApplicationBuilder ConfigureWinFormsShell<TShell>(this IHostApplicationBuilder hostBuilder)
        where TShell : Form, IWinFormsShell
        => hostBuilder.ConfigureWinForms<TShell>();

    /// <summary>
    /// Attempts to retrieve an existing Windows Forms context from the specified property dictionary.
    /// </summary>
    /// <remarks>If a Windows Forms context does not exist in the dictionary, this method creates a new
    /// context, adds it to the dictionary, and returns it in the out parameter.</remarks>
    /// <param name="properties">The dictionary containing property values, which may include a Windows Forms context entry.</param>
    /// <param name="winFormsContext">When this method returns, contains the retrieved Windows Forms context if found; otherwise, a new context
    /// instance.</param>
    /// <returns>true if an existing Windows Forms context was found in the dictionary; otherwise, false.</returns>
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
