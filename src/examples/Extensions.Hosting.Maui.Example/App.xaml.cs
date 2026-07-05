// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace Extensions.Hosting.Maui.Example;

/// <summary>Represents the sample MAUI application.</summary>
/// <seealso cref="Microsoft.Maui.Controls.Application" />
public partial class App : Application
{
    /// <summary>Initializes a new instance of the <see cref="App"/> class.</summary>
    public App()
    {
        InitializeComponent();

        // Start the host
        _ = MauiProgram.HostApp.StartAsync();
    }

    /// <summary>Creates the window.</summary>
    /// <param name="activationState">State of the activation.</param>
    /// <returns>A Window.</returns>
    protected override Window CreateWindow(IActivationState? activationState) => new(new AppShell());
}
