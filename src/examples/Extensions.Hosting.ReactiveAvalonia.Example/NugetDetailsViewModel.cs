// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using NuGet.Protocol.Core.Types;
using ReactiveUI;

namespace Extensions.Hosting.ReactiveAvalonia.Example;

/// <summary>Represents package detail information shown in search results.</summary>
public class NugetDetailsViewModel : ReactiveObject
{
    /// <summary>Stores the icon http client value.</summary>
    private static readonly HttpClient IconHttpClient = new();

    /// <summary>Stores the metadata value.</summary>
    private readonly IPackageSearchMetadata _metadata;

    /// <summary>Stores the default url value.</summary>
    private readonly Uri _defaultUrl;

    /// <summary>Stores whether icon loading has started.</summary>
    private int _iconLoadStarted;

    /// <summary>Initializes a new instance of the <see cref="NugetDetailsViewModel"/> class.</summary>
    /// <param name="metadata">The NuGet package metadata.</param>
    public NugetDetailsViewModel(IPackageSearchMetadata metadata)
    {
        _metadata = metadata;
        _defaultUrl = new("https://git.io/fAlfh");

        var projectUrl = ProjectUrl ?? _defaultUrl;
        OpenPage = ReactiveCommand.Create(() => OpenPackagePage(projectUrl));
    }

    /// <summary>Gets the package icon.</summary>
    public Bitmap? Icon
    {
        get
        {
            BeginIconLoad();
            return field;
        }

        private set => _ = this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>Gets the icon URI.</summary>
    public Uri IconUrl => _metadata.IconUrl ?? _defaultUrl;

    /// <summary>Gets the package description.</summary>
    public string Description => _metadata.Description;

    /// <summary>Gets the project URL.</summary>
    public Uri ProjectUrl => _metadata.ProjectUrl;

    /// <summary>Gets the package title.</summary>
    public string Title => _metadata.Title;

    /// <summary>Gets the command that opens the package page.</summary>
    public ReactiveCommand<Unit, Unit> OpenPage { get; }

    /// <summary>Loads the icon bitmap from the supplied URI.</summary>
    /// <param name="iconUrl">The icon URI.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The loaded bitmap, or null when the icon cannot be loaded.</returns>
    private static async Task<Bitmap?> LoadIconAsync(Uri iconUrl, CancellationToken cancellationToken)
    {
        try
        {
            await using var iconStream = await IconHttpClient.GetStreamAsync(iconUrl, cancellationToken).ConfigureAwait(false);
            await using var iconMemoryStream = new MemoryStream();
            await iconStream.CopyToAsync(iconMemoryStream, cancellationToken).ConfigureAwait(false);
            iconMemoryStream.Position = 0;
            return new(iconMemoryStream);
        }
        catch (HttpRequestException)
        {
            return null;
        }
        catch (TaskCanceledException)
        {
            return null;
        }
        catch (ArgumentException)
        {
            return null;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
        catch (InvalidDataException)
        {
            return null;
        }
        catch (NotSupportedException)
        {
            return null;
        }
    }

    /// <summary>Opens the package page in the default system browser.</summary>
    /// <param name="projectUrl">The package project URL.</param>
    private static void OpenPackagePage(Uri projectUrl)
    {
        if (!string.Equals(projectUrl.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(projectUrl.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (OperatingSystem.IsWindows())
        {
            var windowsStartInfo = new ProcessStartInfo("explorer.exe") { UseShellExecute = false };
            windowsStartInfo.ArgumentList.Add(projectUrl.AbsoluteUri);
            _ = Process.Start(windowsStartInfo);
            return;
        }

        if (OperatingSystem.IsMacOS())
        {
            var macOsStartInfo = new ProcessStartInfo("/usr/bin/open") { UseShellExecute = false };
            macOsStartInfo.ArgumentList.Add(projectUrl.AbsoluteUri);
            _ = Process.Start(macOsStartInfo);
            return;
        }

        var linuxStartInfo = new ProcessStartInfo("xdg-open") { UseShellExecute = false };
        linuxStartInfo.ArgumentList.Add(projectUrl.AbsoluteUri);
        _ = Process.Start(linuxStartInfo);
    }

    /// <summary>Starts loading the package icon the first time it is requested.</summary>
    private void BeginIconLoad()
    {
        if (Interlocked.Exchange(ref _iconLoadStarted, 1) != 0)
        {
            return;
        }

        _ = LoadAndPublishIconAsync();
    }

    /// <summary>Loads the package icon and publishes it to the binding.</summary>
    /// <returns>A task that represents the asynchronous load.</returns>
    private async Task LoadAndPublishIconAsync() => Icon = await LoadIconAsync(IconUrl, CancellationToken.None);
}
