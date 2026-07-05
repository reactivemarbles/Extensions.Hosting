// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Avalonia;

namespace Extensions.Hosting.ReactiveAvalonia.Example;

/// <summary>Represents the view used to display a single NuGet package search result.</summary>
public partial class NugetDetailsView : ReactiveUserControl<NugetDetailsViewModel>
{
    /// <summary>Initializes a new instance of the <see cref="NugetDetailsView"/> class.</summary>
    public NugetDetailsView() => InitializeComponent();
}
