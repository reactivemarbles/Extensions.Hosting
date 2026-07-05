// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;

namespace ReactiveMarbles.Extensions.Hosting.Avalonia.Internals;

/// <inheritdoc />
internal sealed class AvaloniaContext : IAvaloniaContext
{
    /// <inheritdoc />
    public ShutdownMode ShutdownMode { get; set; } = ShutdownMode.OnLastWindowClose;

    /// <inheritdoc />
    public bool IsLifetimeLinked { get; set; }

    /// <inheritdoc />
    public bool IsRunning { get; set; }

    /// <inheritdoc />
    public Application? AvaloniaApplication { get; set; }

    /// <inheritdoc />
    public IClassicDesktopStyleApplicationLifetime? ApplicationLifetime { get; set; }

    /// <inheritdoc />
    public Dispatcher Dispatcher => Dispatcher.UIThread;
}
