// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;

namespace CP.Extensions.Hosting.PluginService;

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
