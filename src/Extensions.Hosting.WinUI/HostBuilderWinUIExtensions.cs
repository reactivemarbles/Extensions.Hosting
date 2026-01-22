// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using ReactiveMarbles.Extensions.Hosting.WinUI.Internals;

namespace ReactiveMarbles.Extensions.Hosting.WinUI;

/// <summary>
/// Provides extension methods for configuring WinUI applications with generic host builders.
/// </summary>
/// <remarks>These extensions enable integration of WinUI application and window types into the .NET Generic Host
/// infrastructure, allowing for dependency injection, service registration, and host-managed application lifetimes. Use
/// these methods to set up WinUI applications in a host-based environment, such as when building modern desktop
/// applications with dependency injection and background services.</remarks>
public static class HostBuilderWinUIExtensions
{
    private const string WinUIContextKey = nameof(WinUIContext);

    /// <summary>
    /// Configures WinUI support for the specified application and main window types within the host builder.
    /// </summary>
    /// <remarks>This method registers the WinUI application and main window types with the dependency
    /// injection container and links the application lifetime to the host. Call this method before building the host to
    /// ensure proper WinUI integration.</remarks>
    /// <typeparam name="TApp">The type of the WinUI application to configure. Must inherit from <see cref="Application"/>.</typeparam>
    /// <typeparam name="TAppWindow">The type of the main window for the application. Must inherit from <see cref="Window"/>.</typeparam>
    /// <param name="hostBuilder">The host builder to configure with WinUI services. Cannot be null.</param>
    /// <returns>The same <see cref="IHostBuilder"/> instance for chaining, or <see langword="null"/> if <paramref
    /// name="hostBuilder"/> is null.</returns>
    /// <exception cref="ArgumentException">Thrown if <typeparamref name="TApp"/> does not inherit from <see cref="Application"/>.</exception>
    public static IHostBuilder? ConfigureWinUI<TApp, TAppWindow>(this IHostBuilder hostBuilder)
        where TApp : Application
        where TAppWindow : Window
    {
        var appType = typeof(TApp);

        hostBuilder?.ConfigureServices((_, serviceCollection) =>
        {
            if (!TryRetrieveWinUIContext(hostBuilder.Properties, out var winUIContext))
            {
                serviceCollection.AddSingleton(winUIContext);
                serviceCollection.AddSingleton(serviceProvider => new WinUIThread(serviceProvider));
                serviceCollection.AddHostedService<WinUIHostedService>();
            }

            winUIContext.AppWindowType = typeof(TAppWindow);
            winUIContext.IsLifetimeLinked = true;
        });

        if (appType != null)
        {
            var baseApplicationType = typeof(Application);
            if (!baseApplicationType.IsAssignableFrom(appType))
            {
                throw new ArgumentException("The registered Application type inherit System.Windows.Application", nameof(TApp));
            }

            hostBuilder?.ConfigureServices((_, serviceCollection) =>
            {
                serviceCollection.AddSingleton<TApp>();

                if (appType != baseApplicationType)
                {
                    serviceCollection.AddSingleton<Application>(services => services.GetRequiredService<TApp>());
                }
            });
        }

        return hostBuilder;
    }

    /// <summary>
    /// Configures WinUI integration for the application by registering the specified application and window types with
    /// the host builder.
    /// </summary>
    /// <remarks>This method registers the WinUI application and window types as singletons in the dependency
    /// injection container and sets up the necessary WinUI hosting services. Call this method before building the host
    /// to ensure proper WinUI initialization.</remarks>
    /// <typeparam name="TApp">The type of the WinUI application to register. Must inherit from <see cref="Application"/>.</typeparam>
    /// <typeparam name="TAppWindow">The type of the main window to use for the application. Must inherit from <see cref="Window"/>.</typeparam>
    /// <param name="hostBuilder">The <see cref="IHostApplicationBuilder"/> to configure for WinUI support.</param>
    /// <returns>The same <see cref="IHostApplicationBuilder"/> instance for chaining further configuration.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="hostBuilder"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown if <typeparamref name="TApp"/> does not inherit from <see cref="Application"/>.</exception>
    public static IHostApplicationBuilder ConfigureWinUI<TApp, TAppWindow>(this IHostApplicationBuilder hostBuilder)
        where TApp : Application
        where TAppWindow : Window
    {
        if (hostBuilder == null)
        {
            throw new ArgumentNullException(nameof(hostBuilder));
        }

        var appType = typeof(TApp);

        if (!TryRetrieveWinUIContext(hostBuilder.Properties, out var winUIContext))
        {
            hostBuilder.Services.AddSingleton(winUIContext);
            hostBuilder.Services.AddSingleton(serviceProvider => new WinUIThread(serviceProvider));
            hostBuilder.Services.AddHostedService<WinUIHostedService>();
        }

        winUIContext.AppWindowType = typeof(TAppWindow);
        winUIContext.IsLifetimeLinked = true;

        var baseApplicationType = typeof(Application);
        if (!baseApplicationType.IsAssignableFrom(appType))
        {
            throw new ArgumentException("The registered Application type inherit System.Windows.Application", nameof(TApp));
        }

        hostBuilder.Services.AddSingleton<TApp>();
        if (appType != baseApplicationType)
        {
            hostBuilder.Services.AddSingleton<Application>(services => services.GetRequiredService<TApp>());
        }

        return hostBuilder;
    }

    /// <summary>
    /// Attempts to retrieve an existing WinUI context from the specified property dictionary.
    /// </summary>
    /// <remarks>If the WinUI context does not exist in the dictionary, this method creates a new instance,
    /// assigns it to the out parameter, and adds it to the dictionary for future retrieval.</remarks>
    /// <param name="properties">The dictionary containing property values, which may include a WinUI context entry.</param>
    /// <param name="winUIContext">When this method returns, contains the WinUI context retrieved from the dictionary if found; otherwise, a new
    /// WinUI context instance.</param>
    /// <returns>true if an existing WinUI context was found in the dictionary; otherwise, false.</returns>
    private static bool TryRetrieveWinUIContext(this IDictionary<object, object> properties, out IWinUIContext winUIContext)
    {
        if (properties.TryGetValue(WinUIContextKey, out var winUIContextAsObject))
        {
            winUIContext = (IWinUIContext)winUIContextAsObject;
            return true;
        }

        winUIContext = new WinUIContext();
        properties[WinUIContextKey] = winUIContext;
        return false;
    }
}
