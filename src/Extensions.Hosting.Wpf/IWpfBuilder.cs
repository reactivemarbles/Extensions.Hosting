// Copyright (c) 2019-2024 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Windows;

namespace ReactiveMarbles.Extensions.Hosting.Wpf;

/// <summary>
/// Interface used for configuring Wpf.
/// </summary>
public interface IWpfBuilder
{
    /// <summary>
    /// Gets or sets type of the application that will be used.
    /// </summary>
    Type? ApplicationType { get; set; }
    /// <summary>
    /// Gets or sets an existing application.
    /// </summary>
    Application? Application { get; set; }
    /// <summary>
    /// Gets type of the windows that will be used.
    /// </summary>
    IList<Type> WindowTypes { get; }
    /// <summary>
    /// Gets or sets action to configure the Wpf context.
    /// </summary>
    Action<IWpfContext>? ConfigureContextAction { get; set; }
}
