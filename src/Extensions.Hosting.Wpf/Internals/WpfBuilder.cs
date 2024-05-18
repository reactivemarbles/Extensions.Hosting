// Copyright (c) 2019-2024 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Windows;

namespace ReactiveMarbles.Extensions.Hosting.Wpf.Internals;

/// <inheritdoc/>
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
