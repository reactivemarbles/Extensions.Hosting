// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows.Threading;
using ReactiveMarbles.Extensions.Hosting.UiThread;

namespace ReactiveMarbles.Extensions.Hosting.WinForms.Internals;

/// <summary>
/// Provides a Windows Forms-specific UI context for managing application-wide settings and services.
/// </summary>
/// <remarks>Use this class to configure and interact with Windows Forms application features, such as enabling
/// visual styles and accessing the UI dispatcher. This context is typically used to coordinate UI-related operations in
/// a Windows Forms environment.</remarks>
public class WinFormsContext : BaseUiContext, IWinFormsContext
{
    /// <inheritdoc />
    public bool EnableVisualStyles { get; set; } = true;

    /// <inheritdoc />
    public Dispatcher? Dispatcher { get; set; }
}
