// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReactiveMarbles.Extensions.Hosting.Maui.Internals;

namespace ReactiveMarbles.Extensions.Hosting.Maui;

/// <summary>Provides an implementation of the IHostedService interface to manage the lifecycle of a .NET MAUI application within a generic host environment.</summary>
/// <remarks>This service enables integration of a MAUI application's startup and shutdown with the ASP.NET Core
/// hosting model. It is typically used to coordinate application lifetime events between the MAUI UI thread and the
/// host.</remarks>
/// <param name="logger">The logger used to record diagnostic messages and operational events for the hosted service.</param>
/// <param name="mauiThread">The thread responsible for running the MAUI application's UI loop.</param>
/// <param name="mauiContext">The context that provides access to the MAUI application's services and dispatcher.</param>
public class MauiHostedService(ILogger<MauiHostedService> logger, MauiThread mauiThread, IMauiContext mauiContext) : IHostedService
{
    /// <summary>Logs when the MAUI application is stopping.</summary>
    private static readonly Action<ILogger, Exception?> LogStoppingMaui =
        LoggerMessage.Define(LogLevel.Debug, new EventId(1, nameof(LogStoppingMaui)), "Stopping MAUI due to application exit.");

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.CompletedTask;
        }

        // Make the UI thread go
        mauiThread.Start();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (!mauiContext.IsRunning)
        {
            return;
        }

        LogStoppingMaui(logger, null);

        // Stop application
        var completion = new TaskCompletionSource();
        _ = mauiContext.Dispatcher?.Dispatch(() =>
        {
            mauiContext.MauiApplication?.Quit();
            completion.SetResult();
        });
        await completion.Task;
    }
}
