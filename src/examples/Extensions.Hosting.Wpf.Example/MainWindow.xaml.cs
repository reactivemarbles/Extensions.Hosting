// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ReactiveMarbles.Extensions.Hosting.Wpf;

namespace Extensions.Hosting.Wpf.Example;

/// <summary>Interaction logic for MainWindow.xaml.</summary>
public partial class MainWindow : IWpfShell
{
    /// <summary>Logs that the second window is opening.</summary>
    private static readonly Action<ILogger, Exception?> OpeningSecondWindow =
        LoggerMessage.Define(LogLevel.Information, new EventId(1, nameof(OpenSecondWindow)), "Opening Second Window");

    /// <summary>Stores the logger value.</summary>
    private readonly ILogger<MainWindow>? _logger;

    /// <summary>Stores the service provider value.</summary>
    private readonly IServiceProvider? _serviceProvider;

    /// <summary>Initializes a new instance of the <see cref="MainWindow" /> class.</summary>
    /// <param name="logger">The logger.</param>
    /// <param name="serviceProvider">The service provider.</param>
    public MainWindow(ILogger<MainWindow> logger, IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    /// <summary>Handles opening the second window.</summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    protected void OpenSecondWindow(object sender, System.Windows.RoutedEventArgs e)
    {
        var window = _serviceProvider?.GetService<SecondWindow>();
        if (_logger is not null)
        {
            OpeningSecondWindow(_logger, null);
        }

        window?.Show();
    }
}
