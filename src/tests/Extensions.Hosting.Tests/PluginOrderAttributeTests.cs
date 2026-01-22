// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using ReactiveMarbles.Extensions.Hosting.Plugins;
using TUnit;

namespace Extensions.Hosting.Tests;

/// <summary>
/// Contains unit tests for the PluginOrderAttribute class.
/// </summary>
public class PluginOrderAttributeTests
{
    /// <summary>
    /// Verifies that the default constructor sets Order to 0.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task DefaultConstructor_SetsOrderToZero()
    {
        var attribute = new PluginOrderAttribute();
        await Assert.That(attribute.Order).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies that the int constructor sets the Order property correctly.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task IntConstructor_SetsOrderCorrectly()
    {
        var attribute = new PluginOrderAttribute(42);
        await Assert.That(attribute.Order).IsEqualTo(42);
    }

    /// <summary>
    /// Verifies that the int constructor handles negative values.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task IntConstructor_WithNegativeValue_SetsOrderCorrectly()
    {
        var attribute = new PluginOrderAttribute(-10);
        await Assert.That(attribute.Order).IsEqualTo(-10);
    }

    /// <summary>
    /// Verifies that the enum constructor converts the enum to its int value.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task EnumConstructor_ConvertsToIntValue()
    {
        var attribute = new PluginOrderAttribute(TestOrder.High);
        await Assert.That(attribute.Order).IsEqualTo((int)TestOrder.High);
    }

    /// <summary>
    /// Verifies that the attribute can be retrieved from a decorated class.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task Attribute_CanBeRetrievedFromClass()
    {
        var type = typeof(OrderedTestPlugin);
        var attribute = (PluginOrderAttribute?)System.Attribute.GetCustomAttribute(type, typeof(PluginOrderAttribute));
        await Assert.That(attribute).IsNotNull();
        await Assert.That(attribute!.Order).IsEqualTo(100);
    }
}
