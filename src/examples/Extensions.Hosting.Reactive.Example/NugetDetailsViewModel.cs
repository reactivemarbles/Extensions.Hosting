// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Reactive;
using NuGet.Protocol.Core.Types;
using ReactiveUI;

namespace Extensions.Hosting.Reactive.Example;

/// <summary>
/// NugetDetailsViewModel.
/// </summary>
/// <seealso cref="ReactiveObject" />
public class NugetDetailsViewModel : ReactiveObject
{
    private readonly IPackageSearchMetadata _metadata;
    private readonly Uri _defaultUrl;

    /// <summary>
    /// Initializes a new instance of the <see cref="NugetDetailsViewModel"/> class.
    /// </summary>
    /// <param name="metadata">The metadata.</param>
    public NugetDetailsViewModel(IPackageSearchMetadata metadata)
    {
        _metadata = metadata;
        _defaultUrl = new Uri("https://git.io/fAlfh");

        var startInfo = new ProcessStartInfo(ProjectUrl?.ToString() ?? _defaultUrl.ToString())
        {
            UseShellExecute = true
        };
#pragma warning disable IDE0053 // Use expression body for lambda expression
#pragma warning disable RCS1021 // Convert lambda expression body to expression body
        OpenPage = ReactiveCommand.Create(() => { Process.Start(startInfo); });
#pragma warning restore RCS1021 // Convert lambda expression body to expression body
#pragma warning restore IDE0053 // Use expression body for lambda expression
    }

    /// <summary>
    /// Gets the icon URL.
    /// </summary>
    /// <value>
    /// The icon URL.
    /// </value>
    public Uri IconUrl => _metadata.IconUrl ?? _defaultUrl;

    /// <summary>
    /// Gets the description.
    /// </summary>
    /// <value>
    /// The description.
    /// </value>
    public string Description => _metadata.Description;

    /// <summary>
    /// Gets the project URL.
    /// </summary>
    /// <value>
    /// The project URL.
    /// </value>
    public Uri ProjectUrl => _metadata.ProjectUrl;

    /// <summary>
    /// Gets the title.
    /// </summary>
    /// <value>
    /// The title.
    /// </value>
    public string Title => _metadata.Title;

    /// <summary>
    /// Gets the open page.
    /// </summary>
    /// <value>
    /// The open page.
    /// </value>
    public ReactiveCommand<Unit, Unit> OpenPage { get; }
}
