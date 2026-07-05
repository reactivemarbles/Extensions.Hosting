// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using Microsoft.Extensions.Logging;

namespace ReactiveMarbles.Extensions.Logging.Log4Net.Entities;

/// <summary>Represents a candidate log message, including its log level, event identifier, state, optional exception, and a formatter function for generating the message text.</summary>
/// <remarks>Use this struct to encapsulate all information required to format and emit a log message. The
/// formatter function can be used to produce the final log message string based on the provided state and exception.
/// This type is typically used in logging frameworks to defer message formatting until it is needed.</remarks>
/// <typeparam name="TState">The type of the state object associated with the log message. This provides contextual information or content for
/// the log entry.</typeparam>
public readonly record struct MessageCandidate<TState>
{
    /// <summary>Initializes a new instance of the <see cref="MessageCandidate{TState}"/> struct. Initializes a new instance of the MessageCandidate class with the specified log level, event identifier, state, exception, and formatter.</summary>
    /// <param name="logLevel">The severity level of the log entry.</param>
    /// <param name="eventId">The identifier for the event associated with the log entry.</param>
    /// <param name="state">The state object to be logged. Represents the primary content or context for the log entry.</param>
    /// <param name="exception">The exception related to the log entry, or null if no exception is associated.</param>
    /// <param name="formatter">A function that formats the state and exception into a log message string.</param>
    public MessageCandidate(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        State = state;
        LogLevel = logLevel;
        EventId = eventId;
        Exception = exception;
        Formatter = formatter;
    }

    /// <summary>Gets the log level the message should be printed with.</summary>
    public LogLevel LogLevel { get; }

    /// <summary>Gets the event id of the message.</summary>
    public EventId EventId { get; }

    /// <summary>Gets the message state. Can be provided to the formatter to generate the string representation of the error message.</summary>
    public TState State { get; }

    /// <summary>Gets exception that should be printed with the message. Null if the log message has no corrosponding exception.</summary>
    public Exception? Exception { get; }

    /// <summary>Gets the message formatter. Can be called with the state and exception to generate the string representation of the error message.</summary>
    public Func<TState, Exception?, string> Formatter { get; }
}
