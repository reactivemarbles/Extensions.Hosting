// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using Microsoft.Extensions.Logging;

namespace ReactiveMarbles.Extensions.Logging.Log4Net.Scope;

/// <summary>A <see cref="IExternalScopeProvider"/> that will not save nor return scopes.</summary>
internal sealed class NullScopeProvider : IExternalScopeProvider
{
    /// <summary>Initializes a new instance of the <see cref="NullScopeProvider"/> class. Constructor that prevents external instantiation.</summary>
    private NullScopeProvider()
    {
    }

    /// <summary>Gets the singleton instance that represents every <see cref="NullScopeProvider"/>.</summary>
    internal static NullScopeProvider Instance { get; } = new NullScopeProvider();

    /// <inheritdoc/>
    public void ForEachScope<TState>(Action<object, TState> callback, TState state)
    {
        // All scopes are null scopes so do nothing.
    }

    /// <inheritdoc/>
    public IDisposable Push(object? state) => NullScope.Instance;
}
