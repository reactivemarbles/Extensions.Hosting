// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;

namespace ReactiveMarbles.Extensions.Hosting.Maui;

/// <summary>
/// Provides extension methods for configuring pages, applications, and context in a .NET MAUI application using the
/// IMauiBuilder interface.
/// </summary>
/// <remarks>These extension methods simplify the registration and configuration of pages and applications within
/// the MAUI dependency injection and application startup pipeline. They are intended to be used during application
/// initialization to customize the app's composition and behavior.</remarks>
public static class MauiBuilderExtensions
{
    /// <summary>
    /// Registers a page type as a singleton in the MAUI application's dependency injection container.
    /// </summary>
    /// <remarks>Use this method to ensure that only a single instance of the specified page type is created
    /// and used throughout the application's lifetime. This is useful for pages that should maintain state or resources
    /// across the application.</remarks>
    /// <typeparam name="TPage">The type of the page to register. Must inherit from Page.</typeparam>
    /// <param name="mauiBuilder">The IMauiBuilder instance to configure. Cannot be null.</param>
    /// <returns>The same IMauiBuilder instance for method chaining, or null if the input was null.</returns>
    public static IMauiBuilder? AddSingletonPage<TPage>(this IMauiBuilder mauiBuilder)
        where TPage : Page
    {
        mauiBuilder?.PageTypes.Add(typeof(TPage));
        return mauiBuilder;
    }

    /// <summary>
    /// Configures the Maui application to use the specified application type and applies optional additional
    /// configuration to the Maui app builder.
    /// </summary>
    /// <typeparam name="TApplication">The type of the application to use. Must derive from <see cref="Application"/>.</typeparam>
    /// <param name="mauiBuilder">The <see cref="IMauiBuilder"/> instance to configure. Cannot be null.</param>
    /// <param name="configureMauiApp">An optional delegate to further configure the <see cref="MauiAppBuilder"/> before building the application. If
    /// null, no additional configuration is applied.</param>
    /// <returns>The same <see cref="IMauiBuilder"/> instance, configured to use the specified application type.</returns>
    public static IMauiBuilder UseMauiApp<TApplication>(this IMauiBuilder mauiBuilder, Action<MauiAppBuilder>? configureMauiApp = null)
        where TApplication : Application
    {
        ArgumentNullException.ThrowIfNull(mauiBuilder);

        mauiBuilder.ApplicationType = typeof(TApplication);
        mauiBuilder.MauiAppBuilder.UseMauiApp<TApplication>();
        configureMauiApp?.Invoke(mauiBuilder.MauiAppBuilder);
        return mauiBuilder;
    }

    /// <summary>
    /// Configures the Maui application to use the specified application instance and applies optional additional
    /// configuration to the Maui app builder.
    /// </summary>
    /// <typeparam name="TApplication">The type of the application to use. Must derive from Application.</typeparam>
    /// <param name="mauiBuilder">The builder used to configure the Maui application. Cannot be null.</param>
    /// <param name="currentApplication">The application instance to use as the root of the Maui app. Cannot be null.</param>
    /// <param name="configureMauiApp">An optional delegate to further configure the Maui app builder before building the application. May be null.</param>
    /// <returns>The same IMauiBuilder instance for chaining further configuration.</returns>
    public static IMauiBuilder UseMauiApp<TApplication>(this IMauiBuilder mauiBuilder, TApplication currentApplication, Action<MauiAppBuilder>? configureMauiApp = null)
        where TApplication : Application
    {
        ArgumentNullException.ThrowIfNull(mauiBuilder);

        mauiBuilder.ApplicationType = typeof(TApplication);
        mauiBuilder.Application = currentApplication;
        mauiBuilder.MauiAppBuilder.UseMauiApp<TApplication>();
        configureMauiApp?.Invoke(mauiBuilder.MauiAppBuilder);
        return mauiBuilder;
    }

    /// <summary>
    /// Configures the application context by specifying an action to be invoked with the application's <see
    /// cref="IMauiContext"/> during app startup.
    /// </summary>
    /// <remarks>Use this method to customize the application's context, such as registering services or
    /// modifying context-specific settings before the app is fully built. This method supports method
    /// chaining.</remarks>
    /// <param name="mauiBuilder">The <see cref="IMauiBuilder"/> instance to configure.</param>
    /// <param name="configureAction">An action to perform additional configuration on the application's <see cref="IMauiContext"/>. Can be <see
    /// langword="null"/> to clear any existing configuration.</param>
    /// <returns>The <see cref="IMauiBuilder"/> instance for chaining further configuration.</returns>
    public static IMauiBuilder ConfigureContext(this IMauiBuilder mauiBuilder, Action<IMauiContext> configureAction)
    {
        ArgumentNullException.ThrowIfNull(mauiBuilder);

        mauiBuilder.ConfigureContextAction = configureAction;
        return mauiBuilder;
    }
}
