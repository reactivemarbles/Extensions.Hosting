// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using ReactiveMarbles.Extensions.Hosting.UiThread;

namespace ReactiveMarbles.Extensions.Hosting.Maui.Internals;

/// <summary>Provides a UI thread implementation for .NET MAUI applications, enabling initialization and management of the MAUI application lifecycle within a host environment.</summary>
/// <remarks>This class is typically used to host and manage a MAUI application's main UI thread in scenarios
/// where integration with a custom host or dependency injection container is required. It ensures that the MAUI
/// application and its services are properly initialized and managed on the correct thread.</remarks>
public class MauiThread : BaseUiThread<IMauiContext>
{
    /// <summary>Stores the component that creates and observes the MAUI application.</summary>
    private readonly IMauiApplicationStarter _mauiApplicationStarter;

    /// <summary>Initializes a new instance of the <see cref="MauiThread"/> class.</summary>
    /// <param name="serviceProvider">The service provider used to resolve application and service dependencies required by the MAUI UI thread.</param>
    public MauiThread(IServiceProvider serviceProvider)
        : this(serviceProvider, new MauiApplicationStarter(), useDedicatedUiThread: true)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="MauiThread"/> class with a composed MAUI application starter and thread mode.</summary>
    /// <param name="serviceProvider">The service provider used to resolve application and service dependencies required by the MAUI UI thread.</param>
    /// <param name="mauiApplicationStarter">The component that creates the MAUI application and observes its exit.</param>
    /// <param name="useDedicatedUiThread">Whether to create a dedicated UI thread.</param>
    internal MauiThread(IServiceProvider serviceProvider, IMauiApplicationStarter mauiApplicationStarter, bool useDedicatedUiThread)
        : base(serviceProvider, useDedicatedUiThread)
    {
        _mauiApplicationStarter = mauiApplicationStarter ?? throw new ArgumentNullException(nameof(mauiApplicationStarter));
    }

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
            var mauiApplication = _mauiApplicationStarter.Create(ServiceProvider);

            // Register to the MAUI application exit to stop the host application
            _mauiApplicationStarter.RegisterApplicationExit(mauiApplication, HandleApplicationExit);

            // Store the application for others to interact
            UiContext!.MauiApplication = mauiApplication;

            // Mark the application as running
            UiContext.IsRunning = true;

            // Use the provided IMauiService
            foreach (var mauiService in ServiceProvider.GetServices<IMauiService>())
            {
                mauiService.Initialize(UiContext.MauiApplication);
            }
        });
}
