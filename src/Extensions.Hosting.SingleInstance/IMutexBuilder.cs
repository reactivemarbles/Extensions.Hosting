// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ReactiveMarbles.Extensions.Hosting.AppServices;

/// <summary>
/// Defines a builder for configuring and creating a named mutex, typically used to enforce single-instance application
/// behavior.
/// </summary>
/// <remarks>Implementations of this interface allow customization of the mutex name, scope (global or local), and
/// the action to perform when another instance is already running. This is commonly used in scenarios where only one
/// instance of an application should be active at a time.</remarks>
public interface IMutexBuilder
{
    /// <summary>
    /// Gets or sets the unique identifier for the mutex associated with the current operation or resource.
    /// </summary>
    string? MutexId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the setting applies globally to all users or contexts.
    /// </summary>
    bool IsGlobal { get; set; }

    /// <summary>
    /// Gets or sets the action to execute when the current process is not the first instance of the application.
    /// </summary>
    /// <remarks>This action is invoked with the application's host environment and logger as parameters. Use
    /// this property to define custom behavior, such as notifying the user or logging a message, when a subsequent
    /// instance of the application is detected.</remarks>
    Action<IHostEnvironment, ILogger>? WhenNotFirstInstance { get; set; }
}
