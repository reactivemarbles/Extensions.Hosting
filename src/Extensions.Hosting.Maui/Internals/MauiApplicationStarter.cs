// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;

namespace ReactiveMarbles.Extensions.Hosting.Maui.Internals;

/// <summary>Provides the production MAUI application creation and exit-observation behavior.</summary>
internal sealed class MauiApplicationStarter : IMauiApplicationStarter
{
    /// <inheritdoc />
    public Application Create(IServiceProvider serviceProvider) =>
        serviceProvider.GetService<Application>() ?? new Application();

    /// <inheritdoc />
    public void RegisterApplicationExit(Application mauiApplication, Action onApplicationExit) =>
        RegisterApplicationExit(
            handler => mauiApplication.ModalPopping += handler,
            onApplicationExit);

    /// <summary>Registers an application-exit callback through the supplied modal-pop subscription.</summary>
    /// <param name="subscribe">The callback that subscribes the modal-pop event handler.</param>
    /// <param name="onApplicationExit">The callback to invoke when the application exits.</param>
    internal static void RegisterApplicationExit(
        Action<EventHandler<ModalPoppingEventArgs>> subscribe,
        Action onApplicationExit) =>
        subscribe((sender, eventArgs) => onApplicationExit());
}
