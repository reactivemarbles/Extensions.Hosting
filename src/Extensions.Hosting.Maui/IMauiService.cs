// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Maui.Controls;

namespace ReactiveMarbles.Extensions.Hosting.Maui;

/// <summary>Defines a service that provides initialization logic for a .NET MAUI application. Implementations perform necessary setup when the application starts.</summary>
/// <remarks>The Initialize method is called from the UI thread and should contain any platform-specific or
/// application-wide initialization required before the app runs. Implementers should ensure that any long-running or
/// blocking operations are avoided to prevent UI delays.</remarks>
public interface IMauiService
{
    /// <summary>Initializes the specified application instance, preparing it for use.</summary>
    /// <param name="application">The application instance to initialize. Cannot be null.</param>
    void Initialize(Application application);
}
