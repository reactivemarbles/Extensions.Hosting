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
        // Create the new MAUI application
        var mauiApplication = ServiceProvider.GetService<Application>() ?? new Application();

        // Register to the MAUI application exit to stop the host application
        mauiApplication.Dispatcher.Dispatch(() => mauiApplication.ModalPopping += (s, e) => HandleApplicationExit());

        // Store the application for others to interact
        UiContext!.MauiApplication = mauiApplication;
    }

    /// <inheritdoc />
    protected override void UiThreadStart() =>
        UiContext?.MauiApplication?.Dispatcher.Dispatch(() =>
        {
            // Mark the application as running
            UiContext.IsRunning = true;

            // Use the provided IMauiService
            var mauiServices = ServiceProvider.GetServices<IMauiService>();
            foreach (var mauiService in mauiServices)
            {
                mauiService.Initialize(UiContext.MauiApplication);
            }

            // Set the main page to the shell
            var shellPages = ServiceProvider.GetServices<IMauiShell>().Cast<Page>().ToList();

            if (shellPages.Count == 1)
            {
                UiContext.MauiApplication.MainPage = shellPages[0];
            }
            else if (shellPages.Count > 1)
            {
                // Perhaps use a NavigationPage or something
                UiContext.MauiApplication.MainPage = new NavigationPage(shellPages[0]);
            }
            else
            {
                // No shell, perhaps throw or set a default
                throw new InvalidOperationException("Please inherit from IMauiShell in a Page to use the required IMauiShell interface");
            }
        });
}
