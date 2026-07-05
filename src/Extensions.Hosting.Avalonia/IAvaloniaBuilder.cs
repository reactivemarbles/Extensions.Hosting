// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Avalonia;

namespace ReactiveMarbles.Extensions.Hosting.Avalonia;

/// <summary>Provides a contract for configuring an Avalonia application, including application type, existing application instance, window types, and context and builder configuration actions.</summary>
/// <remarks>Implementations of this interface allow developers to set up the Avalonia application environment by
/// specifying the application type, supplying an existing application instance, defining window types, and providing
/// actions to configure both the Avalonia context and the AppBuilder. This enables flexible customization of the
/// application startup process and environment.</remarks>
public interface IAvaloniaBuilder
{
    /// <summary>Gets or sets type of the application that will be used.</summary>
    Type? ApplicationType { get; set; }

    /// <summary>Gets or sets an existing application.</summary>
    Application? Application { get; set; }

    /// <summary>Gets type of the windows that will be used.</summary>
    IList<Type> WindowTypes { get; }

    /// <summary>Gets or sets action to configure the Avalonia context.</summary>
    Action<IAvaloniaContext>? ConfigureContextAction { get; set; }

    /// <summary>Gets or sets action to configure the AppBuilder.</summary>
    Action<AppBuilder>? ConfigureAppBuilderAction { get; set; }
}
