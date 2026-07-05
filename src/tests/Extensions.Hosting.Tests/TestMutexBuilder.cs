// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReactiveMarbles.Extensions.Hosting.AppServices;

namespace Extensions.Hosting.Tests;

/// <summary>Test implementation of IMutexBuilder for unit testing.</summary>
public class TestMutexBuilder : IMutexBuilder
{
    /// <inheritdoc />
    public string? MutexId { get; set; }

    /// <inheritdoc />
    public bool IsGlobal { get; set; }

    /// <inheritdoc />
    public Action<IHostEnvironment, ILogger>? WhenNotFirstInstance { get; set; }
}
