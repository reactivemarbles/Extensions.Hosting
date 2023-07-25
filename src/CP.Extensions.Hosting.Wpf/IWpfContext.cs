// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Windows;
using System.Windows.Threading;
using CP.Extensions.Hosting.UiThread;

namespace CP.Extensions.Hosting.Wpf;

/// <summary>
/// The WPF context contains all information about the WPF application and how it's started and stopped.
/// </summary>
public interface IWpfContext : IUiContext
{
    /// <summary>
    /// Gets or sets this is the WPF ShutdownMode used for the WPF application lifetime, default is OnLastWindowClose.
    /// </summary>
    ShutdownMode ShutdownMode { get; set; }

    /// <summary>
    /// Gets or sets the Application.
    /// </summary>
    Application WpfApplication { get; set; }

    /// <summary>
    /// Gets this WPF dispatcher.
    /// </summary>
    Dispatcher Dispatcher { get; }
}
