// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Windows;

namespace ReactiveMarbles.Extensions.Hosting.Wpf.Internals;

/// <summary>
/// Provides a builder for configuring and initializing WPF application components, including the application type, main
/// application instance, and associated window types.
/// </summary>
/// <remarks>Use this class to set up the WPF application environment before launching the application. The
/// builder allows customization of the application type, the application instance, and the set of window types to be
/// managed. It also supports configuring the WPF context through a delegate. This class is intended for internal use
/// within the WPF application infrastructure.</remarks>
internal class WpfBuilder : IWpfBuilder
{
    /// <inheritdoc/>
    public Type? ApplicationType { get; set; }

    /// <inheritdoc/>
    public Application? Application { get; set; }

    /// <inheritdoc/>
    public IList<Type> WindowTypes { get; } = [];

    /// <inheritdoc/>
    public Action<IWpfContext>? ConfigureContextAction { get; set; }
}
