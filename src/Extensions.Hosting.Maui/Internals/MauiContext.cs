// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;

namespace ReactiveMarbles.Extensions.Hosting.Maui.Internals;

/// <inheritdoc />
internal class MauiContext : IMauiContext
{
    /// <inheritdoc />
    public bool IsLifetimeLinked { get; set; }

    /// <inheritdoc />
    public bool IsRunning { get; set; }

    /// <inheritdoc />
    public Application? MauiApplication { get; set; }

    /// <inheritdoc />
    public IDispatcher? Dispatcher => MauiApplication?.Dispatcher;
}
