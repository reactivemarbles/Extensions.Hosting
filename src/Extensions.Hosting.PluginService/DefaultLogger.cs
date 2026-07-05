// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;

namespace ReactiveMarbles.Extensions.Hosting.PluginService;

/// <summary>Provides a default implementation of a logger that wraps an existing <see cref="ILogger"/> instance.</summary>
/// <param name="logger">The underlying <see cref="ILogger{DefaultLogger}"/> instance to use for logging operations. Cannot be null.</param>
public class DefaultLogger(ILogger<DefaultLogger> logger)
{
    /// <summary>Gets the logger instance used for recording diagnostic and operational messages.</summary>
    public ILogger Logger { get; } = logger;
}
