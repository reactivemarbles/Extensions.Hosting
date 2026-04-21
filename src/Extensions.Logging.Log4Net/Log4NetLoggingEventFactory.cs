// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using log4net.Core;
using Microsoft.Extensions.Logging;
using ReactiveMarbles.Extensions.Logging.Log4Net.Entities;

namespace ReactiveMarbles.Extensions.Logging;

/// <inheritdoc cref="ILog4NetLoggingEventFactory"/>
public class Log4NetLoggingEventFactory
    : ILog4NetLoggingEventFactory
{
    /// <summary>
    /// The default property name for scopes that don't provide their own property name by implementing
    /// an <see cref="IEnumerable{T}"/> where T is <see cref="KeyValuePair{TKey,TValue}"/> and where TKey
    /// is <see cref="string"/>.
    /// </summary>
    protected const string DefaultScopeProperty = "scope";
    private const string EventIdProperty = "eventId";

    /// <inheritdoc/>
    public LoggingEvent? CreateLoggingEvent<TState>(
        in MessageCandidate<TState> messageCandidate,
        log4net.Core.ILogger logger,
        Log4NetProviderOptions options,
        IExternalScopeProvider scopeProvider)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        if (logger == null)
        {
            throw new ArgumentNullException(nameof(logger));
        }

        var callerStackBoundaryDeclaringType = typeof(LoggerExtensions);
        var message = messageCandidate.Formatter(messageCandidate.State, messageCandidate.Exception);
        var logLevel = options.LogLevelTranslator?.TranslateLogLevel(messageCandidate.LogLevel, options);

        if (logLevel == null || (string.IsNullOrEmpty(message) && messageCandidate.Exception == null))
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
    protected virtual void EnrichWithScopes(LoggingEvent loggingEvent, IExternalScopeProvider scopeProvider) => scopeProvider?.ForEachScope(
            (scope, @event) =>
        {
            // This function will add the scopes in the legacy way they were added before the IExternalScopeProvider was introduced,
            // to maintain backwards compatibility.
            // This pretty much means that we are emulating a LogicalThreadContextStack, which is a stack, that allows pushing
            // strings on to it, which will be concatenated with space as a separator.
            // See: https://github.com/apache/logging-log4net/blob/47aaf46d5f031ea29d781bac4617bd1bb9446215/src/log4net/Util/LogicalThreadContextStack.cs#L343

            // Because string implements IEnumerable we first need to check for string.
            if (scope is string)
            {
                var previousValue = @event.Properties[DefaultScopeProperty] as string;

                @event.Properties[DefaultScopeProperty] = JoinOldAndNewValue(previousValue, scope.ToString());
                return;
            }

            if (scope is IEnumerable col)
            {
                foreach (var item in col)
                {
                    if (item is KeyValuePair<string, string> keyValuePair3)
                    {
                        var keyValuePair = keyValuePair3;
                        var previousValue = @event.Properties[keyValuePair.Key] as string;
                        @event.Properties[keyValuePair.Key] = JoinOldAndNewValue(previousValue, keyValuePair.Value);
                        continue;
                    }

                    if (item is KeyValuePair<string, object> keyValuePair2)
                    {
                        var keyValuePair = keyValuePair2;
                        var previousValue = @event.Properties[keyValuePair.Key] as string;

                        // The current culture should not influence how integers/floats/... are displayed in logging,
                        // so we are using Convert.ToString which will convert IConvertible and IFormattable with
                        // the specified IFormatProvider.
                        var additionalValue = Convert.ToString(keyValuePair.Value, CultureInfo.InvariantCulture);
                        @event.Properties[keyValuePair.Key] = JoinOldAndNewValue(previousValue, additionalValue);
                        continue;
                    }
                }

                return;
            }

            if (FromValueTuple<string>())
            {
                return;
            }

            if (FromValueTuple<int>())
            {
                return;
            }

            if (FromValueTuple<long>())
            {
                return;
            }

            if (FromValueTuple<short>())
            {
                return;
            }

            if (FromValueTuple<decimal>())
            {
                return;
            }

            if (FromValueTuple<double>())
            {
                return;
            }

            if (FromValueTuple<float>())
            {
                return;
            }

            if (FromValueTuple<uint>())
            {
                return;
            }

            if (FromValueTuple<ulong>())
            {
                return;
            }

            if (FromValueTuple<ushort>())
            {
                return;
            }

            if (FromValueTuple<byte>())
            {
                return;
            }

            if (FromValueTuple<sbyte>())
            {
                return;
            }

            if (FromValueTuple<object>())
            {
                return;
            }

            if (scope is not null)
            {
                var previousValue = @event.Properties[DefaultScopeProperty] as string;
                var additionalValue = Convert.ToString(scope, CultureInfo.InvariantCulture);
                @event.Properties[DefaultScopeProperty] = JoinOldAndNewValue(previousValue, additionalValue);
                return;
            }

            bool FromValueTuple<T>()
            {
                if (scope is ValueTuple<string, T> valueTuple)
                {
                    var previousValue = @event.Properties[valueTuple.Item1] as string;
                    var additionalValue = Convert.ToString(valueTuple.Item2, CultureInfo.InvariantCulture);
                    @event.Properties[valueTuple.Item1] = JoinOldAndNewValue(previousValue, additionalValue);
                    return true;
                }

                return false;
            }
        },
            loggingEvent);

    private static string? JoinOldAndNewValue(string? previousValue, string? newValue)
    {
        if (string.IsNullOrEmpty(previousValue))
        {
            return newValue;
        }

        return previousValue + " " + newValue;
    }
}
