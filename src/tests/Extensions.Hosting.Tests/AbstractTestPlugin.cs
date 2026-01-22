// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using ReactiveMarbles.Extensions.Hosting.Plugins;

namespace Extensions.Hosting.Tests;

/// <summary>
/// An abstract plugin used to test that abstract classes are not discovered.
/// </summary>
public abstract class AbstractTestPlugin : IPlugin
{
    /// <inheritdoc />
    public abstract void ConfigureHost(object hostBuilderContext, IServiceCollection serviceCollection);
}
