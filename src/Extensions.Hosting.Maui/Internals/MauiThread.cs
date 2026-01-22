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
/// Provides a UI thread implementation for .NET MAUI applications, enabling initialization and management of the MAUI
/// application lifecycle within a host environment.
/// </summary>
/// <remarks>This class is typically used to host and manage a MAUI application's main UI thread in scenarios
/// where integration with a custom host or dependency injection container is required. It ensures that the MAUI
/// application and its services are properly initialized and managed on the correct thread.</remarks>
/// <param name="serviceProvider">The service provider used to resolve application and service dependencies required by the MAUI UI thread.</param>
public partial class MauiThread(IServiceProvider serviceProvider) : BaseUiThread<IMauiContext>(serviceProvider)
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
        });
}
