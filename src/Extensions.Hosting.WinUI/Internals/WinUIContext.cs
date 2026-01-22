// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace ReactiveMarbles.Extensions.Hosting.WinUI.Internals;

/// <summary>
/// Provides context information and access to core WinUI application components, including the main window, dispatcher,
/// and application instance.
/// </summary>
/// <remarks>Use this class to interact with and manage the state of a WinUI application's main elements. It
/// exposes properties for accessing the application window, dispatcher queue, and application instance, as well as
/// flags indicating the application's running state and lifetime linkage. This context is typically used to coordinate
/// UI operations and manage application lifecycle events in WinUI-based applications.</remarks>
public class WinUIContext : IWinUIContext
{
    /// <inheritdoc />
    public Window? AppWindow { get; set; }

    /// <inheritdoc />
    public Type? AppWindowType { get; set; }

    /// <inheritdoc />
    public DispatcherQueue? Dispatcher { get; set; }

    /// <inheritdoc />
    public bool IsLifetimeLinked { get; set; }

    /// <inheritdoc />
    public bool IsRunning { get; set; }

    /// <inheritdoc />
    public Application? WinUIApplication { get; set; }
}
