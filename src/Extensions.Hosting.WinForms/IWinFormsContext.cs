// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows.Threading;
using ReactiveMarbles.Extensions.Hosting.UiThread;

namespace ReactiveMarbles.Extensions.Hosting.WinForms;

/// <summary>
/// The WinForms context contains all information about the WinForms application and how it's started and stopped.
/// </summary>
public interface IWinFormsContext : IUiContext
{
    /// <summary>
    /// Gets or sets a value indicating whether specify if the visual styles need to be set, default is true.
    /// </summary>
    bool EnableVisualStyles { get; set; }

    /// <summary>
    /// Gets or sets the dispatcher to send information to forms.
    /// </summary>
    Dispatcher? Dispatcher { get; set; }
}
