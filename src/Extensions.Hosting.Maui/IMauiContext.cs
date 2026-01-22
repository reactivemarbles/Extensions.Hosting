// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using ReactiveMarbles.Extensions.Hosting.UiThread;

namespace ReactiveMarbles.Extensions.Hosting.Maui;

/// <summary>
/// Provides contextual services and resources for .NET MAUI applications, including access to the current application
/// and dispatcher.
/// </summary>
/// <remarks>IMauiContext is used to supply platform-specific context and services required by .NET MAUI
/// components. It enables access to the current Application instance and dispatcher, which are essential for
/// interacting with the application lifecycle and performing UI operations on the correct thread.</remarks>
public interface IMauiContext : IUiContext
{
    /// <summary>
    /// Gets or sets the current .NET MAUI application instance associated with the host environment.
    /// </summary>
    /// <remarks>This property is typically used to access or assign the main application object in a .NET
    /// MAUI app. Setting this property allows integration with the MAUI application lifecycle and services.</remarks>
    Application? MauiApplication { get; set; }

    /// <summary>
    /// Gets the dispatcher associated with the current context, if any.
    /// </summary>
    IDispatcher? Dispatcher { get; }
}
