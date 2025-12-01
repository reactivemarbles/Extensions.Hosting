// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using ReactiveMarbles.Extensions.Hosting.UiThread;

namespace ReactiveMarbles.Extensions.Hosting.Maui;

/// <summary>
/// The MAUI context contains all information about the MAUI application and how it's started and stopped.
/// </summary>
public interface IMauiContext : IUiContext
{
    /// <summary>
    /// Gets or sets the Application.
    /// </summary>
    Application? MauiApplication { get; set; }

    /// <summary>
    /// Gets this MAUI dispatcher.
    /// </summary>
    IDispatcher? Dispatcher { get; }
}
