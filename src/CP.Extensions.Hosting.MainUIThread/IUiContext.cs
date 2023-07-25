// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace CP.Extensions.Hosting.UiThread;

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
