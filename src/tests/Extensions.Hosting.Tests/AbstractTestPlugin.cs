// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using ReactiveMarbles.Extensions.Hosting.Plugins;

namespace Extensions.Hosting.Tests;

/// <summary>An abstract plugin used to test that abstract classes are not discovered.</summary>
public abstract class AbstractTestPlugin : IPlugin
{
    /// <inheritdoc />
    public abstract void ConfigureHost(object hostBuilderContext, IServiceCollection serviceCollection);
}
