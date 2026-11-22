// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Extensions.Hosting.ReactiveAvalonia.Example;

namespace Extensions.Hosting.Avalonia.Tests;

/// <summary>Verifies the Reactive Avalonia example's Primitives-based view model.</summary>
public sealed class ReactiveAvaloniaExampleTests
{
    /// <summary>Verifies property helpers initialize and an empty search term raises a change notification.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task EmptySearchTerm_InitializesBindingsAndNotifies()
    {
        var viewModel = new AppViewModel();
        var changedProperties = new List<string?>();
        viewModel.PropertyChanged += (_, args) => changedProperties.Add(args.PropertyName);

        viewModel.SearchTerm = string.Empty;

        await Assert.That(viewModel.SearchTerm).IsEqualTo(string.Empty);
        await Assert.That(changedProperties).Contains(nameof(AppViewModel.SearchTerm));
        await Assert.That(viewModel.IsAvailable).IsFalse();
    }
}
