// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using Microsoft.Extensions.Logging;

namespace ReactiveMarbles.Extensions.Logging.Log4Net.Extensions;

/// <summary>Log4Net provider extensions.</summary>
public static class Log4NetProviderExtensions
{
    /// <summary>Provides extension members for this receiver.</summary>
    /// <param name="self">The receiver instance.</param>
    extension(ILoggerProvider self)
    {
        /// <summary>Creates a logger instance for the specified category type using the provided logger provider.</summary>
        /// <remarks>This extension method simplifies logger creation by associating the logger with the
        /// full name of the specified category type. Only logger providers of type Log4NetProvider are
        /// supported.</remarks>
        /// <typeparam name="TName">The category type for which to create a logger. Typically, this is the class type that will use the logger.</typeparam>
        /// <returns>An ILogger instance associated with the specified category type.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the logger provider is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the logger provider is not of type Log4NetProvider.</exception>
        public ILogger CreateLogger<TName>()
            where TName : class
        {
            if (self is null)
            {
                throw new ArgumentNullException(nameof(self));
            }

            if (self is not Log4NetProvider)
            {
                throw new ArgumentOutOfRangeException(nameof(self), "The ILoggerProvider should be of type Log4NetProvider.");
            }

            return self.CreateLogger(typeof(TName).FullName!);
        }
    }
}
