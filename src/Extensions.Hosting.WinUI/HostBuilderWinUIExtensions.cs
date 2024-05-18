// Copyright (c) 2019-2024 ReactiveUI Association Incorporated. All rights reserved.
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
/// This contains the WinUI extensions for Microsoft.Extensions.Hosting.
/// </summary>
public static class HostBuilderWinUIExtensions
{
    private const string WinUIContextKey = "WinUIContext";

    /// <summary>
    /// Configure a WinUI application.
    /// </summary>
    /// <typeparam name="TApp">The type of the application.</typeparam>
    /// <typeparam name="TAppWindow">The type of the application window.</typeparam>
    /// <param name="hostBuilder">IHostBuilder.</param>
    /// <returns>A IHostBuilder.</returns>
    /// <exception cref="System.ArgumentException">The registered Application type inherit System.Windows.Application - TApp.</exception>
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
    /// Helper method to retrieve the IWinUIContext.
    /// </summary>
    /// <param name="properties">IDictionary.</param>
    /// <param name="winUIContext">IWinUIContext out value.</param>
    /// <returns>bool if there was already an IWinUIContext.</returns>
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
