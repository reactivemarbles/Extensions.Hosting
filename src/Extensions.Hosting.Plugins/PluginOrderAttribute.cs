// Copyright (c) 2019-2024 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

namespace ReactiveMarbles.Extensions.Hosting.Plugins;

/// <summary>
///     Use this attribute to specify the order for loading plug-ins.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class PluginOrderAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PluginOrderAttribute"/> class.
    /// Default value.
    /// </summary>
    public PluginOrderAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginOrderAttribute" /> class.
    /// Specify the order of the plug-in initialization.
    /// </summary>
    /// <param name="order">The order.</param>
    public PluginOrderAttribute(int order) => Order = order;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginOrderAttribute"/> class.
    /// Specify the order by using enums.
    /// </summary>
    /// <param name="enumValue">object which can be converted to an int.</param>
    public PluginOrderAttribute(object enumValue)
        : this(Convert.ToInt32(enumValue))
    {
    }

    /// <summary>
    /// Gets order to initialize the plug-in.
    /// </summary>
    public int Order { get; }
}
