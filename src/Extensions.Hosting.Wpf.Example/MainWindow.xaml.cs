// Copyright (c) 2019-2024 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ReactiveMarbles.Extensions.Hosting.Wpf;

namespace Extensions.Hosting.Wpf.Example;

/// <summary>
/// Interaction logic for MainWindow.xaml.
/// </summary>
public partial class MainWindow : IWpfShell
{
    private readonly ILogger<MainWindow>? _logger;
    private readonly IServiceProvider? _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow" /> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="serviceProvider">The service provider.</param>
    public MainWindow(ILogger<MainWindow> logger, IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    private void OpenSecondWindow(object sender, System.Windows.RoutedEventArgs e)
    {
        var window = _serviceProvider?.GetService<SecondWindow>();
        _logger?.LogInformation("Opening Second Window");
        window?.Show();
    }
}
