// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactiveMarbles.Extensions.Hosting.AppServices.Internal;

namespace ReactiveMarbles.Extensions.Hosting.AppServices;

/// <summary>
/// Provides extension methods for configuring a host to ensure that only a single instance of the application runs at a
/// time.
/// </summary>
/// <remarks>These extension methods add single-instance enforcement to applications built with IHostBuilder or
/// IHostApplicationBuilder by configuring a mutex-based lifetime service. This is useful for preventing multiple
/// concurrent executions of the same application, such as desktop or service applications that should not be started
/// more than once on the same machine.</remarks>
public static class HostBuilderApplicationExtensions
{
    private const string MutexBuilderKey = nameof(MutexBuilder);

    /// <summary>
    /// Configures the host builder to enforce single-instance application behavior using a mutex, and allows additional
    /// mutex configuration.
    /// </summary>
    /// <remarks>This method ensures that only one instance of the application runs at a time by registering a
    /// mutex-based lifetime service. Additional mutex options can be configured using the provided <paramref
    /// name="configureAction"/>.</remarks>
    /// <param name="hostBuilder">The host builder to configure for single-instance enforcement. Cannot be null.</param>
    /// <param name="configureAction">An action to configure the mutex builder. Can be null if no additional configuration is required.</param>
    /// <returns>The same instance of <see cref="IHostBuilder"/> with single-instance enforcement configured.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="hostBuilder"/> is null.</exception>
    public static IHostBuilder ConfigureSingleInstance(this IHostBuilder hostBuilder, Action<IMutexBuilder> configureAction)
    {
        if (hostBuilder is null)
        {
            throw new ArgumentNullException(nameof(hostBuilder));
        }

        return hostBuilder.ConfigureServices((_, serviceCollection) =>
        {
            if (!TryRetrieveMutexBuilder(hostBuilder.Properties, out var mutexBuilder))
            {
                serviceCollection
                    .AddSingleton(mutexBuilder)
                    .AddHostedService<MutexLifetimeService>();
            }

            configureAction?.Invoke(mutexBuilder);
        });
    }

    /// <summary>
    /// Configures the application to enforce single-instance behavior using a mutex and allows customization of the
    /// mutex configuration.
    /// </summary>
    /// <remarks>This method ensures that only one instance of the application runs at a time by registering a
    /// mutex and a hosted service to manage application lifetime. Use the <paramref name="configureAction"/> parameter
    /// to customize mutex options, such as the mutex name or behavior. This method should be called during application
    /// startup configuration.</remarks>
    /// <param name="hostBuilder">The host application builder to configure for single-instance enforcement. Cannot be null.</param>
    /// <param name="configureAction">An action that configures the mutex builder used to enforce single-instance behavior. This action is invoked
    /// with the mutex builder instance.</param>
    /// <returns>The same <see cref="IHostApplicationBuilder"/> instance, enabling further configuration.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="hostBuilder"/> is null.</exception>
    public static IHostApplicationBuilder ConfigureSingleInstance(this IHostApplicationBuilder hostBuilder, Action<IMutexBuilder> configureAction)
    {
        if (hostBuilder is null)
        {
            throw new ArgumentNullException(nameof(hostBuilder));
        }

        if (!TryRetrieveMutexBuilder(hostBuilder.Properties, out var mutexBuilder))
        {
            hostBuilder.Services
                .AddSingleton(mutexBuilder)
                .AddHostedService<MutexLifetimeService>();
        }

        configureAction?.Invoke(mutexBuilder);

        return hostBuilder;
    }

    /// <summary>
    /// Configures the host builder to enforce single-instance application behavior using a system-wide mutex.
    /// </summary>
    /// <remarks>This method ensures that only one instance of the application can run at a time by using a
    /// named mutex. If another instance is already running with the same mutex identifier, subsequent instances will
    /// not start. Use a unique mutex identifier to avoid conflicts with other applications.</remarks>
    /// <param name="hostBuilder">The host builder to configure for single-instance enforcement.</param>
    /// <param name="mutexId">The unique identifier for the mutex used to ensure only one instance of the application runs. Cannot be null or
    /// empty.</param>
    /// <returns>The same host builder instance, configured for single-instance enforcement.</returns>
    public static IHostBuilder ConfigureSingleInstance(this IHostBuilder hostBuilder, string mutexId) =>
        hostBuilder.ConfigureSingleInstance(builder => builder.MutexId = mutexId);

    /// <summary>
    /// Configures the application to allow only a single instance to run by using a named mutex.
    /// </summary>
    /// <remarks>Use this method to prevent multiple instances of the application from running simultaneously.
    /// The mutex identifier should be unique to the application to avoid conflicts with other applications.</remarks>
    /// <param name="hostBuilder">The host application builder to configure for single instance enforcement.</param>
    /// <param name="mutexId">The unique identifier for the mutex used to enforce single instance behavior. Cannot be null or empty.</param>
    /// <returns>The same instance of <see cref="IHostApplicationBuilder"/> for chaining further configuration.</returns>
    public static IHostApplicationBuilder ConfigureSingleInstance(this IHostApplicationBuilder hostBuilder, string mutexId) =>
        hostBuilder.ConfigureSingleInstance(builder => builder.MutexId = mutexId);

    /// <summary>
    /// Attempts to retrieve an existing <see cref="IMutexBuilder"/> instance from the specified properties dictionary,
    /// or creates and stores a new one if none exists.
    /// </summary>
    /// <remarks>If no <see cref="IMutexBuilder"/> is present in the dictionary, a new instance is created,
    /// added to the dictionary, and returned via the <paramref name="mutexBuilder"/> parameter.</remarks>
    /// <param name="properties">The dictionary of properties used to store and retrieve the <see cref="IMutexBuilder"/> instance. Cannot be
    /// null.</param>
    /// <param name="mutexBuilder">When this method returns, contains the retrieved or newly created <see cref="IMutexBuilder"/> instance.</param>
    /// <returns><see langword="true"/> if an existing <see cref="IMutexBuilder"/> was found in the dictionary; otherwise, <see
    /// langword="false"/>.</returns>
    private static bool TryRetrieveMutexBuilder(this IDictionary<object, object> properties, out IMutexBuilder mutexBuilder)
    {
        if (properties.TryGetValue(MutexBuilderKey, out var mutexBuilderObject))
        {
            mutexBuilder = (IMutexBuilder)mutexBuilderObject;
            return true;
        }

        mutexBuilder = new MutexBuilder();
        properties[MutexBuilderKey] = mutexBuilder;
        return false;
    }
}
