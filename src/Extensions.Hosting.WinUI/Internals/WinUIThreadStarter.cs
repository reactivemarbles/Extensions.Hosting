// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

namespace ReactiveMarbles.Extensions.Hosting.WinUI.Internals;

/// <summary>Adapts a WinUI thread to the hosted-service startup seam.</summary>
internal sealed class WinUIThreadStarter : IUiThreadStarter
{
    /// <summary>Stores the action that starts the WinUI thread.</summary>
    private readonly Action _start;

    /// <summary>Initializes a new instance of the <see cref="WinUIThreadStarter"/> class.</summary>
    /// <param name="winUIThread">The WinUI thread to start.</param>
    internal WinUIThreadStarter(WinUIThread winUIThread)
        : this((winUIThread ?? throw new ArgumentNullException(nameof(winUIThread))).Start)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="WinUIThreadStarter"/> class.</summary>
    /// <param name="start">The action that starts the WinUI thread.</param>
    internal WinUIThreadStarter(Action start) =>
        _start = start ?? throw new ArgumentNullException(nameof(start));

    /// <inheritdoc />
    public void Start() =>
        _start();
}
