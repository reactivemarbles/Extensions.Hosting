// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using ReactiveMarbles.Extensions.Hosting.WinUI.Internals;

namespace ReactiveMarbles.Extensions.Hosting.WinUI;

/// <summary>Provides extension methods for configuring WinUI applications with generic host builders.</summary>
/// <remarks>These extensions enable integration of WinUI application and window types into the .NET Generic Host
/// infrastructure, allowing for dependency injection, service registration, and host-managed application lifetimes. Use
/// these methods to set up WinUI applications in a host-based environment, such as when building modern desktop
/// applications with dependency injection and background services.</remarks>
public static class HostBuilderWinUIExtensions
{
    /// <summary>Stores the win uicontext key value.</summary>
    private const string WinUIContextKey = nameof(WinUIContext);

    /// <summary>Attempts to retrieve an existing WinUI context from the specified property dictionary.</summary>
    /// <remarks>If the WinUI context does not exist in the dictionary, this method creates a new instance,
    /// assigns it to the out parameter, and adds it to the dictionary for future retrieval.</remarks>
    /// <param name="properties">The property dictionary used to store the context.</param>
    /// <param name="winUIContext">When this method returns, contains the WinUI context retrieved from the dictionary if found; otherwise, a new
    /// WinUI context instance.</param>
    /// <returns>true if an existing WinUI context was found in the dictionary; otherwise, false.</returns>
    private static bool TryRetrieveWinUIContext(IDictionary<object, object> properties, out IWinUIContext winUIContext)
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

    /// <summary>Registers the core WinUI hosting services.</summary>
    /// <param name="services">The service collection to register services into.</param>
    /// <param name="winUIContext">The WinUI context instance to register.</param>
    private static void RegisterWinUIHostingServices(IServiceCollection services, IWinUIContext winUIContext) =>
        _ = services
            .AddSingleton(winUIContext)
            .AddSingleton(serviceProvider => new WinUIThread(serviceProvider))
            .AddHostedService<WinUIHostedService>();

    /// <summary>Registers the WinUI application type.</summary>
    /// <typeparam name="TApp">The WinUI application type to register.</typeparam>
    /// <param name="services">The service collection to register services into.</param>
    /// <exception cref="ArgumentException">Thrown if the application type does not derive from <see cref="Application"/>.</exception>
    private static void RegisterWinUIApplication<TApp>(IServiceCollection services)
        where TApp : Application
    {
        var appType = typeof(TApp);
        var baseApplicationType = typeof(Application);
        if (!baseApplicationType.IsAssignableFrom(appType))
        {
            throw new ArgumentException("The registered Application type inherit System.Windows.Application", nameof(TApp));
        }

        _ = services.AddSingleton<TApp>();
        if (appType == baseApplicationType)
        {
            return;
        }

        _ = services.AddSingleton<Application>(services => services.GetRequiredService<TApp>());
    }

    /// <summary>Provides extension members for this receiver.</summary>
    /// <param name="hostBuilder">The receiver instance.</param>
    extension(IHostApplicationBuilder hostBuilder)
    {
        /// <summary>Configures WinUI integration for the application by registering the specified application and window types with the host builder.</summary>
        /// <remarks>This method registers the WinUI application and window types as singletons in the dependency
        /// injection container and sets up the necessary WinUI hosting services. Call this method before building the host
        /// to ensure proper WinUI initialization.</remarks>
        /// <typeparam name="TApp">The type of the WinUI application to register. Must inherit from <see cref="Application"/>.</typeparam>
        /// <typeparam name="TAppWindow">The type of the main window to use for the application. Must inherit from <see cref="Window"/>.</typeparam>
        /// <returns>The same <see cref="IHostApplicationBuilder"/> instance for chaining further configuration.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="hostBuilder"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown if <typeparamref name="TApp"/> does not inherit from <see cref="Application"/>.</exception>
        public IHostApplicationBuilder ConfigureWinUI<TApp, TAppWindow>()
            where TApp : Application
            where TAppWindow : Window
        {
            if (hostBuilder is null)
            {
                throw new ArgumentNullException(nameof(hostBuilder));
            }

            var appType = typeof(TApp);

            if (!TryRetrieveWinUIContext(hostBuilder.Properties, out var winUIContext))
            {
                RegisterWinUIHostingServices(hostBuilder.Services, winUIContext);
            }

            winUIContext.AppWindowType = typeof(TAppWindow);
            winUIContext.IsLifetimeLinked = true;

            RegisterWinUIApplication<TApp>(hostBuilder.Services);

            return hostBuilder;
        }
    }

    /// <summary>Provides extension members for this receiver.</summary>
    /// <param name="hostBuilder">The receiver instance.</param>
    extension(IHostBuilder hostBuilder)
    {
        /// <summary>Configures WinUI support for the specified application and main window types within the host builder.</summary>
        /// <remarks>This method registers the WinUI application and main window types with the dependency
        /// injection container and links the application lifetime to the host. Call this method before building the host to
        /// ensure proper WinUI integration.</remarks>
        /// <typeparam name="TApp">The type of the WinUI application to configure. Must inherit from <see cref="Application"/>.</typeparam>
        /// <typeparam name="TAppWindow">The type of the main window for the application. Must inherit from <see cref="Window"/>.</typeparam>
        /// <returns>The same <see cref="IHostBuilder"/> instance for chaining, or <see langword="null"/> if <paramref
        /// name="hostBuilder"/> is null.</returns>
        /// <exception cref="ArgumentException">Thrown if <typeparamref name="TApp"/> does not inherit from <see cref="Application"/>.</exception>
        public IHostBuilder? ConfigureWinUI<TApp, TAppWindow>()
            where TApp : Application
            where TAppWindow : Window
        {
            if (hostBuilder is null)
            {
                return null;
            }

            _ = hostBuilder.ConfigureServices((context, serviceCollection) =>
            {
                if (!TryRetrieveWinUIContext(hostBuilder.Properties, out var winUIContext))
                {
                    RegisterWinUIHostingServices(serviceCollection, winUIContext);
                }

                winUIContext.AppWindowType = typeof(TAppWindow);
                winUIContext.IsLifetimeLinked = true;
            });

            _ = hostBuilder.ConfigureServices((context, serviceCollection) => RegisterWinUIApplication<TApp>(serviceCollection));

            return hostBuilder;
        }
    }
}
