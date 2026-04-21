// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using log4net;

namespace ReactiveMarbles.Extensions.Logging.Log4Net.Extensions;

/// <summary>
/// Provides extension methods for the ILog interface to support logging messages at custom log levels such as Critical
/// and Trace.
/// </summary>
/// <remarks>These extension methods enable logging at additional severity levels not present in the standard ILog
/// interface. Use these methods to log messages with Critical or Trace importance when using log4net.</remarks>
public static class LogExtensions
{
    /// <summary>
    /// Criticals the specified message.
    /// </summary>
    /// <param name="log">The log.</param>
    /// <param name="message">The message.</param>
    /// <param name="exception">The exception.</param>
    public static void Critical(this ILog log, object message, Exception exception)
        => log?.Logger.Log(null!, log4net.Core.Level.Critical, message, exception);

    /// <summary>
    /// Traces the specified message.
    /// </summary>
    /// <param name="log">The log.</param>
    /// <param name="message">The message.</param>
    /// <param name="exception">The exception.</param>
    public static void Trace(this ILog log, object message, Exception exception)
        => log?.Logger.Log(null!, log4net.Core.Level.Trace, message, exception);
}
