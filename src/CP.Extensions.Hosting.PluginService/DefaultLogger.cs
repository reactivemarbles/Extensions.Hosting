// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;

namespace CP.Extensions.Hosting.PluginService;

/// <summary>
/// DefaultLogger.
/// </summary>
public class DefaultLogger
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultLogger"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public DefaultLogger(ILogger<DefaultLogger> logger) => Logger = logger;

    /// <summary>
    /// Gets the logger.
    /// </summary>
    /// <value>
    /// The logger.
    /// </value>
    public ILogger Logger { get; }
}
