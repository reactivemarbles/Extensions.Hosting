// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;

namespace ReactiveMarbles.Extensions.Hosting.Maui.Internals;

/// <inheritdoc/>
internal sealed class MauiBuilder : IMauiBuilder
{
    /// <summary>Stores the action that applies MAUI application defaults to the underlying builder.</summary>
    private readonly Action<MauiAppBuilder>? _configureMauiApplication;

    /// <summary>Initializes a new instance of the <see cref="MauiBuilder"/> class.</summary>
    internal MauiBuilder()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="MauiBuilder"/> class with a composed MAUI application configurator.</summary>
    /// <param name="configureMauiApplication">The action that applies MAUI application defaults to the underlying builder.</param>
    internal MauiBuilder(Action<MauiAppBuilder>? configureMauiApplication) =>
        _configureMauiApplication = configureMauiApplication;

    /// <inheritdoc/>
    public Type? ApplicationType { get; set; }

    /// <inheritdoc/>
    public Application? Application { get; set; }

    /// <inheritdoc/>
    public Func<IServiceProvider, Application>? ApplicationFactory { get; set; }

    /// <inheritdoc/>
    public IList<Type> PageTypes { get; } = [];

    /// <inheritdoc/>
    public Action<IMauiContext>? ConfigureContextAction { get; set; }

    /// <summary>Gets the maui application builder.</summary>
    /// <value>
    /// The maui application builder.
    /// </value>
    public MauiAppBuilder MauiAppBuilder { get; } = MauiApp.CreateBuilder();

    /// <summary>Applies MAUI application defaults to the underlying builder.</summary>
    /// <typeparam name="TApplication">The application type to configure.</typeparam>
    /// <param name="applicationFactory">The factory that creates the MAUI application.</param>
    /// <returns>The underlying MAUI application builder.</returns>
    internal MauiAppBuilder ApplyMauiApplicationDefaults<TApplication>(Func<IServiceProvider, TApplication> applicationFactory)
        where TApplication : Application
    {
        if (_configureMauiApplication is not null)
        {
            _configureMauiApplication(MauiAppBuilder);
            return MauiAppBuilder;
        }

        return MauiAppBuilder.UseMauiApp(applicationFactory);
    }
}
