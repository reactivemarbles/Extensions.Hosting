// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveMarbles.Extensions.Hosting.UiThread;

/// <summary>
/// The UI context contains all information about a UI application and how it's started and stopped.
/// </summary>
public interface IUiContext
{
    /// <summary>
    /// Gets or sets a value indicating whether defines if the host application is stopped when the UI applications stops.
    /// </summary>
    bool IsLifetimeLinked { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether is the WPF application started and still running?.
    /// </summary>
    bool IsRunning { get; set; }
}
