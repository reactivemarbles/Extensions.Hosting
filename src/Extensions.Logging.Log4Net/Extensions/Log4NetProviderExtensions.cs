// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using Microsoft.Extensions.Logging;

namespace ReactiveMarbles.Extensions.Logging.Log4Net.Extensions;

/// <summary>
/// Log4Net provider extensions.
/// </summary>
public static class Log4NetProviderExtensions
{
    /// <summary>
    /// Creates a logger instance for the specified category type using the provided logger provider.
    /// </summary>
    /// <remarks>This extension method simplifies logger creation by associating the logger with the
    /// full name of the specified category type. Only logger providers of type Log4NetProvider are
    /// supported.</remarks>
    /// <typeparam name="TName">The category type for which to create a logger. Typically, this is the class type that will use the logger.</typeparam>
    /// <param name="self">The logger provider used to create the logger. Must be of type Log4NetProvider.</param>
    /// <returns>An ILogger instance associated with the specified category type.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the logger provider is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the logger provider is not of type Log4NetProvider.</exception>
    public static ILogger CreateLogger<TName>(this ILoggerProvider self)
        where TName : class
    {
        if (self == null)
        {
            throw new ArgumentNullException(nameof(self));
        }

        if (!self.GetType().IsAssignableFrom(typeof(Log4NetProvider)))
        {
            throw new ArgumentOutOfRangeException(nameof(self), "The ILoggerProvider should be of type Log4NetProvider.");
        }

        return self.CreateLogger(typeof(TName).FullName!);
    }
}
