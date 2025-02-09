// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows;
using System.Windows.Threading;

namespace ReactiveMarbles.Extensions.Hosting.Wpf.Internals;

/// <inheritdoc />
internal class WpfContext : IWpfContext
{
    /// <inheritdoc />
    public ShutdownMode ShutdownMode { get; set; } = ShutdownMode.OnLastWindowClose;

    /// <inheritdoc />
    public bool IsLifetimeLinked { get; set; }

    /// <inheritdoc />
    public bool IsRunning { get; set; }

    /// <inheritdoc />
    public Application? WpfApplication { get; set; }

    /// <inheritdoc />
    public Dispatcher Dispatcher => WpfApplication?.Dispatcher!;
}
