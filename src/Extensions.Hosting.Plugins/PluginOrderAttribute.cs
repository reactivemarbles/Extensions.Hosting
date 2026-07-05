// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

namespace ReactiveMarbles.Extensions.Hosting.Plugins;

/// <summary>Specifies the initialization order for a plug-in class.</summary>
/// <remarks>Apply this attribute to a plug-in class to control the sequence in which plug-ins are initialized.
/// Lower order values indicate earlier initialization. If multiple plug-ins have the same order, their relative
/// initialization order is unspecified.</remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class PluginOrderAttribute : Attribute
{
    /// <summary>Initializes a new instance of the <see cref="PluginOrderAttribute"/> class.</summary>
    public PluginOrderAttribute()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="PluginOrderAttribute"/> class with the specified order value.</summary>
    /// <param name="order">The order in which the plugin should be processed. Lower values indicate higher priority.</param>
    public PluginOrderAttribute(int order) => Order = order;

    /// <summary>Initializes a new instance of the <see cref="PluginOrderAttribute"/> class using the specified enumeration value.</summary>
    /// <remarks>This constructor allows specifying the plugin order using an enumeration, which is internally
    /// converted to an integer. This can improve code readability and maintainability when plugin order is defined by
    /// an enum.</remarks>
    /// <param name="enumValue">An enumeration value that determines the order in which the plugin is processed. The value is converted to its
    /// underlying integer representation.</param>
    public PluginOrderAttribute(object enumValue)
        : this(Convert.ToInt32(enumValue))
    {
        EnumValue = enumValue;
    }

    /// <summary>Gets the enumeration value supplied to the attribute constructor, if one was supplied.</summary>
    public object? EnumValue { get; }

    /// <summary>Gets the order or position of the item within a collection or sequence.</summary>
    public int Order { get; }
}
