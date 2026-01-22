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
/// Provides an implementation of <see cref="IHostedService"/> that manages the lifecycle of a WinUI application within
/// a generic host environment.
/// </summary>
/// <remarks>This service enables integration of a WinUI application with the .NET Generic Host, allowing the
/// application to participate in the host's startup and shutdown processes. It is typically registered as a singleton
/// in the application's dependency injection container.</remarks>
/// <param name="logger">The logger used to record diagnostic messages and operational events for the hosted service.</param>
/// <param name="winUIThread">The WinUI thread responsible for running the application's UI event loop.</param>
/// <param name="winUIContext">The context that provides access to the WinUI application's dispatcher and lifecycle management.</param>
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
