// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using Microsoft.Extensions.Logging;
using ReactiveMarbles.Extensions.Logging.Log4Net.Entities;
using Log4NetCoreLogger = log4net.Core.ILogger;
using LogManager = log4net.LogManager;

namespace ReactiveMarbles.Extensions.Logging;

/// <summary>The log4net logger class.</summary>
public class Log4NetLogger : ILogger
{
    /// <summary>Stores the external scope provider value.</summary>
    private readonly IExternalScopeProvider _externalScopeProvider;

    /// <summary>The log.</summary>
    private readonly Log4NetCoreLogger _logger;

    /// <summary>Initializes a new instance of the <see cref="Log4NetLogger"/> class using the specified provider options and external scope. provider.</summary>
    /// <param name="options">The options used to configure the logger, including repository and logger name information. Cannot be null.</param>
    /// <param name="externalScopeProvider">The external scope provider used to capture and manage logging scopes. Cannot be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if either options or externalScopeProvider is null.</exception>
    public Log4NetLogger(Log4NetProviderOptions options, IExternalScopeProvider externalScopeProvider)
    {
        Options = options ?? throw new ArgumentNullException(nameof(options));
        _externalScopeProvider = externalScopeProvider ?? throw new ArgumentNullException(nameof(externalScopeProvider));
        var loggerRepository = options.LoggerRepository;
        _logger = string.IsNullOrWhiteSpace(loggerRepository)
            ? LogManager.GetLogger(options.Name).Logger
            : LogManager.GetLogger(loggerRepository!, options.Name).Logger;
    }

    /// <summary>Gets the name.</summary>
    public string Name
        => _logger.Name;

    /// <summary>Gets a get-only property for accessing the <see cref="Log4NetProviderOptions"/> within the instance.</summary>
    internal Log4NetProviderOptions Options { get; }

    /// <summary>Begins a logical operation scope.</summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="state">The identifier for the scope.</param>
    /// <returns>
    /// An IDisposable that ends the logical operation scope on dispose.
    /// </returns>
    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull
        => _externalScopeProvider.Push(state);

    /// <summary>Determines whether the logging level is enabled.</summary>
    /// <param name="logLevel">The log level.</param>
    /// <returns>The <see cref="bool"/> value indicating whether the logging level is enabled.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Throws when <paramref name="logLevel"/> is outside allowed range.</exception>
    public bool IsEnabled(LogLevel logLevel)
    {
        var translatedLogLevel = Options.LogLevelTranslator?.TranslateLogLevel(logLevel, Options);
        if (translatedLogLevel is not null)
        {
            return _logger.IsEnabledFor(translatedLogLevel);
        }

        if (logLevel == LogLevel.None)
        {
            return false;
        }

        throw new ArgumentOutOfRangeException(nameof(logLevel));
    }

    /// <summary>Logs an exception into the log.</summary>
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

        if (loggingEvent is null)
        {
            return;
        }

        _logger.Log(loggingEvent);
    }

    /// <summary>Validates that a formatter delegate was supplied.</summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="formatter">The formatter to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when formatter is null.</exception>
    private static void EnsureValidFormatter<TState>(Func<TState, Exception?, string> formatter) =>
        _ = formatter ?? throw new ArgumentNullException(nameof(formatter));
}
