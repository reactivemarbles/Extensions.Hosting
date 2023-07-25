// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace CP.Extensions.Hosting.UiThread;

/// <inheritdoc />
public abstract class BaseUiContext : IUiContext
{
    /// <inheritdoc />
    public bool IsLifetimeLinked { get; set; }

    /// <inheritdoc />
    public bool IsRunning { get; set; }
}
