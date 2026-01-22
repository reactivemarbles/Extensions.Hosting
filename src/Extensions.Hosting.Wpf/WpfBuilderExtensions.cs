// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Windows;

namespace ReactiveMarbles.Extensions.Hosting.Wpf;

/// <summary>
/// Provides extension methods for configuring WPF applications and windows using the IWpfBuilder interface.
/// </summary>
/// <remarks>These extension methods enable fluent registration and configuration of WPF application types, main
/// windows, and context actions within a builder pattern. They are intended to simplify setup and integration of WPF
/// components in applications that use dependency injection or modular configuration.</remarks>
public static class WpfBuilderExtensions
{
    /// <summary>
    /// Registers the specified window type with the WPF builder for use in the application.
    /// </summary>
    /// <typeparam name="TWindow">The type of the window to register. Must derive from <see cref="Window"/>.</typeparam>
    /// <param name="wpfBuilder">The WPF builder instance to configure. Cannot be null.</param>
    /// <returns>The same <see cref="IWpfBuilder"/> instance to allow for method chaining, or <see langword="null"/> if <paramref
    /// name="wpfBuilder"/> is null.</returns>
    public static IWpfBuilder? UseWindow<TWindow>(this IWpfBuilder wpfBuilder)
        where TWindow : Window
    {
        wpfBuilder?.WindowTypes.Add(typeof(TWindow));
        return wpfBuilder;
    }

    /// <summary>
    /// Configures the WPF builder to use the specified application type.
    /// </summary>
    /// <typeparam name="TApplication">The type of the WPF application to use. Must derive from <see cref="Application"/>.</typeparam>
    /// <param name="wpfBuilder">The WPF builder to configure. Cannot be null.</param>
    /// <returns>The same <see cref="IWpfBuilder"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="wpfBuilder"/> is null.</exception>
    public static IWpfBuilder UseApplication<TApplication>(this IWpfBuilder wpfBuilder)
        where TApplication : Application
    {
        if (wpfBuilder is null)
        {
            throw new ArgumentNullException(nameof(wpfBuilder));
        }

        wpfBuilder.ApplicationType = typeof(TApplication);
        return wpfBuilder;
    }

    /// <summary>
    /// Configures the WPF builder to use the specified application instance as the current application.
    /// </summary>
    /// <typeparam name="TApplication">The type of the application to use. Must derive from <see cref="Application"/>.</typeparam>
    /// <param name="wpfBuilder">The WPF builder to configure. Cannot be null.</param>
    /// <param name="currentApplication">The application instance to set as the current application. Must not be null.</param>
    /// <returns>The same <see cref="IWpfBuilder"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="wpfBuilder"/> is null.</exception>
    public static IWpfBuilder UseCurrentApplication<TApplication>(this IWpfBuilder wpfBuilder, TApplication currentApplication)
        where TApplication : Application
    {
        if (wpfBuilder is null)
        {
            throw new ArgumentNullException(nameof(wpfBuilder));
        }

        wpfBuilder.ApplicationType = typeof(TApplication);
        wpfBuilder.Application = currentApplication;
        return wpfBuilder;
    }

    /// <summary>
    /// Configures the WPF context by specifying an action to be executed during context setup.
    /// </summary>
    /// <param name="wpfBuilder">The WPF builder instance to configure. Cannot be null.</param>
    /// <param name="configureAction">An action to perform additional configuration on the WPF context. This action is invoked during context setup
    /// and can be null if no additional configuration is required.</param>
    /// <returns>The same <see cref="IWpfBuilder"/> instance, enabling method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="wpfBuilder"/> is null.</exception>
    public static IWpfBuilder ConfigureContext(this IWpfBuilder wpfBuilder, Action<IWpfContext> configureAction)
    {
        if (wpfBuilder is null)
        {
            throw new ArgumentNullException(nameof(wpfBuilder));
        }

        wpfBuilder.ConfigureContextAction = configureAction;
        return wpfBuilder;
    }
}
