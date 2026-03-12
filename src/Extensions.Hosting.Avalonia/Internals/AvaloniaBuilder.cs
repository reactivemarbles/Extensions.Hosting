// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Avalonia;

namespace ReactiveMarbles.Extensions.Hosting.Avalonia.Internals;

/// <inheritdoc />
internal class AvaloniaBuilder : IAvaloniaBuilder
{
    /// <inheritdoc />
    public Type? ApplicationType { get; set; }

    /// <inheritdoc />
    public Application? Application { get; set; }

    /// <inheritdoc />
    public IList<Type> WindowTypes { get; } = new List<Type>();

    /// <inheritdoc />
    public Action<IAvaloniaContext>? ConfigureContextAction { get; set; }

    /// <inheritdoc />
    public Action<AppBuilder>? ConfigureAppBuilderAction { get; set; }
}
