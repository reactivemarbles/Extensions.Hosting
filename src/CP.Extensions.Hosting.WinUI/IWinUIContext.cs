// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using CP.Extensions.Hosting.UiThread;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace CP.Extensions.Hosting.WinUI;

/// <summary>
/// The WinUI context contains all information about the WinUI application and how it's started and stopped.
/// </summary>
public interface IWinUIContext : IUiContext
{
    /// <summary>
    /// Gets or sets started instance of <see cref="AppWindowType"/>.
    /// </summary>
    Window? AppWindow { get; set; }

    /// <summary>
    /// Gets or sets app Window type.
    /// </summary>
    Type? AppWindowType { get; set; }

    /// <summary>
    /// Gets or sets this WinUI dispatcher.
    /// </summary>
    DispatcherQueue? Dispatcher { get; set; }

    /// <summary>
    /// Gets or sets the Application.
    /// </summary>
    Application? WinUIApplication { get; set; }
}
