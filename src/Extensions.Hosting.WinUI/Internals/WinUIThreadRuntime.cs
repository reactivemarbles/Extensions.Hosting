// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using WinRT;

namespace ReactiveMarbles.Extensions.Hosting.WinUI.Internals;

/// <summary>Provides the production WinUI application-loop runtime.</summary>
internal sealed class WinUIThreadRuntime : IWinUIThreadRuntime
{
    /// <inheritdoc />
    public void InitializeComWrappers() =>
        ComWrappersSupport.InitializeComWrappers();

    /// <inheritdoc />
    public void Start(Action<DispatcherQueue?, SynchronizationContext> initialize)
    {
        ArgumentNullException.ThrowIfNull(initialize);

        Application.Start(_ =>
        {
            var dispatcher = DispatcherQueue.GetForCurrentThread();
            initialize(dispatcher, new DispatcherQueueSynchronizationContext(dispatcher));
        });
    }

    /// <inheritdoc />
    public Application GetApplication(IServiceProvider serviceProvider) =>
        serviceProvider.GetRequiredService<Application>();

    /// <inheritdoc />
    public Window CreateWindow(IServiceProvider serviceProvider, Type windowType) =>
        (Window)ActivatorUtilities.CreateInstance(serviceProvider, windowType);

    /// <inheritdoc />
    public void ActivateWindow(Window window) =>
        window.Activate();
}
