// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using ReactiveMarbles.Extensions.Hosting.Avalonia.Internals;

namespace Extensions.Hosting.Avalonia.Tests;

/// <summary>Verifies selection of the application used by hosted services.</summary>
public sealed class AvaloniaApplicationResolverTests
{
    /// <summary>Verifies default creation and registered application selection.</summary>
    /// <returns>A task representing the test.</returns>
    [Test]
    public async Task GetOrCreate_UsesRegistrationOrCreatesDefault()
    {
        var registered = new Application();
        await using var configuredProvider = new ServiceCollection().AddSingleton(registered).BuildServiceProvider();
        await using var emptyProvider = new ServiceCollection().BuildServiceProvider();

        await Assert.That(AvaloniaApplicationResolver.GetOrCreate(configuredProvider)).IsSameReferenceAs(registered);
        await Assert.That(AvaloniaApplicationResolver.GetOrCreate(emptyProvider)).IsTypeOf<Application>();
        await Assert.That(static () => AvaloniaApplicationResolver.GetOrCreate(null!)).Throws<ArgumentNullException>();
    }

    /// <summary>Verifies the active application takes precedence over the registered fallback.</summary>
    /// <returns>A task representing the test.</returns>
    [Test]
    public async Task Resolve_PrefersCurrentApplication()
    {
        var current = new Application();
        var registered = new Application();
        await using var provider = new ServiceCollection().AddSingleton(registered).BuildServiceProvider();

        await Assert.That(AvaloniaApplicationResolver.Resolve(current, provider)).IsSameReferenceAs(current);
    }

    /// <summary>Verifies a registered application is used when there is no active application.</summary>
    /// <returns>A task representing the test.</returns>
    [Test]
    public async Task Resolve_UsesRegisteredApplication()
    {
        var registered = new Application();
        await using var provider = new ServiceCollection().AddSingleton(registered).BuildServiceProvider();

        await Assert.That(AvaloniaApplicationResolver.Resolve(null, provider)).IsSameReferenceAs(registered);
    }

    /// <summary>Verifies initialization fails clearly when neither application source is available.</summary>
    /// <returns>A task representing the test.</returns>
    [Test]
    public async Task Resolve_WithoutApplication_Throws()
    {
        await using var provider = new ServiceCollection().BuildServiceProvider();

        await Assert.That(() => AvaloniaApplicationResolver.Resolve(null, provider)).Throws<InvalidOperationException>();
    }

    /// <summary>Verifies a missing service provider is rejected.</summary>
    /// <returns>A task representing the test.</returns>
    [Test]
    public async Task Resolve_WithNullProvider_Throws() =>
        await Assert.That(static () => AvaloniaApplicationResolver.Resolve(null, null!)).Throws<ArgumentNullException>();
}
