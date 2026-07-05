// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

namespace ReactiveMarbles.Extensions.Logging.Log4Net.Scope;

/// <summary>A logger scope that does not save any information and does not need to be disposed.</summary>
internal sealed class NullScope : IDisposable
{
    /// <summary>Initializes a new instance of the <see cref="NullScope"/> class. Constructor that prevents external instantiation.</summary>
    private NullScope()
    {
    }

    /// <summary>Gets the singleton instance that represent every <see cref="NullScope"/>.</summary>
    internal static NullScope Instance { get; } = new NullScope();

    /// <inheritdoc/>
    public void Dispose()
    {
        // This is a null scope so we need to dispose nothing.
    }
}
