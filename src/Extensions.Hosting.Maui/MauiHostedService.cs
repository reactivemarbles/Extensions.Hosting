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
/// This hosts a MAUI service, making sure the lifecycle is managed.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="MauiHostedService"/> class.
/// The constructor which takes all the DI objects.
/// </remarks>
/// <param name="logger">ILogger.</param>
/// <param name="mauiThread">MauiThread.</param>
/// <param name="mauiContext">IMauiContext.</param>
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
