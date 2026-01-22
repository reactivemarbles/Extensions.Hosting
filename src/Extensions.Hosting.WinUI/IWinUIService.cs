// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.UI.Xaml;

namespace ReactiveMarbles.Extensions.Hosting.WinUI;

/// <summary>
/// Defines a service for initializing WinUI components from the UI thread.
/// </summary>
public interface IWinUIService
{
    /// <summary>
    /// Initializes the specified application instance, preparing it for use.
    /// </summary>
    /// <param name="application">The application instance to initialize. Cannot be null.</param>
    void Initialize(Application application);
}
