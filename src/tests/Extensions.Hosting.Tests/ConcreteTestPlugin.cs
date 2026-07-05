// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;

namespace Extensions.Hosting.Tests;

/// <summary>A concrete plugin derived from an abstract plugin.</summary>
public class ConcreteTestPlugin : AbstractTestPlugin
{
    /// <inheritdoc />
    public override void ConfigureHost(object hostBuilderContext, IServiceCollection serviceCollection)
    {
    }
}
