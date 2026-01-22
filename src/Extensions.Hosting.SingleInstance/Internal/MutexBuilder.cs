// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ReactiveMarbles.Extensions.Hosting.AppServices.Internal;

/// <summary>
/// Provides a builder for configuring and creating mutex instances used to coordinate application instance exclusivity.
/// </summary>
/// <remarks>This class is intended for internal use to facilitate the construction of mutexes that manage
/// single-instance application scenarios. It exposes properties for specifying the mutex identifier, scope, and actions
/// to perform when the current process is not the first instance.</remarks>
internal class MutexBuilder : IMutexBuilder
{
    /// <inheritdoc />
    public string? MutexId { get; set; }

    /// <inheritdoc />
    public bool IsGlobal { get; set; }

    /// <inheritdoc />
    public Action<IHostEnvironment, ILogger>? WhenNotFirstInstance { get; set; }
}
