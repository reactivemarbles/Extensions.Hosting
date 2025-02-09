// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;

namespace ReactiveMarbles.Extensions.Hosting.PluginService;

/// <summary>
/// DefaultLogger.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="DefaultLogger"/> class.
/// </remarks>
/// <param name="logger">The logger.</param>
public class DefaultLogger(ILogger<DefaultLogger> logger)
{
    /// <summary>
    /// Gets the logger.
    /// </summary>
    /// <value>
    /// The logger.
    /// </value>
    public ILogger Logger { get; } = logger;
}
