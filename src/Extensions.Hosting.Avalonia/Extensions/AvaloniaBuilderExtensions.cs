// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
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
        /// <param name="windowType">Specifies the window type to register. The type must derive from the Window class.</param>
        /// <returns>The updated Avalonia builder instance, enabling further configuration through method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the avaloniaBuilder parameter is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="windowType"/> does not inherit from <see cref="Window"/>.</exception>
        public IAvaloniaBuilder UseWindow(Type windowType)
        {
            _ = avaloniaBuilder ?? throw new ArgumentNullException(nameof(avaloniaBuilder));
            ValidateWindowType(windowType);

            avaloniaBuilder.WindowTypes.Add(windowType);
            return avaloniaBuilder;
        }

        /// <summary>Configures the Avalonia application to use the specified application type.</summary>
        /// <remarks>This method sets the application type for the Avalonia application, allowing the framework to
        /// instantiate the specified application class during startup.</remarks>
        /// <param name="applicationType">The type of the application to be used, which must derive from the Application class.</param>
        /// <returns>The updated IAvaloniaBuilder instance for further configuration.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the avaloniaBuilder parameter is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="applicationType"/> does not inherit from <see cref="Application"/>.</exception>
        public IAvaloniaBuilder UseApplication(Type applicationType)
        {
            _ = avaloniaBuilder ?? throw new ArgumentNullException(nameof(avaloniaBuilder));
            ValidateApplicationType(applicationType);

            avaloniaBuilder.ApplicationType = applicationType;
            return avaloniaBuilder;
        }

        /// <summary>Configures the specified Avalonia builder to use the provided application instance for managing the application's lifecycle.</summary>
        /// <remarks>This method sets both the application type and the current application instance on the
        /// builder, allowing the builder to manage the application's lifecycle. Use this method when you want to provide an
        /// existing application instance rather than letting the builder create one.</remarks>
        /// <param name="currentApplication">The application instance to associate with the Avalonia builder.</param>
        /// <returns>The configured IAvaloniaBuilder instance, enabling further method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the avaloniaBuilder parameter is null.</exception>
        public IAvaloniaBuilder UseCurrentApplication(Application currentApplication)
        {
            _ = avaloniaBuilder ?? throw new ArgumentNullException(nameof(avaloniaBuilder));
            _ = currentApplication ?? throw new ArgumentNullException(nameof(currentApplication));

            avaloniaBuilder.ApplicationType = currentApplication.GetType();
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
            _ = avaloniaBuilder ?? throw new ArgumentNullException(nameof(avaloniaBuilder));

            avaloniaBuilder.ConfigureContextAction = configureAction;
            return avaloniaBuilder;
        }

        /// <summary>Configures the application builder with a specified action, allowing for custom setup of the Avalonia application.</summary>
        /// <param name="configureAction">An action that configures the AppBuilder instance. The action is invoked to apply custom configuration logic.</param>
        /// <returns>The configured IAvaloniaBuilder instance, enabling method chaining for further configuration.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="avaloniaBuilder"/> is null.</exception>
        public IAvaloniaBuilder ConfigureAppBuilder(Action<AppBuilder> configureAction)
        {
            _ = avaloniaBuilder ?? throw new ArgumentNullException(nameof(avaloniaBuilder));

            avaloniaBuilder.ConfigureAppBuilderAction = configureAction;
            return avaloniaBuilder;
        }
    }

    /// <summary>Validates that the supplied type can be used as an Avalonia window.</summary>
    /// <param name="windowType">The window type to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="windowType"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="windowType"/> does not derive from <see cref="Window"/>.</exception>
    private static void ValidateWindowType(Type windowType)
    {
        _ = windowType ?? throw new ArgumentNullException(nameof(windowType));
        if (typeof(Window).IsAssignableFrom(windowType))
        {
            return;
        }

        throw new ArgumentException("The registered window type must inherit Avalonia.Controls.Window.", nameof(windowType));
    }

    /// <summary>Validates that the supplied type can be used as an Avalonia application.</summary>
    /// <param name="applicationType">The application type to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="applicationType"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="applicationType"/> does not derive from <see cref="Application"/>.</exception>
    private static void ValidateApplicationType(Type applicationType)
    {
        _ = applicationType ?? throw new ArgumentNullException(nameof(applicationType));
        if (typeof(Application).IsAssignableFrom(applicationType))
        {
            return;
        }

        throw new ArgumentException("The registered application type must inherit Avalonia.Application.", nameof(applicationType));
    }
}
