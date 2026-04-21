// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using log4net.Core;
using Microsoft.Extensions.Logging;

namespace ReactiveMarbles.Extensions.Logging;

/// <inheritdoc cref="ILog4NetLogLevelTranslator"/>
public sealed class Log4NetLogLevelTranslator : ILog4NetLogLevelTranslator
{
    /// <inheritdoc/>
    public Level? TranslateLogLevel(LogLevel logLevel, Log4NetProviderOptions options)
    {
        switch (logLevel)
        {
            case LogLevel.Critical:
                if (options == null)
                {
                    throw new ArgumentNullException(nameof(options));
                }

                var overrideCriticalLevelWith = options.OverrideCriticalLevelWith;
                return !string.IsNullOrEmpty(overrideCriticalLevelWith)
                        && overrideCriticalLevelWith.Equals(LogLevel.Critical.ToString(), StringComparison.OrdinalIgnoreCase)
                            ? Level.Critical
                            : Level.Fatal;
            case LogLevel.Debug:
                return Level.Debug;
            case LogLevel.Error:
                return Level.Error;
            case LogLevel.Information:
                return Level.Info;
            case LogLevel.Warning:
                return Level.Warn;
            case LogLevel.Trace:
                return Level.Trace;
        }

        return null;
    }
}
