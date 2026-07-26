// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Maui.Controls;

namespace ReactiveMarbles.Extensions.Hosting.Maui.Internals;

/// <summary>Selects a compatible current MAUI application for host registration.</summary>
internal static class MauiApplicationCapture
{
    /// <summary>Uses the supplied MAUI application instance when it matches the configured application type.</summary>
    /// <param name="mauiBuilder">The builder that contains application configuration.</param>
    /// <param name="currentApplication">The current MAUI application, when one exists.</param>
    internal static void Capture(MauiBuilder mauiBuilder, Application? currentApplication)
    {
        if (mauiBuilder.ApplicationType is null || mauiBuilder.Application is not null || currentApplication?.GetType() != mauiBuilder.ApplicationType)
        {
            return;
        }

        mauiBuilder.Application = currentApplication;
    }
}
