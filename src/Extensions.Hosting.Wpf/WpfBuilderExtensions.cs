// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Windows;

namespace ReactiveMarbles.Extensions.Hosting.Wpf;

/// <summary>Provides extension methods for configuring WPF applications and windows using the IWpfBuilder interface.</summary>
/// <remarks>These extension methods enable fluent registration and configuration of WPF application types, main
/// windows, and context actions within a builder pattern. They are intended to simplify setup and integration of WPF
/// components in applications that use dependency injection or modular configuration.</remarks>
public static class WpfBuilderExtensions
{
    /// <summary>Provides extension members for this receiver.</summary>
    /// <param name="wpfBuilder">The receiver instance.</param>
    extension(IWpfBuilder wpfBuilder)
    {
        /// <summary>Registers the specified window type with the WPF builder for use in the application.</summary>
        /// <param name="windowType">The type of the window to register. Must derive from <see cref="Window"/>.</param>
        /// <returns>The same <see cref="IWpfBuilder"/> instance to allow for method chaining, or <see langword="null"/> if <paramref
        /// name="wpfBuilder"/> is null.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="windowType"/> does not inherit from <see cref="Window"/>.</exception>
        public IWpfBuilder? UseWindow(Type windowType)
        {
            if (wpfBuilder is null)
            {
                return null;
            }

            ValidateWindowType(windowType);
            wpfBuilder.WindowTypes.Add(windowType);
            return wpfBuilder;
        }

        /// <summary>Configures the WPF builder to use the specified application type.</summary>
        /// <param name="applicationType">The type of the WPF application to use. Must derive from <see cref="Application"/>.</param>
        /// <returns>The same <see cref="IWpfBuilder"/> instance for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="wpfBuilder"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="applicationType"/> does not inherit from <see cref="Application"/>.</exception>
        public IWpfBuilder UseApplication(Type applicationType)
        {
            _ = wpfBuilder ?? throw new ArgumentNullException(nameof(wpfBuilder));
            ValidateApplicationType(applicationType);

            wpfBuilder.ApplicationType = applicationType;
            return wpfBuilder;
        }

        /// <summary>Configures the WPF builder to use the specified application instance as the current application.</summary>
        /// <param name="currentApplication">The application instance to set as the current application. Must not be null.</param>
        /// <returns>The same <see cref="IWpfBuilder"/> instance for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="wpfBuilder"/> is null.</exception>
        public IWpfBuilder UseCurrentApplication(Application currentApplication)
        {
            _ = wpfBuilder ?? throw new ArgumentNullException(nameof(wpfBuilder));
            _ = currentApplication ?? throw new ArgumentNullException(nameof(currentApplication));

            wpfBuilder.ApplicationType = currentApplication.GetType();
            wpfBuilder.Application = currentApplication;
            return wpfBuilder;
        }

        /// <summary>Configures the WPF context by specifying an action to be executed during context setup.</summary>
        /// <param name="configureAction">An action to perform additional configuration on the WPF context. This action is invoked during context setup
        /// and can be null if no additional configuration is required.</param>
        /// <returns>The same <see cref="IWpfBuilder"/> instance, enabling method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="wpfBuilder"/> is null.</exception>
        public IWpfBuilder ConfigureContext(Action<IWpfContext> configureAction)
        {
            _ = wpfBuilder ?? throw new ArgumentNullException(nameof(wpfBuilder));

            wpfBuilder.ConfigureContextAction = configureAction;
            return wpfBuilder;
        }
    }

    /// <summary>Validates that the supplied type can be used as a WPF window.</summary>
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

        throw new ArgumentException("The registered window type must inherit System.Windows.Window.", nameof(windowType));
    }

    /// <summary>Validates that the supplied type can be used as a WPF application.</summary>
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

        throw new ArgumentException("The registered application type must inherit System.Windows.Application.", nameof(applicationType));
    }
}
