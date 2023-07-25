// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CP.Extensions.Hosting.Plugins;

/// <summary>
/// Plugin Base.
/// </summary>
/// <typeparam name="T1">The type of the 1st Plugin.</typeparam>
/// <typeparam name="T2">The type of the 2nd Plugin.</typeparam>
/// <typeparam name="T3">The type of the 3rd Plugin.</typeparam>
/// <seealso cref="CP.Extensions.Hosting.Plugins.IPlugin" />
/// <seealso cref="IPlugin" />
public class PluginBase<T1, T2, T3> : IPlugin
    where T1 : class, IHostedService
    where T2 : class, IHostedService
    where T3 : class, IHostedService
{
    /// <inheritdoc />
    public void ConfigureHost(HostBuilderContext hostBuilderContext, IServiceCollection serviceCollection) =>
        serviceCollection.AddHostedService<T1>().AddHostedService<T2>().AddHostedService<T3>();
}
