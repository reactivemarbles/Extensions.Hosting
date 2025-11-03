// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;

namespace ReactiveMarbles.Extensions.Hosting.Plugins;

/// <summary>
/// This interface is the connection between the host and the plug-in code.
/// </summary>
public interface IPlugin
{
    /// <summary>
    /// Implementing this method allows a plug-in to configure the host.
    /// This makes it possible to add services etc.
    /// </summary>
    /// <param name="hostBuilderContext">HostBuilderContext.</param>
    /// <param name="serviceCollection">IServiceCollection.</param>
    void ConfigureHost(object hostBuilderContext, IServiceCollection serviceCollection);
}
