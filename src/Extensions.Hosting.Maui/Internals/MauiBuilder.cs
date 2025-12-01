// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;

namespace ReactiveMarbles.Extensions.Hosting.Maui.Internals;

/// <inheritdoc/>
internal class MauiBuilder : IMauiBuilder
{
    /// <inheritdoc/>
    public Type? ApplicationType { get; set; }

    /// <inheritdoc/>
    public Application? Application { get; set; }

    /// <inheritdoc/>
    public IList<Type> PageTypes { get; } = [];

    /// <inheritdoc/>
    public Action<IMauiContext>? ConfigureContextAction { get; set; }

    /// <summary>
    /// Gets the maui application builder.
    /// </summary>
    /// <value>
    /// The maui application builder.
    /// </value>
    public MauiAppBuilder MauiAppBuilder { get; } = MauiApp.CreateBuilder();
}
