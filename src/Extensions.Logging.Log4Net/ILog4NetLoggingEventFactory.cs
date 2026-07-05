// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;
using ReactiveMarbles.Extensions.Logging.Log4Net.Entities;
using Log4NetCoreLogger = log4net.Core.ILogger;
using LoggingEvent = log4net.Core.LoggingEvent;

namespace ReactiveMarbles.Extensions.Logging;

/// <summary>Represents a factory that creates the log4net <see cref="log4net.Core.LoggingEvent"/> from a <see cref="MessageCandidate{TState}"/>.</summary>
public interface ILog4NetLoggingEventFactory
{
    /// <summary>Creates a new log4net LoggingEvent instance using the specified message candidate, logger, provider options, and external scope provider.</summary>
    /// <typeparam name="TState">The type of the state object associated with the log message.</typeparam>
    /// <param name="messageCandidate">The candidate message containing the log state and related metadata to be included in the logging event.</param>
    /// <param name="logger">The log4net logger to associate with the created logging event. Cannot be null.</param>
    /// <param name="options">The provider options that configure how the logging event is created and formatted.</param>
    /// <param name="scopeProvider">The external scope provider used to supply additional contextual information for the logging event. May be
    /// null if no external scopes are required.</param>
    /// <returns>A LoggingEvent instance populated with the provided message, logger, options, and scope information.</returns>
    LoggingEvent? CreateLoggingEvent<TState>(
        in MessageCandidate<TState> messageCandidate,
        Log4NetCoreLogger logger,
        Log4NetProviderOptions options,
        IExternalScopeProvider scopeProvider);
}
