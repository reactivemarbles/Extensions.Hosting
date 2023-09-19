// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;
using CP.Extensions.Hosting.WinUI.Internals;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CP.Extensions.Hosting.WinUI;

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
