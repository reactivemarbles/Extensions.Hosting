// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Configuration;
using NuGet.Protocol.Core.Types;
using ReactiveUI;

namespace Extensions.Hosting.ReactiveAvalonia.Example;

/// <summary>
/// Represents the root view model for the sample NuGet browser application.
/// </summary>
public class AppViewModel : ReactiveObject
{
    private readonly ObservableAsPropertyHelper<IEnumerable<NugetDetailsViewModel>> _searchResults;
    private readonly ObservableAsPropertyHelper<bool> _isAvailable;
    private string? _searchTerm;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppViewModel"/> class.
    /// </summary>
    public AppViewModel()
    {
        _searchResults = this
            .WhenAnyValue(x => x.SearchTerm)
            .Throttle(TimeSpan.FromMilliseconds(800))
            .Select(term => term?.Trim())
            .DistinctUntilChanged()
            .Where(term => !string.IsNullOrWhiteSpace(term))
            .SelectMany(SearchNuGetPackages)
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .ToProperty(this, x => x.SearchResults);

        _searchResults.ThrownExceptions.Subscribe(error => { });

        _isAvailable = this
            .WhenAnyValue(x => x.SearchResults)
            .Select(searchResults => searchResults != null)
            .ToProperty(this, x => x.IsAvailable);
    }

    /// <summary>
    /// Gets or sets the search term.
    /// </summary>
    public string? SearchTerm
    {
        get => _searchTerm;
        set => this.RaiseAndSetIfChanged(ref _searchTerm, value);
    }

    /// <summary>
    /// Gets the search results.
    /// </summary>
    public IEnumerable<NugetDetailsViewModel> SearchResults => _searchResults.Value;

    /// <summary>
    /// Gets a value indicating whether results are available.
    /// </summary>
    public bool IsAvailable => _isAvailable.Value;

    private async Task<IEnumerable<NugetDetailsViewModel>> SearchNuGetPackages(string? term, CancellationToken token)
    {
        var providers = new List<Lazy<INuGetResourceProvider>>();
        providers.AddRange(Repository.Provider.GetCoreV3());
        var package = new PackageSource("https://api.nuget.org/v3/index.json");
        var source = new SourceRepository(package, providers);

        var filter = new SearchFilter(false);
        var resource = await source.GetResourceAsync<PackageSearchResource>().ConfigureAwait(false);
        var metadata = await resource.SearchAsync(term, filter, 0, 10, new NuGet.Common.NullLogger(), token).ConfigureAwait(false);
        return metadata.Select(x => new NugetDetailsViewModel(x));
    }
}
