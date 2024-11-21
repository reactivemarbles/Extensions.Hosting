// Copyright (c) 2019-2024 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactiveMarbles.Extensions.Hosting.AppServices.Internal;

namespace ReactiveMarbles.Extensions.Hosting.AppServices;

/// <summary>
/// Extensions for loading plugins.
/// </summary>
public static class HostBuilderApplicationExtensions
{
    private const string MutexBuilderKey = nameof(MutexBuilder);

    /// <summary>
    /// Prevent that an application runs multiple times.
    /// </summary>
    /// <param name="hostBuilder">IHostBuilder.</param>
    /// <param name="configureAction">Action to configure IMutexBuilder.</param>
    /// <returns>
    /// IHostBuilder for fluently calling.
    /// </returns>
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
    /// Prevent that an application runs multiple times.
    /// </summary>
    /// <param name="hostBuilder">IHostBuilder.</param>
    /// <param name="mutexId">string.</param>
    /// <returns>IHostBuilder for fluently calling.</returns>
    public static IHostBuilder ConfigureSingleInstance(this IHostBuilder hostBuilder, string mutexId) =>
        hostBuilder.ConfigureSingleInstance(builder => builder.MutexId = mutexId);

    /// <summary>
    /// Helper method to retrieve the mutex builder.
    /// </summary>
    /// <param name="properties">IDictionary.</param>
    /// <param name="mutexBuilder">IMutexBuilder out value.</param>
    /// <returns>bool if there was a matcher.</returns>
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
