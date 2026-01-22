// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveMarbles.Extensions.Hosting.UiThread;

/// <summary>
/// Defines the contract for managing the lifecycle and running state of a user interface (UI) application within a host
/// environment.
/// </summary>
/// <remarks>Implementations of this interface allow coordination between the UI application's lifecycle and the
/// host application's lifecycle. This is useful for scenarios where the UI and host application need to be started,
/// stopped, or monitored in tandem.</remarks>
public interface IUiContext
{
    /// <summary>
    /// Gets or sets a value indicating whether defines if the host application is stopped when the UI applications stops.
    /// </summary>
    bool IsLifetimeLinked { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether is the application started and still running?.
    /// </summary>
    bool IsRunning { get; set; }
}
