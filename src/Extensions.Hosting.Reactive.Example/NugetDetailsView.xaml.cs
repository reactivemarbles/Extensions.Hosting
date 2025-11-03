// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Disposables.Fluent;
using System.Windows.Media.Imaging;
using ReactiveUI;

namespace Extensions.Hosting.Reactive.Example;

/// <summary>
/// Second level derived class off ReactiveUserControl which contains the ViewModel property.
/// In our MainWindow when we register the ListBox with the collection of
/// NugetDetailsViewModels if no ItemTemplate has been declared it will search for
/// a class derived off IViewFor NugetDetailsViewModel and show that for the item.
/// </summary>
/// <seealso cref="System.Windows.Markup.IComponentConnector" />
public partial class NugetDetailsView : ReactiveUserControl<NugetDetailsViewModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NugetDetailsView"/> class.
    /// </summary>
    public NugetDetailsView()
    {
        InitializeComponent();
        this.WhenActivated(disposableRegistration =>
        {
            // Our 4th parameter we convert from Url into a BitmapImage.
            // This is an easy way of doing value conversion using ReactiveUI binding.
            this.OneWayBind(
                    ViewModel,
                    viewModel => viewModel.IconUrl,
                    view => view.IconImage.Source,
                    url => url == null ? null : new BitmapImage(url))
                .DisposeWith(disposableRegistration);

            this.OneWayBind(
                    ViewModel,
                    viewModel => viewModel.Title,
                    view => view.TitleRun.Text)
                .DisposeWith(disposableRegistration);

            this.OneWayBind(
                    ViewModel,
                    viewModel => viewModel.Description,
                    view => view.DescriptionRun.Text)
                .DisposeWith(disposableRegistration);

            this.BindCommand(
                    ViewModel,
                    viewModel => viewModel.OpenPage,
                    view => view.OpenButton)
                .DisposeWith(disposableRegistration);
        });
    }
}
