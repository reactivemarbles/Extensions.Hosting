// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Windows.Threading;
using CP.Extensions.Hosting.UiThread;

namespace CP.Extensions.Hosting.WinForms.Internals;

/// <inheritdoc cref="IWinFormsContext"/>
public class WinFormsContext : BaseUiContext, IWinFormsContext
{
    /// <inheritdoc />
    public bool EnableVisualStyles { get; set; } = true;

    /// <inheritdoc />
    public Dispatcher? Dispatcher { get; set; }
}
