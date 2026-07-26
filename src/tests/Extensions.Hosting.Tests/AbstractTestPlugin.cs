// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using ReactiveMarbles.Extensions.Hosting.Plugins;

namespace Extensions.Hosting.Tests;

/// <summary>An abstract plugin used to test that abstract classes are not discovered.</summary>
public abstract class AbstractTestPlugin : IPlugin
{
    /// <summary>Gets a value indicating whether the plugin received host configuration.</summary>
    public bool WasConfigured { get; private set; }

    /// <inheritdoc />
    public void ConfigureHost(object hostBuilderContext, IServiceCollection serviceCollection)
    {
        WasConfigured = true;
        ConfigureHostCore(hostBuilderContext, serviceCollection);
    }

    /// <summary>Configures services for a concrete test plugin.</summary>
    /// <param name="hostBuilderContext">The host builder context.</param>
    /// <param name="serviceCollection">The service collection.</param>
    protected abstract void ConfigureHostCore(object hostBuilderContext, IServiceCollection serviceCollection);
}
