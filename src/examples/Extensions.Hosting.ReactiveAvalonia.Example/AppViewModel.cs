// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Configuration;
using NuGet.Protocol.Core.Types;
using ReactiveUI;

namespace Extensions.Hosting.ReactiveAvalonia.Example;

/// <summary>Represents the root view model for the sample NuGet browser application.</summary>
public class AppViewModel : ReactiveObject
{
    /// <summary>Gets the search debounce in milliseconds.</summary>
    private const int SearchDelayMilliseconds = 800;

    /// <summary>Gets the maximum number of package search results.</summary>
    private const int MaximumSearchResults = 10;

    /// <summary>Stores the search results value.</summary>
    private ObservableAsPropertyHelper<IEnumerable<NugetDetailsViewModel>>? _searchResults;

    /// <summary>Stores the is available value.</summary>
    private ObservableAsPropertyHelper<bool>? _isAvailable;

    /// <summary>Gets or sets the search term.</summary>
    public string? SearchTerm
    {
        get;
        set
        {
            EnsurePropertiesInitialized();
            _ = this.RaiseAndSetIfChanged(ref field, value);
        }
    }

    /// <summary>Gets the search results.</summary>
    public IEnumerable<NugetDetailsViewModel> SearchResults => EnsureSearchResultsInitialized().Value;

    /// <summary>Gets a value indicating whether results are available.</summary>
    public bool IsAvailable => EnsureAvailabilityInitialized().Value;

    /// <summary>Searches NuGet packages that match the supplied term.</summary>
    /// <param name="term">The search term.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The package detail view models that match the search term.</returns>
    private static async Task<IEnumerable<NugetDetailsViewModel>> SearchNuGetPackages(string? term, CancellationToken token)
    {
        var providers = new List<Lazy<INuGetResourceProvider>>(Repository.Provider.GetCoreV3());
        var package = new PackageSource("https://api.nuget.org/v3/index.json");
        var source = new SourceRepository(package, providers);

        var filter = new SearchFilter(false);
        var resource = await source.GetResourceAsync<PackageSearchResource>().ConfigureAwait(false);
        var metadata = await resource.SearchAsync(
                term,
                filter,
                0,
                MaximumSearchResults,
                new NuGet.Common.NullLogger(),
                token)
            .ConfigureAwait(false);
        var results = new List<NugetDetailsViewModel>();
        foreach (var packageMetadata in metadata)
        {
            results.Add(new(packageMetadata));
        }

        return results;
    }

    /// <summary>Initializes the lazily-created observable properties.</summary>
    private void EnsurePropertiesInitialized()
    {
        _ = EnsureSearchResultsInitialized();
        _ = EnsureAvailabilityInitialized();
    }

    /// <summary>Gets or initializes the observable search-results helper.</summary>
    /// <returns>The initialized search-results helper.</returns>
    private ObservableAsPropertyHelper<IEnumerable<NugetDetailsViewModel>> EnsureSearchResultsInitialized()
    {
        if (_searchResults is not null)
        {
            return _searchResults;
        }

        _searchResults = this
            .WhenAnyValue(x => x.SearchTerm)
            .Throttle(TimeSpan.FromMilliseconds(SearchDelayMilliseconds))
            .Select(static term => term?.Trim())
            .DistinctUntilChanged()
            .Where(static term => !string.IsNullOrWhiteSpace(term))
            .SelectMany(term => Signal.FromAsync(token => SearchNuGetPackages(term, token)))
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .ToProperty(this, x => x.SearchResults);
        _ = _searchResults.ThrownExceptions.Subscribe(static error => { });
        return _searchResults;
    }

    /// <summary>Gets or initializes the observable availability helper.</summary>
    /// <returns>The initialized availability helper.</returns>
    private ObservableAsPropertyHelper<bool> EnsureAvailabilityInitialized()
    {
        if (_isAvailable is not null)
        {
            return _isAvailable;
        }

        _ = EnsureSearchResultsInitialized();
        _isAvailable = this
            .WhenAnyValue(x => x.SearchResults)
            .Select(static searchResults => searchResults is not null)
            .ToProperty(this, x => x.IsAvailable);
        return _isAvailable;
    }
}
