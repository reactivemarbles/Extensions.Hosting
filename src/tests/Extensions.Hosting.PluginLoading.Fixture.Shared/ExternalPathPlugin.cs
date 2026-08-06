// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;

#if REACTIVE_SHIM
using ReactiveMarbles.Extensions.Hosting.Reactive.Plugins;

namespace Extensions.Hosting.PluginLoading.Reactive.Fixture;
#else
using ReactiveMarbles.Extensions.Hosting.Plugins;

namespace Extensions.Hosting.PluginLoading.Fixture;
#endif

/// <summary>Provides an external-path plugin fixture that is intentionally absent from the test host dependency graph.</summary>
public sealed class ExternalPathPlugin : IPlugin
{
    /// <inheritdoc />
    public void ConfigureHost(object hostBuilderContext, IServiceCollection serviceCollection)
    {
    }
}
