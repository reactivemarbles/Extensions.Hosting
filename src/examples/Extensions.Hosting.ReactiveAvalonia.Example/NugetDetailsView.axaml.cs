// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Avalonia;

namespace Extensions.Hosting.ReactiveAvalonia.Example;

/// <summary>
/// Represents the view used to display a single NuGet package search result.
/// </summary>
public partial class NugetDetailsView : ReactiveUserControl<NugetDetailsViewModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NugetDetailsView"/> class.
    /// </summary>
    public NugetDetailsView() => InitializeComponent();
}
