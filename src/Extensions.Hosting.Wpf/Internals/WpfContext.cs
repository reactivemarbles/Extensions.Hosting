// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows;
using System.Windows.Threading;

namespace ReactiveMarbles.Extensions.Hosting.Wpf.Internals;

/// <summary>
/// Provides context and state information for a running WPF application, including access to the application instance,
/// dispatcher, and shutdown behavior.
/// </summary>
/// <remarks>This class is intended for internal use to coordinate WPF application lifetime and threading. It
/// exposes properties for managing shutdown mode, application lifetime linkage, and access to the application's
/// dispatcher for thread-safe operations.</remarks>
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
