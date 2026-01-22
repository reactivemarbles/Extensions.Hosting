// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveMarbles.Extensions.Hosting.Maui;

namespace Extensions.Hosting.Maui.Example;

/// <summary>
/// MainPage.
/// </summary>
public partial class MainPage : ContentPage, IMauiShell
{
    private int _count = 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainPage"/> class.
    /// </summary>
    public MainPage() => InitializeComponent();

    private void OnCounterClicked(object? sender, EventArgs e)
    {
        _count++;

        if (_count == 1)
        {
            CounterBtn.Text = $"Clicked {_count} time";
        }
        else
        {
            CounterBtn.Text = $"Clicked {_count} times";
        }

        SemanticScreenReader.Announce(CounterBtn.Text);
    }

    private async void OnNavigateClicked(object? sender, EventArgs e) => await Navigation.PushAsync(new SecondPage());
}
