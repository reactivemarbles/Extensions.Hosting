// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReactiveMarbles.Extensions.Hosting.WinUI.Internals;

namespace ReactiveMarbles.Extensions.Hosting.WinUI;

/// <summary>
/// This hosts a WinUI service, making sure the lifecycle is managed.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="WinUIHostedService"/> class.
/// The constructor which takes all the DI objects.
/// </remarks>
/// <param name="logger">ILogger.</param>
/// <param name="winUIThread">WinUIThread.</param>
/// <param name="winUIContext">IWinUIContext.</param>
public class WinUIHostedService(ILogger<WinUIHostedService> logger, WinUIThread winUIThread, IWinUIContext winUIContext) : IHostedService
{
    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.CompletedTask;
        }

        // Make the UI thread go
        winUIThread.Start();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (winUIContext.IsRunning)
        {
            logger.LogDebug("Stopping WinUI due to application exit.");

            // Stop application
            var completion = new TaskCompletionSource();
            winUIContext.Dispatcher?.TryEnqueue(() =>
            {
                winUIContext.WinUIApplication?.Exit();
                completion.SetResult();
            });
            await completion.Task;
        }
    }
}
