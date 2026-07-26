// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using NuGet.Protocol.Core.Types;
using ReactiveUI;

namespace Extensions.Hosting.Reactive.Example;

/// <summary>Represents package detail information shown in search results.</summary>
/// <seealso cref="ReactiveObject" />
public class NugetDetailsViewModel : ReactiveObject
{
    /// <summary>Stores the metadata value.</summary>
    private readonly IPackageSearchMetadata _metadata;

    /// <summary>Stores the default url value.</summary>
    private readonly Uri _defaultUrl;

    /// <summary>Initializes a new instance of the <see cref="NugetDetailsViewModel"/> class.</summary>
    /// <param name="metadata">The metadata.</param>
    public NugetDetailsViewModel(IPackageSearchMetadata metadata)
    {
        _metadata = metadata;
        _defaultUrl = new("https://git.io/fAlfh");

        var projectUrl = ProjectUrl ?? _defaultUrl;
        OpenPage = ReactiveCommand.Create(() => OpenPackagePage(projectUrl));
    }

    /// <summary>Gets the icon URL.</summary>
    /// <value>
    /// The icon URL.
    /// </value>
    public Uri IconUrl => _metadata.IconUrl ?? _defaultUrl;

    /// <summary>Gets the description.</summary>
    /// <value>
    /// The description.
    /// </value>
    public string Description => _metadata.Description;

    /// <summary>Gets the project URL.</summary>
    /// <value>
    /// The project URL.
    /// </value>
    public Uri ProjectUrl => _metadata.ProjectUrl;

    /// <summary>Gets the title.</summary>
    /// <value>
    /// The title.
    /// </value>
    public string Title => _metadata.Title;

    /// <summary>Gets the open page.</summary>
    /// <value>
    /// The open page.
    /// </value>
    public ReactiveCommand<Unit, Unit> OpenPage { get; }

    /// <summary>Opens the package page in the default system browser.</summary>
    /// <param name="projectUrl">The package project URL.</param>
    private static void OpenPackagePage(Uri projectUrl)
    {
        if (!string.Equals(projectUrl.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(projectUrl.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var startInfo = new ProcessStartInfo("explorer.exe") { Arguments = projectUrl.AbsoluteUri, UseShellExecute = false };
        _ = Process.Start(startInfo);
    }
}
