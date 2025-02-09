// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ReactiveMarbles.Extensions.Hosting.AppServices.Internal;

/// <summary>
/// This is the configuration for the mutex service.
/// </summary>
internal class MutexBuilder : IMutexBuilder
{
    /// <inheritdoc />
    public string? MutexId { get; set; }

    /// <inheritdoc />
    public bool IsGlobal { get; set; }

    /// <inheritdoc />
    public Action<IHostEnvironment, ILogger>? WhenNotFirstInstance { get; set; }
}
