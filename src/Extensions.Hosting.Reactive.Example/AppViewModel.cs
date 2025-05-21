// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Linq;
using NuGet.Configuration;
using NuGet.Protocol.Core.Types;
using ReactiveUI;

namespace Extensions.Hosting.Reactive.Example;

/// <summary>
/// AppViewModel is where we will describe the interaction of our application.
/// We can describe the entire application in one class since it's very small now.
/// Most ViewModels will derive off ReactiveObject, while most Model classes will
/// most derive off INotifyPropertyChanged.
/// </summary>
/// <seealso cref="ReactiveObject" />
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
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToProperty(this, x => x.SearchResults);

        _searchResults.ThrownExceptions.Subscribe(error => { /* Handle errors here */ });

        _isAvailable = this
            .WhenAnyValue(x => x.SearchResults)
            .Select(searchResults => searchResults != null)
            .ToProperty(this, x => x.IsAvailable);
    }

    /// <summary>
    /// Gets or sets the search term.
    /// </summary>
    /// <value>
    /// The search term.
    /// </value>
    public string? SearchTerm
    {
        get => _searchTerm;
        set => this.RaiseAndSetIfChanged(ref _searchTerm, value);
    }

    /// <summary>
    /// Gets the search results.
    /// </summary>
    /// <value>
    /// The search results.
    /// </value>
    public IEnumerable<NugetDetailsViewModel> SearchResults => _searchResults.Value;

    /// <summary>
    /// Gets a value indicating whether this instance is available.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is available; otherwise, <c>false</c>.
    /// </value>
    public bool IsAvailable => _isAvailable.Value;

    private async Task<IEnumerable<NugetDetailsViewModel>> SearchNuGetPackages(string? term, CancellationToken token)
    {
        var providers = new List<Lazy<INuGetResourceProvider>>();
        providers.AddRange(Repository.Provider.GetCoreV3()); // Add v3 API support
        var package = new PackageSource("https://api.nuget.org/v3/index.json");
        var source = new SourceRepository(package, providers);

        var filter = new SearchFilter(false);
        var resource = await source.GetResourceAsync<PackageSearchResource>().ConfigureAwait(false);
        var metadata = await resource.SearchAsync(term, filter, 0, 10, new NuGet.Common.NullLogger(), token).ConfigureAwait(false);
        return metadata.Select(x => new NugetDetailsViewModel(x));
    }
}
