// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using ReactivePlugin = ReactiveMarbles.Extensions.Hosting.Reactive.Plugins.IPlugin;
using StandardPlugin = ReactiveMarbles.Extensions.Hosting.Plugins.IPlugin;

namespace Extensions.Hosting.Tests;

/// <summary>Conventional plugin type used to cover naming-convention scanners.</summary>
public sealed class Plugin : StandardPlugin, ReactivePlugin
{
    /// <inheritdoc />
    public void ConfigureHost(object hostBuilderContext, IServiceCollection serviceCollection)
    {
    }
}
