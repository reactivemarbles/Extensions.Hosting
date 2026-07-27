// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

namespace ReactiveMarbles.Extensions.Hosting.Maui.Internals;

/// <summary>Adapts a MAUI thread to the hosted-service startup seam.</summary>
internal sealed class MauiThreadStarter : IUiThreadStarter
{
    /// <summary>Stores the action that starts the MAUI thread.</summary>
    private readonly Action _start;

    /// <summary>Initializes a new instance of the <see cref="MauiThreadStarter"/> class.</summary>
    /// <param name="mauiThread">The MAUI thread to start.</param>
    internal MauiThreadStarter(MauiThread mauiThread)
        : this((mauiThread ?? throw new ArgumentNullException(nameof(mauiThread))).Start)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="MauiThreadStarter"/> class.</summary>
    /// <param name="start">The action that starts the MAUI thread.</param>
    internal MauiThreadStarter(Action start) =>
        _start = start ?? throw new ArgumentNullException(nameof(start));

    /// <inheritdoc />
    public void Start() =>
        _start();
}
