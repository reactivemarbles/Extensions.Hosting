// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactiveMarbles.Extensions.Hosting.Wpf.Internals;

namespace ReactiveMarbles.Extensions.Hosting.Wpf;

/// <summary>Provides extension methods for configuring and integrating WPF application lifetime and services with .NET Generic Host builders.</summary>
/// <remarks>These extensions enable seamless integration of WPF applications with the hosting infrastructure,
/// allowing developers to configure WPF services, link application shutdown to host lifetime, and register WPF windows
/// and application types. Methods are provided for both classic and new builder APIs. Ensure that WPF is properly
/// configured before using lifetime integration methods.</remarks>
public static class HostBuilderWpfExtensions
{
    /// <summary>Stores the wpf context key value.</summary>
    private const string WpfContextKey = nameof(WpfContext);

    /// <summary>Attempts to retrieve an existing IWpfContext instance from the specified properties dictionary, or creates and stores a new instance if one does not exist.</summary>
    /// <remarks>If no IWpfContext is present in the dictionary, a new instance is created, added to the
    /// dictionary, and returned via the out parameter. Subsequent calls with the same dictionary will retrieve the same
    /// instance.</remarks>
    /// <param name="properties">The property dictionary used to store the context.</param>
    /// <param name="wpfContext">When this method returns, contains the IWpfContext instance retrieved from the dictionary if one exists;
    /// otherwise, a newly created IWpfContext instance.</param>
    /// <returns>true if an IWpfContext instance was found in the dictionary; otherwise, false.</returns>
    private static bool TryRetrieveWpfContext(IDictionary<object, object> properties, out IWpfContext wpfContext)
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

    /// <summary>Registers the core WPF hosting services.</summary>
    /// <param name="services">The service collection to register services into.</param>
    /// <param name="wpfContext">The WPF context instance to register.</param>
    private static void RegisterWpfHostingServices(IServiceCollection services, IWpfContext wpfContext) =>
        _ = services
            .AddSingleton(wpfContext)
            .AddSingleton(static serviceProvider => new WpfThread(serviceProvider))
            .AddHostedService<WpfHostedService>();

    /// <summary>Registers a configured WPF application type or instance.</summary>
    /// <param name="services">The service collection to register services into.</param>
    /// <param name="wpfBuilder">The builder that contains the application configuration.</param>
    /// <param name="parameterName">The name of the public parameter used for exception reporting.</param>
    /// <exception cref="ArgumentException">Thrown if the configured application type does not derive from <see cref="Application"/>.</exception>
    private static void RegisterWpfApplication(IServiceCollection services, WpfBuilder wpfBuilder, string parameterName)
    {
        if (wpfBuilder.ApplicationType is null)
        {
            return;
        }

        var baseApplicationType = typeof(Application);
        if (!baseApplicationType.IsAssignableFrom(wpfBuilder.ApplicationType))
        {
            throw new ArgumentException("The registered Application type inherit System.Windows.Application", parameterName);
        }

        if (wpfBuilder.Application is not null)
        {
            _ = services.AddSingleton(wpfBuilder.ApplicationType, wpfBuilder.Application);
        }
        else
        {
            _ = services.AddSingleton(wpfBuilder.ApplicationType);
        }

        if (wpfBuilder.ApplicationType == baseApplicationType)
        {
            return;
        }

        _ = services.AddSingleton(serviceProvider => (Application)serviceProvider.GetRequiredService(wpfBuilder.ApplicationType));
    }

    /// <summary>Registers configured WPF shell windows.</summary>
    /// <param name="services">The service collection to register services into.</param>
    /// <param name="wpfBuilder">The builder that contains the window configuration.</param>
    private static void RegisterWpfWindows(IServiceCollection services, WpfBuilder wpfBuilder)
    {
        foreach (var wpfWindowType in wpfBuilder.WindowTypes)
        {
            _ = services.AddSingleton(wpfWindowType);

            var shellInterfaceType = typeof(IWpfShell);
            if (!shellInterfaceType.IsAssignableFrom(wpfWindowType))
            {
                continue;
            }

            _ = services.AddSingleton(shellInterfaceType, serviceProvider => serviceProvider.GetRequiredService(wpfWindowType));
        }
    }

    /// <summary>Provides extension members for this receiver.</summary>
    /// <param name="hostBuilder">The receiver instance.</param>
    extension(IHostApplicationBuilder hostBuilder)
    {
        /// <summary>Enables WPF lifetime management using <see cref="ShutdownMode.OnLastWindowClose"/>.</summary>
        /// <returns>The same host application builder.</returns>
        public IHostApplicationBuilder UseWpfLifetime() =>
            hostBuilder.UseWpfLifetime(ShutdownMode.OnLastWindowClose);

        /// <summary>Enables WPF-specific application lifetime management for the host, configuring how the application shuts down based on the specified shutdown mode.</summary>
        /// <remarks>This method links the application's lifetime to the WPF application's lifetime, allowing the
        /// host to shut down according to the specified shutdown mode. Call this method after configuring WPF services and
        /// before building the host.</remarks>
        /// <param name="shutdownMode">The shutdown behavior for the WPF application. Defaults to ShutdownMode.OnLastWindowClose.</param>
        /// <returns>The same instance of the host application builder, configured to use WPF lifetime management.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the hostBuilder parameter is null.</exception>
        /// <exception cref="NotSupportedException">Thrown if WPF has not been configured on the host builder before calling this method.</exception>
        public IHostApplicationBuilder UseWpfLifetime(ShutdownMode shutdownMode)
        {
            _ = hostBuilder ?? throw new ArgumentNullException(nameof(hostBuilder));

            if (!TryRetrieveWpfContext(hostBuilder.Properties, out var wpfContext))
            {
                throw new NotSupportedException("Please configure WPF first!");
            }

            wpfContext.ShutdownMode = shutdownMode;
            wpfContext.IsLifetimeLinked = true;
            return hostBuilder;
        }

        /// <summary>Configures WPF support using the default builder settings.</summary>
        /// <returns>The same host application builder.</returns>
        public IHostApplicationBuilder ConfigureWpf() => hostBuilder.ConfigureWpf(null);

        /// <summary>Configures WPF support and applies the supplied builder configuration.</summary>
        /// <param name="configureDelegate">The optional WPF builder configuration.</param>
        /// <returns>The same host application builder.</returns>
        public IHostApplicationBuilder ConfigureWpf(Action<IWpfBuilder>? configureDelegate)
        {
            _ = hostBuilder ?? throw new ArgumentNullException(nameof(hostBuilder));

            var wpfBuilder = new WpfBuilder();
            configureDelegate?.Invoke(wpfBuilder);

            if (!TryRetrieveWpfContext(hostBuilder.Properties, out var wpfContext))
            {
                RegisterWpfHostingServices(hostBuilder.Services, wpfContext);
            }

            wpfBuilder.ConfigureContextAction?.Invoke(wpfContext);
            RegisterWpfApplication(hostBuilder.Services, wpfBuilder, nameof(configureDelegate));
            RegisterWpfWindows(hostBuilder.Services, wpfBuilder);

            return hostBuilder;
        }
    }

    /// <summary>Provides extension members for this receiver.</summary>
    /// <param name="hostBuilder">The receiver instance.</param>
    extension(IHostBuilder hostBuilder)
    {
        /// <summary>Enables WPF lifetime management using <see cref="ShutdownMode.OnLastWindowClose"/>.</summary>
        /// <returns>The same host builder.</returns>
        public IHostBuilder UseWpfLifetime() =>
            hostBuilder.UseWpfLifetime(ShutdownMode.OnLastWindowClose);

        /// <summary>Enables WPF application lifetime integration for the host, configuring shutdown behavior according to the specified mode.</summary>
        /// <remarks>This method should be called after configuring WPF on the host builder. It links the host's
        /// lifetime to the WPF application's lifetime, ensuring that the host shuts down according to the specified
        /// shutdown mode.</remarks>
        /// <param name="shutdownMode">The shutdown mode that determines when the WPF application will exit. The default is
        /// ShutdownMode.OnLastWindowClose.</param>
        /// <returns>The same instance of <see cref="IHostBuilder"/> for chaining further configuration.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="hostBuilder"/> is null.</exception>
        /// <exception cref="NotSupportedException">Thrown if WPF has not been configured on the host builder before calling this method.</exception>
        public IHostBuilder UseWpfLifetime(ShutdownMode shutdownMode)
        {
            _ = hostBuilder ?? throw new ArgumentNullException(nameof(hostBuilder));

            return hostBuilder.ConfigureServices((context, services) =>
            {
                if (!TryRetrieveWpfContext(hostBuilder.Properties, out var wpfContext))
                {
                    throw new NotSupportedException("Please configure WPF first!");
                }

                wpfContext.ShutdownMode = shutdownMode;
                wpfContext.IsLifetimeLinked = true;
            });
        }

        /// <summary>Configures WPF support using the default builder settings.</summary>
        /// <returns>The same host builder.</returns>
        public IHostBuilder ConfigureWpf() => hostBuilder.ConfigureWpf(null);

        /// <summary>Configures WPF support and applies the supplied builder configuration.</summary>
        /// <param name="configureDelegate">The optional WPF builder configuration.</param>
        /// <returns>The same host builder.</returns>
        public IHostBuilder ConfigureWpf(Action<IWpfBuilder>? configureDelegate)
        {
            _ = hostBuilder ?? throw new ArgumentNullException(nameof(hostBuilder));

            var wpfBuilder = new WpfBuilder();
            configureDelegate?.Invoke(wpfBuilder);

            _ = hostBuilder.ConfigureServices((context, serviceCollection) =>
            {
                if (!TryRetrieveWpfContext(hostBuilder.Properties, out var wpfContext))
                {
                    RegisterWpfHostingServices(serviceCollection, wpfContext);
                }

                wpfBuilder.ConfigureContextAction?.Invoke(wpfContext);
            });

            if (wpfBuilder.ApplicationType is not null)
            {
                _ = hostBuilder.ConfigureServices((context, serviceCollection) => RegisterWpfApplication(serviceCollection, wpfBuilder, nameof(configureDelegate)));
            }

            if (wpfBuilder.WindowTypes.Count > 0)
            {
                _ = hostBuilder.ConfigureServices((context, serviceCollection) => RegisterWpfWindows(serviceCollection, wpfBuilder));
            }

            return hostBuilder;
        }
    }
}
