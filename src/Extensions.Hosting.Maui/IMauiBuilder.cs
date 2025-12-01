// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;

namespace ReactiveMarbles.Extensions.Hosting.Maui;

/// <summary>
/// Interface used for configuring Maui.
/// </summary>
public interface IMauiBuilder
{
    /// <summary>
    /// Gets or sets type of the application that will be used.
    /// </summary>
    Type? ApplicationType { get; set; }
    /// <summary>
    /// Gets or sets an existing application.
    /// </summary>
    Application? Application { get; set; }
    /// <summary>
    /// Gets type of the pages that will be used.
    /// </summary>
    IList<Type> PageTypes { get; }
    /// <summary>
    /// Gets or sets action to configure the Maui context.
    /// </summary>
    Action<IMauiContext>? ConfigureContextAction { get; set; }

    /// <summary>
    /// Gets the maui application builder.
    /// </summary>
    /// <value>
    /// The maui application builder.
    /// </value>
    MauiAppBuilder MauiAppBuilder { get; }
}
