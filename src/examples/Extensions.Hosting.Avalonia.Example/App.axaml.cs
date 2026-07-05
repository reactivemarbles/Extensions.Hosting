// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace Extensions.Hosting.Avalonia.Example;

/// <summary>Represents the main application class for the Avalonia application. Provides the entry point for application initialization and resource loading.</summary>
/// <remarks>Overrides the Initialize method to load XAML resources required for the application's user interface.
/// Ensure that all necessary XAML files are included in the project to enable proper resource loading and application
/// startup.</remarks>
public class App : Application
{
    /// <summary>Initializes the component by loading its associated XAML resources.</summary>
    /// <remarks>This method should be called during the component's initialization phase to ensure that all
    /// XAML-defined resources and controls are properly loaded and available for use.</remarks>
    public override void Initialize() => AvaloniaXamlLoader.Load(this);
}
