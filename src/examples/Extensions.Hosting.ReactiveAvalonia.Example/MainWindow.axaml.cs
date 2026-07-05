// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveMarbles.Extensions.Hosting.Avalonia;
using ReactiveUI.Avalonia;

namespace Extensions.Hosting.ReactiveAvalonia.Example;

/// <summary>Represents the main application window for the reactive NuGet browser sample.</summary>
public partial class MainWindow : ReactiveWindow<AppViewModel>, IAvaloniaShell
{
    /// <summary>Initializes a new instance of the <see cref="MainWindow"/> class.</summary>
    public MainWindow()
    {
        InitializeComponent();
        ViewModel = new();
        DataContext = ViewModel;
    }
}
