// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;

namespace Extensions.Hosting.Tests;

/// <summary>
/// A concrete plugin derived from an abstract plugin.
/// </summary>
public class ConcreteTestPlugin : AbstractTestPlugin
{
    /// <inheritdoc />
    public override void ConfigureHost(object hostBuilderContext, IServiceCollection serviceCollection)
    {
    }
}
