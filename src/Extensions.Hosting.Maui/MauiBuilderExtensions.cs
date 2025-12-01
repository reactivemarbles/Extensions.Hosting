// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;

namespace ReactiveMarbles.Extensions.Hosting.Maui;

/// <summary>
/// Extension methods to configure Maui.
/// </summary>
public static class MauiBuilderExtensions
{
    /// <summary>
    /// Register a page, as a singleton.
    /// </summary>
    /// <typeparam name="TPage">Type of the page, must inherit from Page.</typeparam>
    /// <param name="mauiBuilder">IMauiBuilder.</param>
    /// <returns>A IMauiBuilder.</returns>
    public static IMauiBuilder? AddSingletonPage<TPage>(this IMauiBuilder mauiBuilder)
        where TPage : Page
    {
        mauiBuilder?.PageTypes.Add(typeof(TPage));
        return mauiBuilder;
    }

    /// <summary>
    /// Register a type for the main application.
    /// </summary>
    /// <typeparam name="TApplication">Type of the application, must inherit from Application.</typeparam>
    /// <param name="mauiBuilder">IMauiBuilder.</param>
    /// <param name="configureMauiApp">Configure action for the MauiAppBuilder.</param>
    /// <returns>
    /// A IMauiBuilder.
    /// </returns>
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
    /// Uses the current application.
    /// </summary>
    /// <typeparam name="TApplication">The type of the application.</typeparam>
    /// <param name="mauiBuilder">The MAUI builder.</param>
    /// <param name="currentApplication">The current application.</param>
    /// <param name="configureMauiApp">The configure maui application.</param>
    /// <returns>
    /// A IMauiBuilder.
    /// </returns>
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
    /// Register action to configure the Application.
    /// </summary>
    /// <param name="mauiBuilder">IMauiBuilder.</param>
    /// <param name="configureAction">Action to configure the Application.</param>
    /// <returns>A IMauiBuilder.</returns>
    public static IMauiBuilder ConfigureContext(this IMauiBuilder mauiBuilder, Action<IMauiContext> configureAction)
    {
        ArgumentNullException.ThrowIfNull(mauiBuilder);

        mauiBuilder.ConfigureContextAction = configureAction;
        return mauiBuilder;
    }
}
