// Copyright (c) 2019-2024 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactiveMarbles.Extensions.Hosting.Wpf.Internals;

namespace ReactiveMarbles.Extensions.Hosting.Wpf;

/// <summary>
/// This contains the WPF extensions for Microsoft.Extensions.Hosting.
/// </summary>
public static class HostBuilderWpfExtensions
{
    private const string WpfContextKey = nameof(WpfContext);

    /// <summary>
    /// Defines that stopping the WPF application also stops the host (application).
    /// </summary>
    /// <param name="hostBuilder">IHostBuilder.</param>
    /// <param name="shutdownMode">ShutdownMode default is OnLastWindowClose.</param>
    /// <returns>A IHostBuilder.</returns>
    public static IHostBuilder UseWpfLifetime(this IHostBuilder hostBuilder, ShutdownMode shutdownMode = ShutdownMode.OnLastWindowClose)
    {
        if (hostBuilder == null)
        {
            throw new ArgumentNullException(nameof(hostBuilder));
        }

        return hostBuilder.ConfigureServices((_, __) =>
        {
            if (!TryRetrieveWpfContext(hostBuilder.Properties, out var wpfContext))
            {
                throw new NotSupportedException("Please configure WPF first!");
            }

            wpfContext.ShutdownMode = shutdownMode;
            wpfContext.IsLifetimeLinked = true;
        });
    }

    /// <summary>
    /// Configure an WPF application.
    /// </summary>
    /// <param name="hostBuilder">IHostBuilder.</param>
    /// <param name="configureDelegate">Action to configure Wpf.</param>
    /// <returns>A IHostBuilder.</returns>
    public static IHostBuilder ConfigureWpf(this IHostBuilder hostBuilder, Action<IWpfBuilder>? configureDelegate = null)
    {
        if (hostBuilder == null)
        {
            throw new ArgumentNullException(nameof(hostBuilder));
        }

        var wpfBuilder = new WpfBuilder();
        configureDelegate?.Invoke(wpfBuilder);

        hostBuilder.ConfigureServices((_, serviceCollection) =>
        {
            if (!TryRetrieveWpfContext(hostBuilder.Properties, out var wpfContext))
            {
                serviceCollection.AddSingleton(wpfContext);
                serviceCollection.AddSingleton(serviceProvider => new WpfThread(serviceProvider));
                serviceCollection.AddHostedService<WpfHostedService>();
            }

            wpfBuilder.ConfigureContextAction?.Invoke(wpfContext);
        });

        if (wpfBuilder.ApplicationType != null)
        {
            // Check if the registered application does inherit System.Windows.Application
            var baseApplicationType = typeof(Application);
            if (!baseApplicationType.IsAssignableFrom(wpfBuilder.ApplicationType))
            {
                throw new ArgumentException("The registered Application type inherit System.Windows.Application", nameof(configureDelegate));
            }

            hostBuilder.ConfigureServices((_, serviceCollection) =>
            {
                if (wpfBuilder.Application != null)
                {
                    // Add existing Application
                    serviceCollection.AddSingleton(wpfBuilder.ApplicationType, wpfBuilder.Application);
                }
                else
                {
                    serviceCollection.AddSingleton(wpfBuilder.ApplicationType);
                }

                if (wpfBuilder.ApplicationType != baseApplicationType)
                {
                    serviceCollection.AddSingleton(serviceProvider => (Application)serviceProvider.GetRequiredService(wpfBuilder.ApplicationType));
                }
            });
        }

        if (wpfBuilder.WindowTypes.Count > 0)
        {
            hostBuilder.ConfigureServices((_, serviceCollection) =>
            {
                foreach (var wpfWindowType in wpfBuilder.WindowTypes)
                {
                    serviceCollection.AddSingleton(wpfWindowType);

                    // Check if it also implements IWpfShell so we can register it as this
                    var shellInterfaceType = typeof(IWpfShell);
                    if (shellInterfaceType.IsAssignableFrom(wpfWindowType))
                    {
                        serviceCollection.AddSingleton(shellInterfaceType, serviceProvider => serviceProvider.GetRequiredService(wpfWindowType));
                    }
                }
            });
        }

        return hostBuilder;
    }

    /// <summary>
    /// Helper method to retrieve the IWpfContext.
    /// </summary>
    /// <param name="properties">IDictionary.</param>
    /// <param name="wpfContext">IWpfContext out value.</param>
    /// <returns>bool if there was already an IWpfContext.</returns>
    private static bool TryRetrieveWpfContext(this IDictionary<object, object> properties, out IWpfContext wpfContext)
    {
        if (properties.TryGetValue(WpfContextKey, out var wpfContextAsObject))
        {
            wpfContext = (IWpfContext)wpfContextAsObject;
            return true;
        }

        wpfContext = new WpfContext();
        properties[WpfContextKey] = wpfContext;
        return false;
    }
}
