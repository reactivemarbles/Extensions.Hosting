// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ReactiveMarbles.Extensions.Hosting.AppServices;

/// <summary>
/// This is to configure the ForceSingleInstance extension.
/// </summary>
public interface IMutexBuilder
{
    /// <summary>
    /// Gets or sets the name of the mutex, usually a GUID.
    /// </summary>
    string? MutexId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this decides what prefix the mutex name gets, true will prepend Global\ and false Local\.
    /// </summary>
    bool IsGlobal { get; set; }

    /// <summary>
    /// Gets or sets the action which is called when the mutex cannot be locked.
    /// </summary>
    Action<IHostEnvironment, ILogger>? WhenNotFirstInstance { get; set; }
}
