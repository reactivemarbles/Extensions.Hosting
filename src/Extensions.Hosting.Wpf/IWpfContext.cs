// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows;
using System.Windows.Threading;
using ReactiveMarbles.Extensions.Hosting.UiThread;

namespace ReactiveMarbles.Extensions.Hosting.Wpf;

/// <summary>
/// Defines a context for WPF-based UI operations, providing access to the application, dispatcher, and shutdown
/// behavior.
/// </summary>
/// <remarks>Use this interface to interact with WPF-specific application features within a UI context
/// abstraction. It exposes properties for managing application lifetime, accessing the WPF dispatcher for thread-safe
/// UI operations, and referencing the current WPF application instance.</remarks>
public interface IWpfContext : IUiContext
{
    /// <summary>
    /// Gets or sets the shutdown behavior for the application.
    /// </summary>
    /// <remarks>The shutdown mode determines when the application will automatically shut down. This setting
    /// affects how and when the application's main window or all windows are closed, and can be used to control
    /// application lifetime in different scenarios.</remarks>
    ShutdownMode ShutdownMode { get; set; }

    /// <summary>
    /// Gets or sets the current WPF application instance associated with the host.
    /// </summary>
    Application? WpfApplication { get; set; }

    /// <summary>
    /// Gets the dispatcher associated with the current object.
    /// </summary>
    /// <remarks>The dispatcher can be used to execute code on the thread that the object is associated with,
    /// typically the UI thread in applications that use a dispatcher model. This is useful for marshaling calls from
    /// background threads to the main thread.</remarks>
    Dispatcher Dispatcher { get; }
}
