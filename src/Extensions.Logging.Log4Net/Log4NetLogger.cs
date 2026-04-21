// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using log4net;
using Microsoft.Extensions.Logging;
using ReactiveMarbles.Extensions.Logging.Log4Net.Entities;

namespace ReactiveMarbles.Extensions.Logging;

/// <summary>
/// The log4net logger class.
/// </summary>
public class Log4NetLogger : ILogger
{
    private readonly IExternalScopeProvider _externalScopeProvider;

    /// <summary>
    /// The log.
    /// </summary>
    private readonly log4net.Core.ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="Log4NetLogger"/> class using the specified provider options and external scope.
    /// provider.
    /// </summary>
    /// <param name="options">The options used to configure the logger, including repository and logger name information. Cannot be null.</param>
    /// <param name="externalScopeProvider">The external scope provider used to capture and manage logging scopes. Cannot be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if either options or externalScopeProvider is null.</exception>
    public Log4NetLogger(Log4NetProviderOptions options, IExternalScopeProvider externalScopeProvider)
    {
        Options = options ?? throw new ArgumentNullException(nameof(options));
        _externalScopeProvider = externalScopeProvider ?? throw new ArgumentNullException(nameof(externalScopeProvider));
        _logger = LogManager.GetLogger(options.LoggerRepository, options.Name).Logger;
    }

    /// <summary>
    /// Gets the name.
    /// </summary>
    public string Name
        => _logger.Name;

    /// <summary>
    /// Gets a get-only property for accessing the <see cref="Log4NetProviderOptions"/>
    /// within the instance.
    /// </summary>
    internal Log4NetProviderOptions Options { get; }

    /// <summary>
    /// Begins a logical operation scope.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="state">The identifier for the scope.</param>
    /// <returns>
    /// An IDisposable that ends the logical operation scope on dispose.
    /// </returns>
    public IDisposable BeginScope<TState>(TState state)
        => _externalScopeProvider.Push(state);

    /// <summary>
    /// Determines whether the logging level is enabled.
    /// </summary>
    /// <param name="logLevel">The log level.</param>
    /// <returns>The <see cref="bool"/> value indicating whether the logging level is enabled.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Throws when <paramref name="logLevel"/> is outside allowed range.</exception>
    public bool IsEnabled(LogLevel logLevel)
    {
        var translatedLogLevel = Options.LogLevelTranslator?.TranslateLogLevel(logLevel, Options);
        if (translatedLogLevel != null)
        {
            return _logger.IsEnabledFor(translatedLogLevel);
        }

        if (logLevel == LogLevel.None)
        {
            return false;
        }

        throw new ArgumentOutOfRangeException(nameof(logLevel));
    }

    /// <summary>
    /// Logs an exception into the log.
    /// </summary>
    /// <param name="logLevel">The log level.</param>
    /// <param name="eventId">The event Id.</param>
    /// <param name="state">The state.</param>
    /// <param name="exception">The exception.</param>
    /// <param name="formatter">The formatter.</param>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <exception cref="ArgumentNullException">Throws when the <paramref name="formatter"/> is null.</exception>
    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        EnsureValidFormatter(formatter);

        var candidate = new MessageCandidate<TState>(logLevel, eventId, state, exception, formatter);

        var loggingEvent = Options.LoggingEventFactory?.CreateLoggingEvent(in candidate, _logger, Options, _externalScopeProvider);

        if (loggingEvent == null)
        {
            return;
        }

        _logger.Log(loggingEvent);
    }

    private static void EnsureValidFormatter<TState>(Func<TState, Exception, string> formatter)
    {
        if (formatter == null)
        {
            throw new ArgumentNullException(nameof(formatter));
        }
    }
}
