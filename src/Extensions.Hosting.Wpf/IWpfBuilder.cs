// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Windows;

namespace ReactiveMarbles.Extensions.Hosting.Wpf;

/// <summary>
/// Defines a contract for configuring and building a WPF application, including application type, application instance,
/// window types, and context configuration actions.
/// </summary>
/// <remarks>Implementations of this interface allow customization of WPF application startup and context
/// configuration. Use the provided properties to specify the application type or instance, register window types, and
/// supply additional context configuration logic as needed.</remarks>
public interface IWpfBuilder
{
    /// <summary>
    /// Gets or sets the type of the application associated with this instance.
    /// </summary>
    Type? ApplicationType { get; set; }

    /// <summary>
    /// Gets or sets the application associated with the current context.
    /// </summary>
    Application? Application { get; set; }

    /// <summary>
    /// Gets the collection of window types supported by the application.
    /// </summary>
    /// <remarks>The returned list contains the types that can be instantiated as windows within the
    /// application. The order of types in the list may be significant depending on the application's usage.</remarks>
    IList<Type> WindowTypes { get; }

    /// <summary>
    /// Gets or sets an action that configures the provided WPF context before it is used.
    /// </summary>
    /// <remarks>Use this property to supply custom logic for initializing or modifying the WPF context. The
    /// action is invoked with the context instance, allowing you to set properties or perform setup tasks as needed. If
    /// not set, no additional configuration is applied.</remarks>
    Action<IWpfContext>? ConfigureContextAction { get; set; }
}
