// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows.Threading;
using ReactiveMarbles.Extensions.Hosting.UiThread;

namespace ReactiveMarbles.Extensions.Hosting.WinForms;

/// <summary>
/// Defines a context for Windows Forms operations, providing properties for managing visual styles and dispatching
/// actions to forms.
/// </summary>
/// <remarks>Implementations of this interface enable configuration of UI behavior specific to Windows Forms
/// applications, such as enabling visual styles and specifying a dispatcher for thread-safe operations.</remarks>
public interface IWinFormsContext : IUiContext
{
    /// <summary>
    /// Gets or sets a value indicating whether visual styles are enabled for the application.
    /// </summary>
    /// <remarks>When visual styles are enabled, controls are rendered with the current Windows theme,
    /// providing a modern appearance. Disabling visual styles may cause controls to appear with classic styling. This
    /// property should typically be set before creating any UI elements.</remarks>
    bool EnableVisualStyles { get; set; }

    /// <summary>
    /// Gets or sets the dispatcher used to manage the execution of operations on a specific thread or context.
    /// </summary>
    Dispatcher? Dispatcher { get; set; }
}
