// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactiveMarbles.Extensions.Hosting.AppServices.Internal;

namespace ReactiveMarbles.Extensions.Hosting.AppServices;

/// <summary>Provides extension methods for configuring a host to ensure that only a single instance of the application runs at a time.</summary>
/// <remarks>These extension methods add single-instance enforcement to applications built with IHostBuilder or
/// IHostApplicationBuilder by configuring a mutex-based lifetime service. This is useful for preventing multiple
/// concurrent executions of the same application, such as desktop or service applications that should not be started
/// more than once on the same machine.</remarks>
public static class HostBuilderApplicationExtensions
{
    /// <summary>Stores the mutex builder key value.</summary>
    private const string MutexBuilderKey = nameof(MutexBuilder);

    /// <summary>Attempts to retrieve an existing <see cref="IMutexBuilder"/> instance from the specified properties dictionary, or creates and stores a new one if none exists.</summary>
    /// <param name="properties">The properties dictionary.</param>
    /// <param name="mutexBuilder">When this method returns, contains the retrieved or newly created <see cref="IMutexBuilder"/> instance.</param>
    /// <returns><see langword="true"/> if an existing <see cref="IMutexBuilder"/> was found in the dictionary; otherwise, <see langword="false"/>.</returns>
    private static bool TryRetrieveMutexBuilder(IDictionary<object, object> properties, out IMutexBuilder mutexBuilder)
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

    /// <summary>Adds the mutex services to the service collection.</summary>
    /// <param name="services">The service collection to update.</param>
    /// <param name="mutexBuilder">The configured mutex builder.</param>
    private static void AddMutexServices(IServiceCollection services, IMutexBuilder mutexBuilder)
    {
        _ = services
            .AddSingleton(mutexBuilder)
            .AddHostedService<MutexLifetimeService>();
    }

    /// <summary>Provides extension members for this receiver.</summary>
    /// <param name="hostBuilder">The receiver instance.</param>
    extension(IHostApplicationBuilder hostBuilder)
    {
        /// <summary>Configures the application to enforce single-instance behavior using a mutex and allows customization of the mutex configuration.</summary>
        /// <remarks>This method ensures that only one instance of the application runs at a time by registering a
        /// mutex and a hosted service to manage application lifetime. Use the <paramref name="configureAction"/> parameter
        /// to customize mutex options, such as the mutex name or behavior. This method should be called during application
        /// startup configuration.</remarks>
        /// <param name="configureAction">An action that configures the mutex builder used to enforce single-instance behavior. This action is invoked
        /// with the mutex builder instance.</param>
        /// <returns>The same <see cref="IHostApplicationBuilder"/> instance, enabling further configuration.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="hostBuilder"/> is null.</exception>
        public IHostApplicationBuilder ConfigureSingleInstance(Action<IMutexBuilder> configureAction)
        {
            if (hostBuilder is null)
            {
                throw new ArgumentNullException(nameof(hostBuilder));
            }

            if (!TryRetrieveMutexBuilder(hostBuilder.Properties, out var mutexBuilder))
            {
                AddMutexServices(hostBuilder.Services, mutexBuilder);
            }

            configureAction?.Invoke(mutexBuilder);

            return hostBuilder;
        }

        /// <summary>Configures the application to allow only a single instance to run by using a named mutex.</summary>
        /// <remarks>Use this method to prevent multiple instances of the application from running simultaneously.
        /// The mutex identifier should be unique to the application to avoid conflicts with other applications.</remarks>
        /// <param name="mutexId">The unique identifier for the mutex used to enforce single instance behavior. Cannot be null or empty.</param>
        /// <returns>The same instance of <see cref="IHostApplicationBuilder"/> for chaining further configuration.</returns>
        public IHostApplicationBuilder ConfigureSingleInstance(string mutexId) =>
            hostBuilder.ConfigureSingleInstance(builder => builder.MutexId = mutexId);
    }

    /// <summary>Provides extension members for this receiver.</summary>
    /// <param name="hostBuilder">The receiver instance.</param>
    extension(IHostBuilder hostBuilder)
    {
        /// <summary>Configures the host builder to enforce single-instance application behavior using a mutex, and allows additional mutex configuration.</summary>
        /// <remarks>This method ensures that only one instance of the application runs at a time by registering a
        /// mutex-based lifetime service. Additional mutex options can be configured using the provided <paramref
        /// name="configureAction"/>.</remarks>
        /// <param name="configureAction">An action to configure the mutex builder. Can be null if no additional configuration is required.</param>
        /// <returns>The same instance of <see cref="IHostBuilder"/> with single-instance enforcement configured.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="hostBuilder"/> is null.</exception>
        public IHostBuilder ConfigureSingleInstance(Action<IMutexBuilder> configureAction)
        {
            if (hostBuilder is null)
            {
                throw new ArgumentNullException(nameof(hostBuilder));
            }

            return hostBuilder.ConfigureServices((_, serviceCollection) =>
            {
                if (!TryRetrieveMutexBuilder(hostBuilder.Properties, out var mutexBuilder))
                {
                    AddMutexServices(serviceCollection, mutexBuilder);
                }

                configureAction?.Invoke(mutexBuilder);
            });
        }

        /// <summary>Configures the host builder to enforce single-instance application behavior using a system-wide mutex.</summary>
        /// <remarks>This method ensures that only one instance of the application can run at a time by using a
        /// named mutex. If another instance is already running with the same mutex identifier, subsequent instances will
        /// not start. Use a unique mutex identifier to avoid conflicts with other applications.</remarks>
        /// <param name="mutexId">The unique identifier for the mutex used to ensure only one instance of the application runs. Cannot be null or
        /// empty.</param>
        /// <returns>The same host builder instance, configured for single-instance enforcement.</returns>
        public IHostBuilder ConfigureSingleInstance(string mutexId) =>
            hostBuilder.ConfigureSingleInstance(builder => builder.MutexId = mutexId);
    }
}
