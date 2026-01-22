// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;

namespace ReactiveMarbles.Extensions.Hosting.Maui;

/// <summary>
/// Defines a builder interface for configuring and constructing a .NET MAUI application.
/// </summary>
/// <remarks>The IMauiBuilder interface provides properties and configuration hooks for setting up a MAUI
/// application, including specifying the application type, registering pages, and customizing the application context.
/// Implementations of this interface are typically used during application startup to prepare and build the MAUI app
/// instance.</remarks>
public interface IMauiBuilder
{
    /// <summary>
    /// Gets or sets the type of the application associated with this instance.
    /// </summary>
    Type? ApplicationType { get; set; }

    /// <summary>
    /// Gets or sets the application associated with the current context.
    /// </summary>
    Application? Application { get; set; }

    /// <summary>
    /// Gets the collection of page types supported by the current context.
    /// </summary>
    IList<Type> PageTypes { get; }

    /// <summary>
    /// Gets or sets an action that configures the provided <see cref="IMauiContext"/> before it is used.
    /// </summary>
    /// <remarks>Use this property to customize the <see cref="IMauiContext"/> instance, such as registering
    /// services or modifying context-specific settings, prior to its consumption by dependent components. If not set,
    /// the context will be used without additional configuration.</remarks>
    Action<IMauiContext>? ConfigureContextAction { get; set; }

    /// <summary>
    /// Gets the builder used to configure and create the .NET MAUI application instance.
    /// </summary>
    /// <remarks>Use this property to access the underlying builder for registering services, configuring app
    /// settings, or customizing the application startup process before building the final app instance.</remarks>
    MauiAppBuilder MauiAppBuilder { get; }
}
