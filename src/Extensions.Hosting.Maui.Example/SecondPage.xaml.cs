// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace Extensions.Hosting.Maui.Example;

/// <summary>
/// SecondPage.
/// </summary>
/// <seealso cref="Microsoft.Maui.Controls.ContentPage" />
public partial class SecondPage : ContentPage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SecondPage"/> class.
    /// </summary>
    public SecondPage() => InitializeComponent();

    private async void OnNavigateClicked(object? sender, EventArgs e) => await Navigation.PopAsync();
}
