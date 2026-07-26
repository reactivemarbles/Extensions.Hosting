// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Threading;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace ReactiveMarbles.Extensions.Hosting.WinUI.Internals;

/// <summary>Runs the WinUI application loop for a UI thread.</summary>
internal interface IWinUIThreadRuntime
{
    /// <summary>Initializes COM wrappers required by the WinUI runtime.</summary>
    void InitializeComWrappers();

    /// <summary>Runs the application loop and invokes the supplied UI-thread initialization callback.</summary>
    /// <param name="initialize">The callback that initializes WinUI services and the main window.</param>
    void Start(Action<DispatcherQueue?, SynchronizationContext> initialize);

    /// <summary>Resolves the WinUI application from the supplied services.</summary>
    /// <param name="serviceProvider">The service provider from which to resolve the application.</param>
    /// <returns>The resolved WinUI application.</returns>
    Application GetApplication(IServiceProvider serviceProvider);

    /// <summary>Creates the configured WinUI window.</summary>
    /// <param name="serviceProvider">The service provider used to resolve window dependencies.</param>
    /// <param name="windowType">The type of window to create.</param>
    /// <returns>The created WinUI window.</returns>
    Window CreateWindow(IServiceProvider serviceProvider, Type windowType);

    /// <summary>Activates the specified WinUI window.</summary>
    /// <param name="window">The window to activate.</param>
    void ActivateWindow(Window window);
}
