// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using Microsoft.Extensions.Logging;
using Level = log4net.Core.Level;

namespace ReactiveMarbles.Extensions.Logging;

/// <inheritdoc cref="ILog4NetLogLevelTranslator"/>
public sealed class Log4NetLogLevelTranslator : ILog4NetLogLevelTranslator
{
    /// <inheritdoc/>
    public Level? TranslateLogLevel(LogLevel logLevel, Log4NetProviderOptions options) =>
        logLevel switch
        {
            LogLevel.Critical => TranslateCriticalLevel(options),
            LogLevel.Debug => Level.Debug,
            LogLevel.Error => Level.Error,
            LogLevel.Information => Level.Info,
            LogLevel.Warning => Level.Warn,
            LogLevel.Trace => Level.Trace,
            LogLevel.None => null,
            _ => null,
        };

    /// <summary>Translates the critical log level using the configured override value.</summary>
    /// <param name="options">The log4net provider options.</param>
    /// <returns>The log4net level to use for critical messages.</returns>
    /// <exception cref="ArgumentNullException">Thrown when options is null.</exception>
    private static Level TranslateCriticalLevel(Log4NetProviderOptions options)
    {
        _ = options ?? throw new ArgumentNullException(nameof(options));

        var overrideCriticalLevelWith = options.OverrideCriticalLevelWith;
        var useCriticalLevel = !string.IsNullOrEmpty(overrideCriticalLevelWith)
            && overrideCriticalLevelWith.Equals(nameof(LogLevel.Critical), StringComparison.OrdinalIgnoreCase);

        return useCriticalLevel
            ? Level.Critical
            : Level.Fatal;
    }
}
