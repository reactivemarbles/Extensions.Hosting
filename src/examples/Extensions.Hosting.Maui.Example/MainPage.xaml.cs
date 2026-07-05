// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveMarbles.Extensions.Hosting.Maui;

namespace Extensions.Hosting.Maui.Example;

/// <summary>Represents the main sample MAUI page.</summary>
public partial class MainPage : ContentPage, IMauiShell
{
    /// <summary>Stores the count value.</summary>
    private int _count;

    /// <summary>Initializes a new instance of the <see cref="MainPage"/> class.</summary>
    public MainPage() => InitializeComponent();

    /// <summary>Handles counter button clicks.</summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    protected void OnCounterClicked(object? sender, EventArgs e)
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

    /// <summary>Handles navigation to the second page.</summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    protected async void OnNavigateClicked(object? sender, EventArgs e) => await Navigation.PushAsync(new SecondPage());
}
