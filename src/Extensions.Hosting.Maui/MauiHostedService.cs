// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReactiveMarbles.Extensions.Hosting.Maui.Internals;

namespace ReactiveMarbles.Extensions.Hosting.Maui;

/// <summary>
/// Provides an implementation of the IHostedService interface to manage the lifecycle of a .NET MAUI application within
/// a generic host environment.
/// </summary>
/// <remarks>This service enables integration of a MAUI application's startup and shutdown with the ASP.NET Core
/// hosting model. It is typically used to coordinate application lifetime events between the MAUI UI thread and the
/// host.</remarks>
/// <param name="logger">The logger used to record diagnostic messages and operational events for the hosted service.</param>
/// <param name="mauiThread">The thread responsible for running the MAUI application's UI loop.</param>
/// <param name="mauiContext">The context that provides access to the MAUI application's services and dispatcher.</param>
public class MauiHostedService(ILogger<MauiHostedService> logger, MauiThread mauiThread, IMauiContext mauiContext) : IHostedService
{
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
        if (mauiContext.IsRunning)
        {
            logger.LogDebug("Stopping MAUI due to application exit.");

            // Stop application
            var completion = new TaskCompletionSource();
            mauiContext.Dispatcher?.Dispatch(() =>
            {
                mauiContext.MauiApplication?.Quit();
                completion.SetResult();
            });
            await completion.Task;
        }
    }
}
