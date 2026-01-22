// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
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
/// Provides extension methods for configuring and integrating WPF application lifetime and services with .NET Generic
/// Host builders.
/// </summary>
/// <remarks>These extensions enable seamless integration of WPF applications with the hosting infrastructure,
/// allowing developers to configure WPF services, link application shutdown to host lifetime, and register WPF windows
/// and application types. Methods are provided for both classic and new builder APIs. Ensure that WPF is properly
/// configured before using lifetime integration methods.</remarks>
public static class HostBuilderWpfExtensions
{
    private const string WpfContextKey = nameof(WpfContext);

    /// <summary>
    /// Enables WPF application lifetime integration for the host, configuring shutdown behavior according to the
    /// specified mode.
    /// </summary>
    /// <remarks>This method should be called after configuring WPF on the host builder. It links the host's
    /// lifetime to the WPF application's lifetime, ensuring that the host shuts down according to the specified
    /// shutdown mode.</remarks>
    /// <param name="hostBuilder">The host builder to configure for WPF lifetime integration. Cannot be null.</param>
    /// <param name="shutdownMode">The shutdown mode that determines when the WPF application will exit. The default is
    /// ShutdownMode.OnLastWindowClose.</param>
    /// <returns>The same instance of <see cref="IHostBuilder"/> for chaining further configuration.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="hostBuilder"/> is null.</exception>
    /// <exception cref="NotSupportedException">Thrown if WPF has not been configured on the host builder before calling this method.</exception>
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
    /// Enables WPF-specific application lifetime management for the host, configuring how the application shuts down
    /// based on the specified shutdown mode.
    /// </summary>
    /// <remarks>This method links the application's lifetime to the WPF application's lifetime, allowing the
    /// host to shut down according to the specified shutdown mode. Call this method after configuring WPF services and
    /// before building the host.</remarks>
    /// <param name="hostBuilder">The host application builder to configure for WPF lifetime management. Cannot be null.</param>
    /// <param name="shutdownMode">The shutdown behavior for the WPF application. Defaults to ShutdownMode.OnLastWindowClose.</param>
    /// <returns>The same instance of the host application builder, configured to use WPF lifetime management.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the hostBuilder parameter is null.</exception>
    /// <exception cref="NotSupportedException">Thrown if WPF has not been configured on the host builder before calling this method.</exception>
    public static IHostApplicationBuilder UseWpfLifetime(this IHostApplicationBuilder hostBuilder, ShutdownMode shutdownMode = ShutdownMode.OnLastWindowClose)
    {
        if (hostBuilder == null)
        {
            throw new ArgumentNullException(nameof(hostBuilder));
        }

        if (!TryRetrieveWpfContext(hostBuilder.Properties, out var wpfContext))
        {
            throw new NotSupportedException("Please configure WPF first!");
        }

        wpfContext.ShutdownMode = shutdownMode;
        wpfContext.IsLifetimeLinked = true;
        return hostBuilder;
    }

    /// <summary>
    /// Configures WPF support for the specified host builder, enabling integration of WPF application and window types
    /// into the hosting environment.
    /// </summary>
    /// <remarks>This method registers WPF services and allows customization of the WPF application and main
    /// window types through the <paramref name="configureDelegate"/>. It should be called before building the host to
    /// ensure WPF integration is properly configured. Only one WPF context is registered per host builder
    /// instance.</remarks>
    /// <param name="hostBuilder">The host builder to configure for WPF support. Cannot be null.</param>
    /// <param name="configureDelegate">An optional delegate to further configure WPF-specific services and application or window types. If null,
    /// default WPF configuration is applied.</param>
    /// <returns>The same instance of <see cref="IHostBuilder"/> for chaining further configuration.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="hostBuilder"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the application type registered via <paramref name="configureDelegate"/> does not inherit from <see
    /// cref="System.Windows.Application"/>.</exception>
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
    /// Configures WPF support for the application and registers required WPF services with the host builder.
    /// </summary>
    /// <remarks>This method adds the necessary services to enable WPF integration in a generic host
    /// application. It should be called during application startup before building the host. If an Application type or
    /// main window is specified via the configureDelegate, they are registered as singletons in the service
    /// container.</remarks>
    /// <param name="hostBuilder">The host application builder to configure for WPF support. Cannot be null.</param>
    /// <param name="configureDelegate">An optional delegate to further configure WPF-specific options using an IWpfBuilder instance. If null, default
    /// configuration is applied.</param>
    /// <returns>The same IHostApplicationBuilder instance for chaining further configuration.</returns>
    /// <exception cref="ArgumentNullException">Thrown if hostBuilder is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the Application type registered via configureDelegate does not inherit from
    /// System.Windows.Application.</exception>
    public static IHostApplicationBuilder ConfigureWpf(this IHostApplicationBuilder hostBuilder, Action<IWpfBuilder>? configureDelegate = null)
    {
        if (hostBuilder == null)
        {
            throw new ArgumentNullException(nameof(hostBuilder));
        }

        var wpfBuilder = new WpfBuilder();
        configureDelegate?.Invoke(wpfBuilder);

        if (!TryRetrieveWpfContext(hostBuilder.Properties, out var wpfContext))
        {
            hostBuilder.Services.AddSingleton(wpfContext);
            hostBuilder.Services.AddSingleton(serviceProvider => new WpfThread(serviceProvider));
            hostBuilder.Services.AddHostedService<WpfHostedService>();
        }

        wpfBuilder.ConfigureContextAction?.Invoke(wpfContext);

        if (wpfBuilder.ApplicationType != null)
        {
            // Check if the registered application does inherit System.Windows.Application
            var baseApplicationType = typeof(Application);
            if (!baseApplicationType.IsAssignableFrom(wpfBuilder.ApplicationType))
            {
                throw new ArgumentException("The registered Application type inherit System.Windows.Application", nameof(configureDelegate));
            }

            if (wpfBuilder.Application != null)
            {
                // Add existing Application
                hostBuilder.Services.AddSingleton(wpfBuilder.ApplicationType, wpfBuilder.Application);
            }
            else
            {
                hostBuilder.Services.AddSingleton(wpfBuilder.ApplicationType);
            }

            if (wpfBuilder.ApplicationType != baseApplicationType)
            {
                hostBuilder.Services.AddSingleton(serviceProvider => (Application)serviceProvider.GetRequiredService(wpfBuilder.ApplicationType));
            }
        }

        if (wpfBuilder.WindowTypes.Count > 0)
        {
            foreach (var wpfWindowType in wpfBuilder.WindowTypes)
            {
                hostBuilder.Services.AddSingleton(wpfWindowType);

                // Check if it also implements IWpfShell so we can register it as this
                var shellInterfaceType = typeof(IWpfShell);
                if (shellInterfaceType.IsAssignableFrom(wpfWindowType))
                {
                    hostBuilder.Services.AddSingleton(shellInterfaceType, serviceProvider => serviceProvider.GetRequiredService(wpfWindowType));
                }
            }
        }

        return hostBuilder;
    }

    /// <summary>
    /// Attempts to retrieve an existing IWpfContext instance from the specified properties dictionary, or creates and
    /// stores a new instance if one does not exist.
    /// </summary>
    /// <remarks>If no IWpfContext is present in the dictionary, a new instance is created, added to the
    /// dictionary, and returned via the out parameter. Subsequent calls with the same dictionary will retrieve the same
    /// instance.</remarks>
    /// <param name="properties">The dictionary containing property values, typically used to store and retrieve context-specific data. Cannot be
    /// null.</param>
    /// <param name="wpfContext">When this method returns, contains the IWpfContext instance retrieved from the dictionary if one exists;
    /// otherwise, a newly created IWpfContext instance.</param>
    /// <returns>true if an IWpfContext instance was found in the dictionary; otherwise, false.</returns>
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
