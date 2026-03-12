// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Avalonia;

namespace ReactiveMarbles.Extensions.Hosting.Avalonia;

/// <summary>
/// Defines methods for initializing and managing Avalonia application services within a host environment.
/// </summary>
/// <remarks>Implementations of this interface should ensure that initialization occurs on the UI thread and
/// adheres to Avalonia's application lifecycle requirements. Thread safety and proper sequencing are important when
/// invoking initialization routines. This interface is intended for use in scenarios where Avalonia is required for UI
/// rendering and integration with hosting frameworks.</remarks>
public interface IAvaloniaService
{
    /// <summary>
    /// Initializes the application with the specified configuration settings.
    /// </summary>
    /// <remarks>This method must be called before any other operations on the application to ensure it is
    /// properly configured.</remarks>
    /// <param name="application">The application instance that contains the configuration settings to be applied during initialization.</param>
    void Initialize(Application application);
}
