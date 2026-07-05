// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReactiveMarbles.Extensions.Hosting.Avalonia.Internals;

namespace ReactiveMarbles.Extensions.Hosting.Avalonia;

/// <summary>Provides a hosted service for managing the lifecycle of an Avalonia application within a hosted environment.</summary>
/// <remarks>This service is responsible for starting and stopping the Avalonia application, ensuring that the UI
/// thread is properly managed. It requires a valid logger, threading model, and Avalonia context to function
/// correctly.</remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="AvaloniaHostedService"/> class with the specified logger, threading model, and Avalonia context.
/// </remarks>
/// <remarks>This constructor configures the AvaloniaHostedService with the required dependencies for
/// hosting an Avalonia application. Ensure that all parameters are valid and not null to avoid runtime
/// errors.</remarks>
/// <param name="logger">The logger used to record operational messages and diagnostics for the AvaloniaHostedService.</param>
/// <param name="avaloniaThread">The AvaloniaThread instance that manages the threading model for Avalonia application operations.</param>
/// <param name="avaloniaContext">The IAvaloniaContext instance that provides access to Avalonia-specific services and context information.</param>
public class AvaloniaHostedService(ILogger<AvaloniaHostedService> logger, AvaloniaThread avaloniaThread, IAvaloniaContext avaloniaContext) : IHostedService
{
    /// <summary>Logs when the Avalonia application is stopping.</summary>
    private static readonly Action<ILogger, Exception?> LogStoppingAvalonia =
        LoggerMessage.Define(LogLevel.Debug, new EventId(1, nameof(LogStoppingAvalonia)), "Stopping Avalonia due to application exit.");

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.CompletedTask;
        }

        if (Environment.OSVersion.Platform == PlatformID.Win32NT && Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
        {
            throw new InvalidOperationException("Avalonia desktop startup requires an STA entry thread. Use [STAThread] on a synchronous entry point.");
        }

        avaloniaThread.Start();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (!avaloniaContext.IsRunning)
        {
            return;
        }

        LogStoppingAvalonia(logger, null);
        await avaloniaContext.Dispatcher.InvokeAsync(() => avaloniaContext.ApplicationLifetime?.Shutdown());
    }
}
