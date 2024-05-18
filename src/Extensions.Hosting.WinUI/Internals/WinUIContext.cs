// Copyright (c) 2019-2024 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace ReactiveMarbles.Extensions.Hosting.WinUI.Internals;

/// <inheritdoc />
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
