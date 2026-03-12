// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using ReactiveMarbles.Extensions.Hosting.UiThread;

namespace ReactiveMarbles.Extensions.Hosting.Avalonia;

/// <summary>
/// Provides a context for Avalonia applications, enabling management of application lifetime, shutdown behavior, and
/// access to the Avalonia dispatcher.
/// </summary>
/// <remarks>This interface is essential for configuring and controlling Avalonia application behavior. It allows
/// developers to specify shutdown modes, access the application instance, manage the application lifetime, and interact
/// with the Avalonia dispatcher for UI operations. Use this context to integrate Avalonia-specific features into
/// hosting environments or application frameworks.</remarks>
public interface IAvaloniaContext : IUiContext
{
    /// <summary>
    /// Gets or sets this is the Avalonia ShutdownMode used for the Avalonia application lifetime, default is OnLastWindowClose.
    /// </summary>
    ShutdownMode ShutdownMode { get; set; }

    /// <summary>
    /// Gets or sets the Application.
    /// </summary>
    Application? AvaloniaApplication { get; set; }

    /// <summary>
    /// Gets or sets the Application Lifetime.
    /// </summary>
    IClassicDesktopStyleApplicationLifetime? ApplicationLifetime { get; set; }

    /// <summary>
    /// Gets the Avalonia dispatcher.
    /// </summary>
    Dispatcher Dispatcher { get; }
}
