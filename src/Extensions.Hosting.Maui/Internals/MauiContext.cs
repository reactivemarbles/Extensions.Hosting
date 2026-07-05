// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;

namespace ReactiveMarbles.Extensions.Hosting.Maui.Internals;

/// <inheritdoc />
internal sealed class MauiContext : IMauiContext
{
    /// <inheritdoc />
    public bool IsLifetimeLinked { get; set; }

    /// <inheritdoc />
    public bool IsRunning { get; set; }

    /// <inheritdoc />
    public Application? MauiApplication { get; set; }

    /// <inheritdoc />
    public IDispatcher? Dispatcher => Application.Current?.Dispatcher;
}
