// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using ReactiveMarbles.Extensions.Hosting.Plugins;

namespace Extensions.Hosting.Tests;

/// <summary>A test plugin implementation for unit testing purposes.</summary>
public class TestPlugin : IPlugin
{
    /// <summary>Gets a value indicating whether ConfigureHost was called.</summary>
    public bool WasConfigured { get; private set; }

    /// <inheritdoc />
    public void ConfigureHost(object hostBuilderContext, IServiceCollection serviceCollection) => WasConfigured = true;
}
