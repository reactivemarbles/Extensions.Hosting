// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using ReactiveMarbles.Extensions.Hosting.UiThread;

namespace ReactiveMarbles.Extensions.Hosting.Maui.Internals;

/// <summary>
/// This contains the logic for the MAUI thread.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="MauiThread"/> class.
/// This will create the MauiThread.
/// </remarks>
/// <param name="serviceProvider">IServiceProvider.</param>
public class MauiThread(IServiceProvider serviceProvider) : BaseUiThread<IMauiContext>(serviceProvider)
{
    /// <inheritdoc />
    protected override void PreUiThreadStart()
    {
        // No initialization needed here
    }

    /// <inheritdoc />
    protected override void UiThreadStart() =>
        UiContext?.Dispatcher?.Dispatch(() =>
        {
            // Create the new MAUI application
            var mauiApplication = ServiceProvider.GetService<Application>() ?? new Application();

            // Register to the MAUI application exit to stop the host application
            mauiApplication.ModalPopping += (s, e) => HandleApplicationExit();

            // Store the application for others to interact
            UiContext!.MauiApplication = mauiApplication;

            // Mark the application as running
            UiContext.IsRunning = true;

            // Use the provided IMauiService
            var mauiServices = ServiceProvider.GetServices<IMauiService>();
            foreach (var mauiService in mauiServices)
            {
                mauiService.Initialize(UiContext.MauiApplication);
            }

            // The main page will be set in CreateWindow
        });
}
