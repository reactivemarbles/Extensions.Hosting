// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.Logging;
using ReactiveMarbles.Extensions.Logging.Log4Net.Entities;
using Log4NetCoreLogger = log4net.Core.ILogger;
using LoggingEvent = log4net.Core.LoggingEvent;

namespace ReactiveMarbles.Extensions.Logging;

/// <inheritdoc cref="ILog4NetLoggingEventFactory"/>
public class Log4NetLoggingEventFactory : ILog4NetLoggingEventFactory
{
    /// <summary>The default property name for scopes that don't provide their own property name by implementing an <see cref="IEnumerable{T}"/> where T is <see cref="KeyValuePair{TKey,TValue}"/> and where TKey is <see cref="string"/>.</summary>
    protected const string DefaultScopeProperty = "scope";

    /// <summary>Stores the event id property value.</summary>
    private const string EventIdProperty = "eventId";

    /// <summary>Stores tuple scope handlers for supported tuple value types.</summary>
    private static readonly Func<object, LoggingEvent, bool>[] _tupleScopeHandlers =
    [
        TryAddTupleScope<string>,
        TryAddTupleScope<int>,
        TryAddTupleScope<long>,
        TryAddTupleScope<short>,
        TryAddTupleScope<decimal>,
        TryAddTupleScope<double>,
        TryAddTupleScope<float>,
        TryAddTupleScope<uint>,
        TryAddTupleScope<ulong>,
        TryAddTupleScope<ushort>,
        TryAddTupleScope<byte>,
        TryAddTupleScope<sbyte>,
        TryAddTupleScope<object>,
    ];

    /// <inheritdoc/>
    public LoggingEvent? CreateLoggingEvent<TState>(
        in MessageCandidate<TState> messageCandidate,
        Log4NetCoreLogger logger,
        Log4NetProviderOptions options,
        IExternalScopeProvider scopeProvider)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        if (logger is null)
        {
            throw new ArgumentNullException(nameof(logger));
        }

        var callerStackBoundaryDeclaringType = typeof(LoggerExtensions);
        var message = messageCandidate.Formatter(messageCandidate.State, messageCandidate.Exception);
        var logLevel = options.LogLevelTranslator?.TranslateLogLevel(messageCandidate.LogLevel, options);

        if (logLevel is null || (string.IsNullOrEmpty(message) && messageCandidate.Exception is null))
        {
            return null;
        }

        var loggingEvent = new LoggingEvent(
            callerStackBoundaryDeclaringType: callerStackBoundaryDeclaringType,
            repository: logger.Repository,
            loggerName: logger.Name,
            level: logLevel,
            message: message,
            exception: messageCandidate.Exception);

        EnrichWithScopes(loggingEvent, scopeProvider);

        loggingEvent.Properties[EventIdProperty] = messageCandidate.EventId;

        return loggingEvent;
    }

    /// <summary>
    /// Gets the scopes from the external scope provider and converts them to the properties on the logging event.
    /// This function will honor the convention that logging scopes can provide their own property name, by implementing
    /// an <see cref="IEnumerable{T}"/> where T is <see cref="KeyValuePair{TKey,TValue}"/> and where TKey is
    /// <see cref="string"/>.
    /// </summary>
    /// <remarks>
    /// The default implementation will call Convert.ToString(scope, CultureInfo.InvariantCulture) on all scope objects.
    /// If you want to do this conversion inside the Log4Net Pipeline, e. g. with a custom layout, you can override this
    /// method and change the behaviour.
    /// </remarks>
    /// <param name="loggingEvent">The <see cref="LoggingEvent"/> the scope information will be added to.</param>
    /// <param name="scopeProvider">The external provider for the current logging scope.</param>
    protected virtual void EnrichWithScopes(LoggingEvent loggingEvent, IExternalScopeProvider scopeProvider) =>
        scopeProvider?.ForEachScope(AddScopeProperties, loggingEvent);

    /// <summary>Adds a scope value to the logging event properties.</summary>
    /// <param name="scope">The scope value supplied by Microsoft.Extensions.Logging.</param>
    /// <param name="loggingEvent">The logging event to enrich.</param>
    private static void AddScopeProperties(object? scope, LoggingEvent loggingEvent)
    {
        if (scope is string scopeText)
        {
            AppendProperty(loggingEvent, DefaultScopeProperty, scopeText);
            return;
        }

        if (scope is null || TryAddEnumerableScope(scope, loggingEvent) || TryAddTupleScope(scope, loggingEvent))
        {
            return;
        }

        AppendProperty(loggingEvent, DefaultScopeProperty, Convert.ToString(scope, CultureInfo.InvariantCulture));
    }

    /// <summary>Adds key/value scope items from enumerable scope state.</summary>
    /// <param name="scope">The scope object to inspect.</param>
    /// <param name="loggingEvent">The logging event to enrich.</param>
    /// <returns>true when the scope was an enumerable scope; otherwise, false.</returns>
    private static bool TryAddEnumerableScope(object scope, LoggingEvent loggingEvent)
    {
        if (scope is not IEnumerable collection)
        {
            return false;
        }

        foreach (var item in collection)
        {
            if (item is KeyValuePair<string, string> stringPair)
            {
                AppendProperty(loggingEvent, stringPair.Key, stringPair.Value);
                continue;
            }

            if (item is KeyValuePair<string, object> objectPair)
            {
                AppendProperty(loggingEvent, objectPair.Key, Convert.ToString(objectPair.Value, CultureInfo.InvariantCulture));
            }
        }

        return true;
    }

    /// <summary>Adds key/value scope items from tuple scope state.</summary>
    /// <param name="scope">The scope object to inspect.</param>
    /// <param name="loggingEvent">The logging event to enrich.</param>
    /// <returns>true when the scope was a supported tuple scope; otherwise, false.</returns>
    private static bool TryAddTupleScope(object scope, LoggingEvent loggingEvent)
    {
        foreach (var tupleScopeHandler in _tupleScopeHandlers)
        {
            if (tupleScopeHandler(scope, loggingEvent))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Adds a key/value scope item from tuple scope state.</summary>
    /// <typeparam name="T">The tuple value type to match.</typeparam>
    /// <param name="scope">The scope object to inspect.</param>
    /// <param name="loggingEvent">The logging event to enrich.</param>
    /// <returns>true when the scope matched the tuple value type; otherwise, false.</returns>
    private static bool TryAddTupleScope<T>(object scope, LoggingEvent loggingEvent)
    {
        if (scope.GetType() != typeof((string Key, T Value)))
        {
            return false;
        }

        var valueTuple = ((string Key, T Value))scope;
        AppendProperty(loggingEvent, valueTuple.Key, Convert.ToString(valueTuple.Value, CultureInfo.InvariantCulture));
        return true;
    }

    /// <summary>Appends a log4net property value using the legacy scope separator.</summary>
    /// <param name="loggingEvent">The logging event to enrich.</param>
    /// <param name="propertyName">The property name to update.</param>
    /// <param name="newValue">The value to append.</param>
    private static void AppendProperty(LoggingEvent loggingEvent, string propertyName, string? newValue)
    {
        var previousValue = loggingEvent.Properties[propertyName] as string;
        loggingEvent.Properties[propertyName] = JoinOldAndNewValue(previousValue, newValue);
    }

    /// <summary>Joins an existing property value with a new value using the legacy scope separator.</summary>
    /// <param name="previousValue">The existing property value.</param>
    /// <param name="newValue">The new property value.</param>
    /// <returns>The combined value.</returns>
    private static string? JoinOldAndNewValue(string? previousValue, string? newValue) =>
        string.IsNullOrEmpty(previousValue) ? newValue : previousValue + " " + newValue;
}
