// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace Extensions.Hosting.Tests;

/// <summary>
/// Test enumeration for plugin ordering.
/// </summary>
public enum TestOrder
{
    /// <summary>Low priority.</summary>
    Low = 0,

    /// <summary>Medium priority.</summary>
    Medium = 50,

    /// <summary>High priority.</summary>
    High = 100
}
