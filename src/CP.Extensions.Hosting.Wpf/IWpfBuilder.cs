// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Windows;

namespace CP.Extensions.Hosting.Wpf;

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
