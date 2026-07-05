// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactiveMarbles.Extensions.Hosting.WinForms.Internals;

namespace ReactiveMarbles.Extensions.Hosting.WinForms;

/// <summary>Provides extension methods for configuring and integrating Windows Forms (WinForms) application lifetime and services with .NET Generic Host builders.</summary>
/// <remarks>These extensions enable seamless integration of WinForms applications with the .NET hosting model,
/// allowing developers to configure WinForms services, specify the main form (shell), and control application lifetime
/// in relation to the host. Methods are provided for both IHostBuilder and IHostApplicationBuilder to support different
/// hosting scenarios. Use these extensions to register WinForms components, configure the application context, and
/// ensure proper startup and shutdown coordination between the WinForms UI and the host.</remarks>
public static class HostBuilderWinFormsExtensions
{
    /// <summary>Stores the win forms context key value.</summary>
    private const string WinFormsContextKey = nameof(WinFormsContext);

    /// <summary>Attempts to retrieve an existing Windows Forms context from the specified property dictionary.</summary>
    /// <remarks>If a Windows Forms context does not exist in the dictionary, this method creates a new
    /// context, adds it to the dictionary, and returns it in the out parameter.</remarks>
    /// <param name="properties">The property dictionary used to store the context.</param>
    /// <param name="winFormsContext">When this method returns, contains the retrieved Windows Forms context if found; otherwise, a new context
    /// instance.</param>
    /// <returns>true if an existing Windows Forms context was found in the dictionary; otherwise, false.</returns>
    private static bool TryRetrieveWinFormsContext(IDictionary<object, object> properties, out IWinFormsContext winFormsContext)
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

    /// <summary>Provides extension members for this receiver.</summary>
    /// <param name="hostBuilder">The receiver instance.</param>
    extension(IHostApplicationBuilder hostBuilder)
    {
        /// <summary>Enables WinForms lifetime management for the application, allowing the host to be controlled by the WinForms message loop.</summary>
        /// <remarks>When WinForms lifetime is enabled, the application's lifetime is tied to the WinForms message
        /// loop. This is typically used in WinForms applications to ensure that the host shuts down when the main form
        /// closes.</remarks>
        /// <returns>The same instance of <see cref="IHostApplicationBuilder"/> for chaining further configuration.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="hostBuilder"/> is null.</exception>
        public IHostApplicationBuilder UseWinFormsLifetime()
        {
            if (hostBuilder is null)
            {
                throw new ArgumentNullException(nameof(hostBuilder));
            }

            _ = TryRetrieveWinFormsContext(hostBuilder.Properties, out var winFormsContext);
            winFormsContext.IsLifetimeLinked = true;
            return hostBuilder;
        }

        /// <summary>Configures WinForms support for the specified host application builder and optionally allows additional WinForms-specific configuration.</summary>
        /// <remarks>This method registers the necessary services to enable WinForms integration in a generic host
        /// application. If called multiple times, WinForms services are only registered once. Use the <paramref
        /// name="configureAction"/> parameter to customize the WinForms context as needed.</remarks>
        /// <param name="configureAction">An optional delegate to configure the WinForms context after it is created. If null, no additional configuration
        /// is performed.</param>
        /// <returns>The same instance of <see cref="IHostApplicationBuilder"/> with WinForms services configured.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="hostBuilder"/> is null.</exception>
        public IHostApplicationBuilder ConfigureWinForms(Action<IWinFormsContext>? configureAction = null)
        {
            if (hostBuilder is null)
            {
                throw new ArgumentNullException(nameof(hostBuilder));
            }

            if (!TryRetrieveWinFormsContext(hostBuilder.Properties, out var winFormsContext))
            {
                _ = hostBuilder.Services
                    .AddSingleton(winFormsContext)
                    .AddSingleton(serviceProvider => new WinFormsThread(serviceProvider))
                    .AddHostedService<WinFormsHostedService>();
            }

            configureAction?.Invoke(winFormsContext);
            return hostBuilder;
        }

        /// <summary>Configures WinForms support for the application and registers the specified main form type as a singleton service.</summary>
        /// <remarks>If <typeparamref name="TView"/> also implements <see cref="IWinFormsShell"/>, it is
        /// registered as a singleton service for that interface as well. This enables dependency injection of the main form
        /// and shell interface throughout the application.</remarks>
        /// <typeparam name="TView">The type of the main form to register. Must inherit from <see cref="Form"/>.</typeparam>
        /// <param name="configureAction">An optional delegate to configure additional WinForms services or settings. Can be <see langword="null"/>.</param>
        /// <returns>The same <see cref="IHostApplicationBuilder"/> instance for chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="hostBuilder"/> is <see langword="null"/>.</exception>
        public IHostApplicationBuilder ConfigureWinForms<TView>(Action<IWinFormsContext>? configureAction = null)
            where TView : Form
        {
            if (hostBuilder is null)
            {
                throw new ArgumentNullException(nameof(hostBuilder));
            }

            _ = hostBuilder.ConfigureWinForms(configureAction);

            _ = hostBuilder.Services.AddSingleton<TView>();

            // Check if it also implements IWinFormsShell so we can register it as this
            var viewType = typeof(TView);
            var shellInterfaceType = typeof(IWinFormsShell);
            if (shellInterfaceType.IsAssignableFrom(viewType))
            {
                _ = hostBuilder.Services.AddSingleton(shellInterfaceType, serviceProvider => serviceProvider.GetRequiredService<TView>());
            }

            return hostBuilder;
        }

        /// <summary>Configures the WinForms shell for the application using the specified shell form type.</summary>
        /// <typeparam name="TShell">The type of the main shell form to use for the application. Must inherit from Form and implement IWinFormsShell.</typeparam>
        /// <returns>The same IHostApplicationBuilder instance for chaining further configuration.</returns>
        public IHostApplicationBuilder ConfigureWinFormsShell<TShell>()
            where TShell : Form, IWinFormsShell
            => hostBuilder.ConfigureWinForms<TShell>();
    }

    /// <summary>Provides extension members for this receiver.</summary>
    /// <param name="hostBuilder">The receiver instance.</param>
    extension(IHostBuilder hostBuilder)
    {
        /// <summary>Configures the host to use Windows Forms lifetime management, enabling the application to start and stop in coordination with the WinForms message loop.</summary>
        /// <remarks>This method links the application's lifetime to the Windows Forms message loop, so the host
        /// will start when the message loop begins and stop when it ends. Use this method when building WinForms
        /// applications that require integration with the generic host infrastructure.</remarks>
        /// <returns>The same instance of <see cref="IHostBuilder"/> for chaining further configuration, or null if <paramref
        /// name="hostBuilder"/> is null.</returns>
        public IHostBuilder? UseWinFormsLifetime() =>
            hostBuilder?.ConfigureServices((context, services) =>
            {
                _ = TryRetrieveWinFormsContext(hostBuilder.Properties, out var winFormsContext);
                winFormsContext.IsLifetimeLinked = true;
            });

        /// <summary>Configures WinForms support for the specified host builder, enabling integration of Windows Forms message loop and services into the application's hosting environment.</summary>
        /// <remarks>This method registers the necessary services to run a Windows Forms message loop within a
        /// generic host. Call this method before building the host to ensure WinForms integration is available throughout
        /// the application's lifetime.</remarks>
        /// <param name="configureAction">An optional delegate to further configure the WinForms context. If null, no additional configuration is
        /// performed.</param>
        /// <returns>The same instance of <see cref="IHostBuilder"/> with WinForms services configured, or null if <paramref
        /// name="hostBuilder"/> is null.</returns>
        public IHostBuilder? ConfigureWinForms(Action<IWinFormsContext>? configureAction = null) =>
            hostBuilder?.ConfigureServices((context, serviceCollection) =>
            {
                if (!TryRetrieveWinFormsContext(hostBuilder.Properties, out var winFormsContext))
                {
                    _ = serviceCollection
                        .AddSingleton(winFormsContext)
                        .AddSingleton(serviceProvider => new WinFormsThread(serviceProvider))
                        .AddHostedService<WinFormsHostedService>();
                }

                configureAction?.Invoke(winFormsContext);
            });

        /// <summary>Configures WinForms support for the host builder and registers the specified main view form as a singleton service.</summary>
        /// <remarks>If <typeparamref name="TView"/> also implements <see cref="IWinFormsShell"/>, it is
        /// registered as a singleton service for that interface as well. This method enables dependency injection and
        /// configuration for WinForms applications using the generic host.</remarks>
        /// <typeparam name="TView">The type of the main WinForms form to register. Must inherit from <see cref="Form"/>.</typeparam>
        /// <param name="configureAction">An optional action to further configure the WinForms context. May be <see langword="null"/>.</param>
        /// <returns>The same <see cref="IHostBuilder"/> instance for chaining, or <see langword="null"/> if <paramref
        /// name="hostBuilder"/> is <see langword="null"/>.</returns>
        public IHostBuilder? ConfigureWinForms<TView>(Action<IWinFormsContext>? configureAction = null)
            where TView : Form
            =>
            hostBuilder?
                .ConfigureWinForms(configureAction)?
                .ConfigureServices((context, serviceCollection) =>
                {
                    _ = serviceCollection.AddSingleton<TView>();

                    // Check if it also implements IWinFormsShell so we can register it as this
                    var viewType = typeof(TView);
                    var shellInterfaceType = typeof(IWinFormsShell);
                    if (!shellInterfaceType.IsAssignableFrom(viewType))
                    {
                        return;
                    }

                    _ = serviceCollection.AddSingleton(shellInterfaceType, serviceProvider => serviceProvider.GetRequiredService<TView>());
                });

        /// <summary>Configures the specified host builder to use a WinForms shell of the given type as the application's main window.</summary>
        /// <typeparam name="TShell">The type of the WinForms shell form to use as the application's main window. Must inherit from <see
        /// cref="Form"/> and implement <see cref="IWinFormsShell"/>.</typeparam>
        /// <returns>The same <see cref="IHostBuilder"/> instance for chaining, or <see langword="null"/> if <paramref
        /// name="hostBuilder"/> is null.</returns>
        public IHostBuilder? ConfigureWinFormsShell<TShell>()
            where TShell : Form, IWinFormsShell
            => hostBuilder?.ConfigureWinForms<TShell>();
    }
}
