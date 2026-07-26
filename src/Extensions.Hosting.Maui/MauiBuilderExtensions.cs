// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using ReactiveMarbles.Extensions.Hosting.Maui.Internals;

namespace ReactiveMarbles.Extensions.Hosting.Maui;

/// <summary>Provides extension methods for configuring pages, applications, and context in a .NET MAUI application using the IMauiBuilder interface.</summary>
/// <remarks>These extension methods simplify the registration and configuration of pages and applications within
/// the MAUI dependency injection and application startup pipeline. They are intended to be used during application
/// initialization to customize the app's composition and behavior.</remarks>
public static class MauiBuilderExtensions
{
    /// <summary>Provides extension members for this receiver.</summary>
    /// <param name="mauiBuilder">The receiver instance.</param>
    extension(IMauiBuilder mauiBuilder)
    {
        /// <summary>Registers a page type as a singleton in the MAUI application's dependency injection container.</summary>
        /// <remarks>Use this method to ensure that only a single instance of the specified page type is created
        /// and used throughout the application's lifetime. This is useful for pages that should maintain state or resources
        /// across the application.</remarks>
        /// <typeparam name="TPage">The type of the page to register. Must inherit from Page.</typeparam>
        /// <returns>The same IMauiBuilder instance for method chaining, or null if the input was null.</returns>
        public IMauiBuilder? AddSingletonPage<TPage>()
            where TPage : Page
        {
            mauiBuilder?.PageTypes.Add(typeof(TPage));
            return mauiBuilder;
        }

        /// <summary>Configures the Maui application to use the specified application type and applies optional additional configuration to the Maui app builder.</summary>
        /// <typeparam name="TApplication">The type of the application to use. Must derive from <see cref="Application"/>.</typeparam>
        /// <returns>The same <see cref="IMauiBuilder"/> instance, configured to use the specified application type.</returns>
        public IMauiBuilder UseMauiApp<TApplication>()
            where TApplication : Application =>
            mauiBuilder.UseMauiApp<TApplication>((Action<MauiAppBuilder>?)null);

        /// <summary>Configures the MAUI application type and applies additional builder configuration.</summary>
        /// <typeparam name="TApplication">The type of the application to use. Must derive from <see cref="Application"/>.</typeparam>
        /// <param name="configureMauiApp">A delegate to further configure the <see cref="MauiAppBuilder"/>.</param>
        /// <returns>The same <see cref="IMauiBuilder"/> instance, configured to use the specified application type.</returns>
        public IMauiBuilder UseMauiApp<TApplication>(Action<MauiAppBuilder>? configureMauiApp)
            where TApplication : Application
        {
            _ = mauiBuilder ?? throw new ArgumentNullException(nameof(mauiBuilder));

            mauiBuilder.ApplicationType = typeof(TApplication);
            if (mauiBuilder is MauiBuilder builder)
            {
                _ = builder.UseMauiApp<TApplication>();
            }
            else
            {
                _ = mauiBuilder.MauiAppBuilder.UseMauiApp<TApplication>();
            }

            configureMauiApp?.Invoke(mauiBuilder.MauiAppBuilder);
            return mauiBuilder;
        }

        /// <summary>Configures the Maui application to use the specified application instance and applies optional additional configuration to the Maui app builder.</summary>
        /// <typeparam name="TApplication">The type of the application to use. Must derive from Application.</typeparam>
        /// <param name="currentApplication">The application instance to use as the root of the Maui app. Cannot be null.</param>
        /// <returns>The same IMauiBuilder instance for chaining further configuration.</returns>
        public IMauiBuilder UseMauiApp<TApplication>(TApplication currentApplication)
            where TApplication : Application =>
            mauiBuilder.UseMauiApp(currentApplication, null);

        /// <summary>Configures the MAUI application instance and applies additional builder configuration.</summary>
        /// <typeparam name="TApplication">The type of the application to use. Must derive from Application.</typeparam>
        /// <param name="currentApplication">The application instance to use as the root of the MAUI app. Cannot be null.</param>
        /// <param name="configureMauiApp">A delegate to further configure the Maui app builder.</param>
        /// <returns>The same IMauiBuilder instance for chaining further configuration.</returns>
        public IMauiBuilder UseMauiApp<TApplication>(
            TApplication currentApplication,
            Action<MauiAppBuilder>? configureMauiApp)
            where TApplication : Application
        {
            _ = mauiBuilder ?? throw new ArgumentNullException(nameof(mauiBuilder));

            mauiBuilder.ApplicationType = typeof(TApplication);
            mauiBuilder.Application = currentApplication;
            if (mauiBuilder is MauiBuilder builder)
            {
                _ = builder.UseMauiApp<TApplication>();
            }
            else
            {
                _ = mauiBuilder.MauiAppBuilder.UseMauiApp<TApplication>();
            }

            configureMauiApp?.Invoke(mauiBuilder.MauiAppBuilder);
            return mauiBuilder;
        }

        /// <summary>Configures the application context by specifying an action to be invoked with the application's <see cref="IMauiContext"/> during app startup.</summary>
        /// <remarks>Use this method to customize the application's context, such as registering services or
        /// modifying context-specific settings before the app is fully built. This method supports method
        /// chaining.</remarks>
        /// <param name="configureAction">An action to perform additional configuration on the application's <see cref="IMauiContext"/>. Can be <see
        /// langword="null"/> to clear any existing configuration.</param>
        /// <returns>The <see cref="IMauiBuilder"/> instance for chaining further configuration.</returns>
        public IMauiBuilder ConfigureContext(Action<IMauiContext> configureAction)
        {
            _ = mauiBuilder ?? throw new ArgumentNullException(nameof(mauiBuilder));

            mauiBuilder.ConfigureContextAction = configureAction;
            return mauiBuilder;
        }
    }
}
