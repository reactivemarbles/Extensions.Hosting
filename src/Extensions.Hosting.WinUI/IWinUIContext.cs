// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using ReactiveMarbles.Extensions.Hosting.UiThread;

namespace ReactiveMarbles.Extensions.Hosting.WinUI;

/// <summary>
/// Defines a context for WinUI-based user interface operations, providing access to the application window, dispatcher,
/// and related WinUI components.
/// </summary>
/// <remarks>Implementations of this interface enable interaction with WinUI elements and threading
/// infrastructure, facilitating UI operations within a WinUI application environment. This context is typically used to
/// coordinate UI actions, access the main application window, and dispatch work to the UI thread.</remarks>
public interface IWinUIContext : IUiContext
{
    /// <summary>
    /// Gets or sets the application window associated with the current context.
    /// </summary>
    Window? AppWindow { get; set; }

    /// <summary>
    /// Gets or sets the type of the application window to be created or managed.
    /// </summary>
    /// <remarks>Set this property to specify a custom window type for the application. If the value is null,
    /// the default window type will be used. Changing this property may affect how the application window is
    /// instantiated or displayed.</remarks>
    Type? AppWindowType { get; set; }

    /// <summary>
    /// Gets or sets the dispatcher queue associated with the object.
    /// </summary>
    /// <remarks>The dispatcher queue is typically used to schedule work to run on a specific thread, such as
    /// the UI thread in a Windows application. Assigning a dispatcher queue enables thread-safe operations and
    /// coordination with the application's message loop.</remarks>
    DispatcherQueue? Dispatcher { get; set; }

    /// <summary>
    /// Gets or sets the current Windows UI Application instance associated with the host.
    /// </summary>
    Application? WinUIApplication { get; set; }
}
