// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ReactiveMarbles.Extensions.Hosting.Plugins;

/// <summary>
/// Provides a base implementation for plugins that register a hosted service of the specified type with the
/// application's dependency injection container.
/// </summary>
/// <typeparam name="T">The type of the hosted service to register. Must implement <see cref="IHostedService"/>.</typeparam>
public class PluginBase<T> : IPlugin
    where T : class, IHostedService
{
    /// <inheritdoc />
    public void ConfigureHost(object hostBuilderContext, IServiceCollection serviceCollection) =>
        serviceCollection.AddHostedService<T>();
}
