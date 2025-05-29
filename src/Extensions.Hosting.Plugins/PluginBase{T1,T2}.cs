// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ReactiveMarbles.Extensions.Hosting.Plugins;

/// <summary>
/// Plugin Base.
/// </summary>
/// <typeparam name="T1">The type of the 1st Plugin.</typeparam>
/// <typeparam name="T2">The type of the 2nd Plugin.</typeparam>
/// <seealso cref="IPlugin" />
public class PluginBase<T1, T2> : IPlugin
    where T1 : class, IHostedService
    where T2 : class, IHostedService
{
    /// <inheritdoc />
    public void ConfigureHost(object hostBuilderContext, IServiceCollection serviceCollection) =>
        serviceCollection.AddHostedService<T1>().AddHostedService<T2>();
}
