// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactiveMarbles.Extensions.Hosting.WinForms.Internals;

namespace ReactiveMarbles.Extensions.Hosting.WinForms;

/// <summary>Provides extension methods for integrating Windows Forms applications with .NET Generic Host builders.</summary>
/// <remarks>
/// These extensions register WinForms services, configure the main form, and coordinate the UI lifetime with the host.
/// </remarks>
public static class HostBuilderWinFormsExtensions
{
    /// <summary>Stores the WinForms context key.</summary>
    private const string WinFormsContextKey = nameof(WinFormsContext);

    /// <summary>Attempts to retrieve an existing Windows Forms context from the specified property dictionary.</summary>
    /// <param name="properties">The property dictionary used to store the context.</param>
    /// <param name="winFormsContext">The existing or newly created context.</param>
    /// <returns><see langword="true"/> when an existing context was found; otherwise, <see langword="false"/>.</returns>
    private static bool TryRetrieveWinFormsContext(
        IDictionary<object, object> properties,
        out IWinFormsContext winFormsContext)
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
        /// <summary>Enables WinForms lifetime management for the application.</summary>
        /// <returns>The same host application builder.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="hostBuilder"/> is null.</exception>
        public IHostApplicationBuilder UseWinFormsLifetime()
        {
            _ = hostBuilder ?? throw new ArgumentNullException(nameof(hostBuilder));

            _ = TryRetrieveWinFormsContext(hostBuilder.Properties, out var winFormsContext);
            winFormsContext.IsLifetimeLinked = true;
            return hostBuilder;
        }

        /// <summary>Configures WinForms support without additional context configuration.</summary>
        /// <returns>The same host application builder.</returns>
        public IHostApplicationBuilder ConfigureWinForms() => hostBuilder.ConfigureWinForms(null);

        /// <summary>Configures WinForms support and applies the supplied context configuration.</summary>
        /// <param name="configureAction">The optional context configuration.</param>
        /// <returns>The same host application builder.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="hostBuilder"/> is null.</exception>
        public IHostApplicationBuilder ConfigureWinForms(Action<IWinFormsContext>? configureAction)
        {
            _ = hostBuilder ?? throw new ArgumentNullException(nameof(hostBuilder));

            if (!TryRetrieveWinFormsContext(hostBuilder.Properties, out var winFormsContext))
            {
                _ = hostBuilder.Services
                    .AddSingleton(winFormsContext)
                    .AddSingleton(static serviceProvider => new WinFormsThread(serviceProvider))
                    .AddHostedService<WinFormsHostedService>();
            }

            configureAction?.Invoke(winFormsContext);
            return hostBuilder;
        }

        /// <summary>Configures WinForms and registers the specified main form type.</summary>
        /// <typeparam name="TView">The main form type.</typeparam>
        /// <returns>The same host application builder.</returns>
        public IHostApplicationBuilder ConfigureWinForms<TView>()
            where TView : Form =>
            hostBuilder.ConfigureWinForms<TView>(null);

        /// <summary>Configures WinForms, registers the specified main form type, and configures the context.</summary>
        /// <typeparam name="TView">The main form type.</typeparam>
        /// <param name="configureAction">The optional context configuration.</param>
        /// <returns>The same host application builder.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="hostBuilder"/> is null.</exception>
        public IHostApplicationBuilder ConfigureWinForms<TView>(Action<IWinFormsContext>? configureAction)
            where TView : Form
        {
            _ = hostBuilder ?? throw new ArgumentNullException(nameof(hostBuilder));
            _ = hostBuilder.ConfigureWinForms(configureAction);
            _ = hostBuilder.Services.AddSingleton<TView>();

            var viewType = typeof(TView);
            var shellInterfaceType = typeof(IWinFormsShell);
            if (shellInterfaceType.IsAssignableFrom(viewType))
            {
                _ = hostBuilder.Services.AddSingleton(
                    shellInterfaceType,
                    static serviceProvider => serviceProvider.GetRequiredService<TView>());
            }

            return hostBuilder;
        }

        /// <summary>Configures the specified WinForms shell as the main form.</summary>
        /// <typeparam name="TShell">The shell form type.</typeparam>
        /// <returns>The same host application builder.</returns>
        public IHostApplicationBuilder ConfigureWinFormsShell<TShell>()
            where TShell : Form, IWinFormsShell =>
            hostBuilder.ConfigureWinForms<TShell>();
    }

    /// <summary>Provides extension members for this receiver.</summary>
    /// <param name="hostBuilder">The receiver instance.</param>
    extension(IHostBuilder hostBuilder)
    {
        /// <summary>Enables WinForms lifetime management for the host.</summary>
        /// <returns>The configured host builder, or null when the receiver is null.</returns>
        public IHostBuilder? UseWinFormsLifetime() =>
            hostBuilder?.ConfigureServices((_, _) =>
            {
                _ = TryRetrieveWinFormsContext(hostBuilder.Properties, out var winFormsContext);
                winFormsContext.IsLifetimeLinked = true;
            });

        /// <summary>Configures WinForms support without additional context configuration.</summary>
        /// <returns>The configured host builder, or null when the receiver is null.</returns>
        public IHostBuilder? ConfigureWinForms() => hostBuilder?.ConfigureWinForms(null);

        /// <summary>Configures WinForms support and applies the supplied context configuration.</summary>
        /// <param name="configureAction">The optional context configuration.</param>
        /// <returns>The configured host builder, or null when the receiver is null.</returns>
        public IHostBuilder? ConfigureWinForms(Action<IWinFormsContext>? configureAction) =>
            hostBuilder?.ConfigureServices((hostBuilderContext, serviceCollection) =>
            {
                if (!TryRetrieveWinFormsContext(hostBuilder.Properties, out var winFormsContext))
                {
                    _ = serviceCollection
                        .AddSingleton(winFormsContext)
                        .AddSingleton(static serviceProvider => new WinFormsThread(serviceProvider))
                        .AddHostedService<WinFormsHostedService>();
                }

                configureAction?.Invoke(winFormsContext);
            });

        /// <summary>Configures WinForms and registers the specified main form type.</summary>
        /// <typeparam name="TView">The main form type.</typeparam>
        /// <returns>The configured host builder, or null when the receiver is null.</returns>
        public IHostBuilder? ConfigureWinForms<TView>()
            where TView : Form =>
            hostBuilder?.ConfigureWinForms<TView>(null);

        /// <summary>Configures WinForms, registers the specified main form type, and configures the context.</summary>
        /// <typeparam name="TView">The main form type.</typeparam>
        /// <param name="configureAction">The optional context configuration.</param>
        /// <returns>The configured host builder, or null when the receiver is null.</returns>
        public IHostBuilder? ConfigureWinForms<TView>(Action<IWinFormsContext>? configureAction)
            where TView : Form =>
            hostBuilder?
                .ConfigureWinForms(configureAction)?
                .ConfigureServices(static (hostBuilderContext, serviceCollection) =>
                {
                    _ = serviceCollection.AddSingleton<TView>();

                    var viewType = typeof(TView);
                    var shellInterfaceType = typeof(IWinFormsShell);
                    if (!shellInterfaceType.IsAssignableFrom(viewType))
                    {
                        return;
                    }

                    _ = serviceCollection.AddSingleton(
                        shellInterfaceType,
                        static serviceProvider => serviceProvider.GetRequiredService<TView>());
                });

        /// <summary>Configures the specified WinForms shell as the main form.</summary>
        /// <typeparam name="TShell">The shell form type.</typeparam>
        /// <returns>The configured host builder, or null when the receiver is null.</returns>
        public IHostBuilder? ConfigureWinFormsShell<TShell>()
            where TShell : Form, IWinFormsShell =>
            hostBuilder?.ConfigureWinForms<TShell>();
    }
}
