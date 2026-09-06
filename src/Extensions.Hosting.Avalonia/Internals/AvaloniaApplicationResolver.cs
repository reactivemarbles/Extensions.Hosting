// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using Avalonia;
using Microsoft.Extensions.DependencyInjection;

namespace ReactiveMarbles.Extensions.Hosting.Avalonia.Internals;

/// <summary>Resolves the application used to initialize hosted Avalonia services.</summary>
internal static class AvaloniaApplicationResolver
{
    /// <summary>Gets the host's application or creates the default application.</summary>
    /// <param name="serviceProvider">The host service provider.</param>
    /// <returns>The application used by the desktop application builder.</returns>
    internal static Application GetOrCreate(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        return serviceProvider.GetService<Application>() ?? new Application();
    }

    /// <summary>Uses the active application, falling back to the host's registered application.</summary>
    /// <param name="currentApplication">The active Avalonia application, when available.</param>
    /// <param name="serviceProvider">The host service provider.</param>
    /// <returns>The application to use for service initialization.</returns>
    /// <exception cref="ArgumentNullException">The service provider is null.</exception>
    /// <exception cref="InvalidOperationException">No application is available.</exception>
    internal static Application Resolve(Application? currentApplication, IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        return currentApplication ?? serviceProvider.GetService<Application>()
            ?? throw new InvalidOperationException("Unable to initialize the Avalonia application.");
    }
}
