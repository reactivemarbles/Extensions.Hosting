// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using Microsoft.Maui.Controls;

namespace ReactiveMarbles.Extensions.Hosting.Maui.Internals;

/// <summary>Creates a MAUI application and observes application exit.</summary>
internal interface IMauiApplicationStarter
{
    /// <summary>Creates the MAUI application for the supplied service provider.</summary>
    /// <param name="serviceProvider">The service provider used to resolve a registered MAUI application.</param>
    /// <returns>The registered MAUI application, or a default application when none is registered.</returns>
    Application Create(IServiceProvider serviceProvider);

    /// <summary>Registers an action to run when the MAUI application exits.</summary>
    /// <param name="mauiApplication">The MAUI application to observe.</param>
    /// <param name="onApplicationExit">The action to run when the application exits.</param>
    void RegisterApplicationExit(Application mauiApplication, Action onApplicationExit);
}
