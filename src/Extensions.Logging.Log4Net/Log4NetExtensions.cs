// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ReactiveMarbles.Extensions.Logging;

/// <summary>
/// The log4net extensions class.
/// </summary>
public static class Log4NetExtensions
{
    /// <summary>
    /// Adds the log4net.
    /// </summary>
    /// <param name="factory">The factory.</param>
    /// <returns>The <see cref="ILoggerFactory"/> with added Log4Net provider.</returns>
    public static ILoggerFactory AddLog4Net(this ILoggerFactory factory)
        => factory.AddLog4Net(new Log4NetProviderOptions());

    /// <summary>
    /// Adds the log4net.
    /// </summary>
    /// <param name="factory">The factory.</param>
    /// <param name="log4NetConfigFile">The log4net Config File.</param>
    /// <returns>The <see cref="ILoggerFactory"/> after adding the log4net provider.</returns>
    public static ILoggerFactory AddLog4Net(this ILoggerFactory factory, string log4NetConfigFile)
        => factory.AddLog4Net(log4NetConfigFile, false);

    /// <summary>
    /// Adds the log4net logging provider.
    /// </summary>
    /// <param name="factory">The factory.</param>
    /// <param name="log4NetConfigFile">The log4 net configuration file.</param>
    /// <param name="watch">if set to <c>true</c> [watch].</param>
    /// <returns>The <see cref="ILoggerFactory"/> after adding the log4net provider.</returns>
    public static ILoggerFactory AddLog4Net(this ILoggerFactory factory, string log4NetConfigFile, bool watch)
        => factory.AddLog4Net(new Log4NetProviderOptions(log4NetConfigFile, watch));

    /// <summary>
    /// Adds the log4net logging provider.
    /// </summary>
    /// <param name="factory">The logger factory.</param>
    /// <param name="options">The options for log4net provider.</param>
    /// <returns>The <see cref="ILoggerFactory"/> after adding the log4net provider.</returns>
    public static ILoggerFactory AddLog4Net(this ILoggerFactory factory, Log4NetProviderOptions options)
    {
        if (factory == null)
        {
            throw new ArgumentNullException(nameof(factory));
        }

        factory.AddProvider(new Log4NetProvider(options));
        return factory;
    }

    /// <summary>
    /// Adds the log4net logging provider.
    /// </summary>
    /// <param name="builder">The logging builder instance.</param>
    /// <returns>The <see ref="ILoggingBuilder" /> passed as parameter with the new provider registered.</returns>
    public static ILoggingBuilder AddLog4Net(this ILoggingBuilder builder)
    {
        var options = new Log4NetProviderOptions();
        return builder.AddLog4Net(options);
    }

    /// <summary>
    /// Adds the log4net logging provider.
    /// </summary>
    /// <param name="builder">The logging builder instance.</param>
    /// <param name="log4NetConfigFile">The log4net Config File.</param>
    /// <returns>The <see ref="ILoggingBuilder" /> passed as parameter with the new provider registered.</returns>
    public static ILoggingBuilder AddLog4Net(this ILoggingBuilder builder, string log4NetConfigFile)
    {
        var options = new Log4NetProviderOptions(log4NetConfigFile);
        return builder.AddLog4Net(options);
    }

    /// <summary>
    /// Adds the log4net logging provider.
    /// </summary>
    /// <param name="builder">The logging builder instance.</param>
    /// <param name="log4NetConfigFile">The log4net Config File.</param>
    /// <param name="watch">if set to <c>true</c>, the configuration will be reloaded when the xml configuration file changes.</param>
    /// <returns>
    /// The <see ref="ILoggingBuilder" /> passed as parameter with the new provider registered.
    /// </returns>
    public static ILoggingBuilder AddLog4Net(this ILoggingBuilder builder, string log4NetConfigFile, bool watch)
    {
        var options = new Log4NetProviderOptions(log4NetConfigFile, watch);
        return builder.AddLog4Net(options);
    }

    /// <summary>
    /// Adds a Log4Net-based logging provider to the specified logging builder.
    /// </summary>
    /// <remarks>Use this method to enable Log4Net logging in an application's logging pipeline. This method
    /// registers the Log4Net provider as a singleton service.</remarks>
    /// <param name="builder">The logging builder to which the Log4Net provider will be added.</param>
    /// <param name="options">The options used to configure the Log4Net provider. Cannot be null.</param>
    /// <returns>The same instance of <see cref="ILoggingBuilder"/> for chaining.</returns>
    public static ILoggingBuilder AddLog4Net(this ILoggingBuilder builder, Log4NetProviderOptions options)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.Services.AddSingleton<ILoggerProvider>(new Log4NetProvider(options));
        return builder;
    }
}
