// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveMarbles.Extensions.Hosting.Wpf;
using ReactiveUI;

namespace Extensions.Hosting.Reactive.Example;

/// <summary>
/// MainWindow class derives off ReactiveWindow which implements the IViewFor&lt;TViewModel&gt;
/// interface using a WPF DependencyProperty. We need this to use WhenActivated extension
/// method that helps us handling View and ViewModel activation and deactivation.
/// </summary>
/// <seealso cref="IWpfShell" />
/// <seealso cref="System.Windows.Markup.IComponentConnector" />
public partial class MainWindow : ReactiveWindow<AppViewModel>, IWpfShell
{
    /// <summary>Initializes a new instance of the <see cref="MainWindow"/> class.</summary>
    public MainWindow()
    {
        InitializeComponent();
        ViewModel = new();

        _ = this.WhenActivated(disposableRegistration =>
        {
            _ = this.OneWayBind(
                    ViewModel,
                    viewModel => viewModel.IsAvailable,
                    view => view.SearchResultsListBox.Visibility)
                .DisposeWith(disposableRegistration);

            _ = this.OneWayBind(
                    ViewModel,
                    viewModel => viewModel.SearchResults,
                    view => view.SearchResultsListBox.ItemsSource)
                .DisposeWith(disposableRegistration);

            _ = this.Bind(
                    ViewModel,
                    viewModel => viewModel.SearchTerm,
                    view => view.SearchTextBox.Text)
                .DisposeWith(disposableRegistration);
        });
    }
}
