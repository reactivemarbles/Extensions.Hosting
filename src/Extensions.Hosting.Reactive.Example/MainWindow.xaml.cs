// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Disposables.Fluent;
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
    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
        ViewModel = new AppViewModel();

        this.WhenActivated(disposableRegistration =>
        {
            this.OneWayBind(
                    ViewModel,
                    viewModel => viewModel.IsAvailable,
                    view => view.SearchResultsListBox.Visibility)
                .DisposeWith(disposableRegistration);

            this.OneWayBind(
                    ViewModel,
                    viewModel => viewModel.SearchResults,
                    view => view.SearchResultsListBox.ItemsSource)
                .DisposeWith(disposableRegistration);

            this.Bind(
                    ViewModel,
                    viewModel => viewModel.SearchTerm,
                    view => view.SearchTextBox.Text)
                .DisposeWith(disposableRegistration);
        });
    }
}
