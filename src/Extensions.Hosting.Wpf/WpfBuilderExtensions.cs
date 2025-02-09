// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Windows;

namespace ReactiveMarbles.Extensions.Hosting.Wpf;

/// <summary>
/// Extension methods to configure Wpf.
/// </summary>
public static class WpfBuilderExtensions
{
    /// <summary>
    /// Register a window, as a singleton.
    /// </summary>
    /// <typeparam name="TWindow">Type of the window, must inherit from Window.</typeparam>
    /// <param name="wpfBuilder">IWpfBuilder.</param>
    /// <returns>A IWpfBuilder.</returns>
    public static IWpfBuilder? UseWindow<TWindow>(this IWpfBuilder wpfBuilder)
        where TWindow : Window
    {
        wpfBuilder?.WindowTypes.Add(typeof(TWindow));
        return wpfBuilder;
    }

    /// <summary>
    /// Register a type for the main window.
    /// </summary>
    /// <typeparam name="TApplication">Type of the application, must inherit from Application.</typeparam>
    /// <param name="wpfBuilder">IWpfBuilder.</param>
    /// <returns>A IWpfBuilder.</returns>
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
    /// Uses the current application.
    /// </summary>
    /// <typeparam name="TApplication">The type of the application.</typeparam>
    /// <param name="wpfBuilder">The WPF builder.</param>
    /// <param name="currentApplication">The current application.</param>
    /// <returns>A IWpfBuilder.</returns>
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
    /// Register action to configure the Application.
    /// </summary>
    /// <param name="wpfBuilder">IWpfBuilder.</param>
    /// <param name="configureAction">Action to configure the Application.</param>
    /// <returns>A IWpfBuilder.</returns>
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
