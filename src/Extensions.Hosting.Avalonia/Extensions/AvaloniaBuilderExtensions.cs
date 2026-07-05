// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using Avalonia;
using Avalonia.Controls;

namespace ReactiveMarbles.Extensions.Hosting.Avalonia;

/// <summary>Provides extension methods for configuring the Avalonia application builder.</summary>
/// <remarks>This static class contains methods that allow developers to customize the Avalonia application setup,
/// including registering window types, configuring application instances, and applying custom configuration
/// actions.</remarks>
public static class AvaloniaBuilderExtensions
{
    /// <summary>Provides extension members for this receiver.</summary>
    /// <param name="avaloniaBuilder">The receiver instance.</param>
    extension(IAvaloniaBuilder avaloniaBuilder)
    {
        /// <summary>Registers a window type with the Avalonia application builder for use when creating application windows.</summary>
        /// <remarks>Use this method to configure the application builder to create instances of a custom window
        /// type. This is typically called during application startup to specify the main window or additional window
        /// types.</remarks>
        /// <typeparam name="TWindow">Specifies the window type to register. The type must derive from the Window class.</typeparam>
        /// <returns>The updated Avalonia builder instance, enabling further configuration through method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the avaloniaBuilder parameter is null.</exception>
        public IAvaloniaBuilder UseWindow<TWindow>()
            where TWindow : Window
        {
            if (avaloniaBuilder is null)
            {
                throw new ArgumentNullException(nameof(avaloniaBuilder));
            }

            avaloniaBuilder.WindowTypes.Add(typeof(TWindow));
            return avaloniaBuilder;
        }

        /// <summary>Configures the Avalonia application to use the specified application type.</summary>
        /// <remarks>This method sets the application type for the Avalonia application, allowing the framework to
        /// instantiate the specified application class during startup.</remarks>
        /// <typeparam name="TApplication">The type of the application to be used, which must derive from the Application class.</typeparam>
        /// <returns>The updated IAvaloniaBuilder instance for further configuration.</returns>
        public IAvaloniaBuilder UseApplication<TApplication>()
            where TApplication : Application
        {
            if (avaloniaBuilder is null)
            {
                throw new ArgumentNullException(nameof(avaloniaBuilder));
            }

            avaloniaBuilder.ApplicationType = typeof(TApplication);
            return avaloniaBuilder;
        }

        /// <summary>Configures the specified Avalonia builder to use the provided application instance for managing the application's lifecycle.</summary>
        /// <remarks>This method sets both the application type and the current application instance on the
        /// builder, allowing the builder to manage the application's lifecycle. Use this method when you want to provide an
        /// existing application instance rather than letting the builder create one.</remarks>
        /// <typeparam name="TApplication">Specifies the type of the application to be used. The type must derive from the Application class.</typeparam>
        /// <param name="currentApplication">The application instance to associate with the Avalonia builder.</param>
        /// <returns>The configured IAvaloniaBuilder instance, enabling further method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the avaloniaBuilder parameter is null.</exception>
        public IAvaloniaBuilder UseCurrentApplication<TApplication>(TApplication currentApplication)
            where TApplication : Application
        {
            if (avaloniaBuilder is null)
            {
                throw new ArgumentNullException(nameof(avaloniaBuilder));
            }

            avaloniaBuilder.ApplicationType = typeof(TApplication);
            avaloniaBuilder.Application = currentApplication;
            return avaloniaBuilder;
        }

        /// <summary>Configures the Avalonia application context by applying the specified configuration action to the builder.</summary>
        /// <remarks>Use this method to customize the Avalonia context during application setup by providing a
        /// configuration action.</remarks>
        /// <param name="configureAction">An action that configures the Avalonia context. The action is invoked with the context as its parameter.</param>
        /// <returns>The updated Avalonia builder instance, enabling further configuration through method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="avaloniaBuilder"/> is null.</exception>
        public IAvaloniaBuilder ConfigureContext(Action<IAvaloniaContext> configureAction)
        {
            if (avaloniaBuilder is null)
            {
                throw new ArgumentNullException(nameof(avaloniaBuilder));
            }

            avaloniaBuilder.ConfigureContextAction = configureAction;
            return avaloniaBuilder;
        }

        /// <summary>Configures the application builder with a specified action, allowing for custom setup of the Avalonia application.</summary>
        /// <param name="configureAction">An action that configures the AppBuilder instance. The action is invoked to apply custom configuration logic.</param>
        /// <returns>The configured IAvaloniaBuilder instance, enabling method chaining for further configuration.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="avaloniaBuilder"/> is null.</exception>
        public IAvaloniaBuilder ConfigureAppBuilder(Action<AppBuilder> configureAction)
        {
            if (avaloniaBuilder is null)
            {
                throw new ArgumentNullException(nameof(avaloniaBuilder));
            }

            avaloniaBuilder.ConfigureAppBuilderAction = configureAction;
            return avaloniaBuilder;
        }
    }
}
