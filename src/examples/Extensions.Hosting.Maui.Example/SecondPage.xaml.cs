// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace Extensions.Hosting.Maui.Example;

/// <summary>Represents the second sample MAUI page.</summary>
/// <seealso cref="Microsoft.Maui.Controls.ContentPage" />
public partial class SecondPage : ContentPage
{
    /// <summary>Initializes a new instance of the <see cref="SecondPage"/> class.</summary>
    public SecondPage() => InitializeComponent();

    /// <summary>Handles navigation back to the previous page.</summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    protected async void OnNavigateClicked(object? sender, EventArgs e) => await Navigation.PopAsync();
}
