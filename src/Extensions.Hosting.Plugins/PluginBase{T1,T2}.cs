// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ReactiveMarbles.Extensions.Hosting.Plugins;

/// <summary>
/// Provides a base implementation for plugins that register two hosted services with a dependency injection container.
/// </summary>
/// <remarks>This class is intended to be used as a base for plugins that require multiple hosted services to be
/// added to the application's service collection. Both type parameters must be reference types implementing <see
/// cref="IHostedService"/>.</remarks>
/// <typeparam name="T1">The type of the first hosted service to register. Must implement <see cref="IHostedService"/>.</typeparam>
/// <typeparam name="T2">The type of the second hosted service to register. Must implement <see cref="IHostedService"/>.</typeparam>
public class PluginBase<T1, T2> : IPlugin
    where T1 : class, IHostedService
    where T2 : class, IHostedService
{
    /// <inheritdoc />
    public void ConfigureHost(object hostBuilderContext, IServiceCollection serviceCollection) =>
        serviceCollection.AddHostedService<T1>().AddHostedService<T2>();
}
