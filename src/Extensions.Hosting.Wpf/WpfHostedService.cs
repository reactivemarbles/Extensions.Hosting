// Copyright (c) 2019-2024 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReactiveMarbles.Extensions.Hosting.Wpf.Internals;

namespace ReactiveMarbles.Extensions.Hosting.Wpf;

/// <summary>
/// This hosts a WPF service, making sure the lifecycle is managed.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="WpfHostedService"/> class.
/// The constructor which takes all the DI objects.
/// </remarks>
/// <param name="logger">ILogger.</param>
/// <param name="wpfThread">WpfThread.</param>
/// <param name="wpfContext">IWpfContext.</param>
public class WpfHostedService(ILogger<WpfHostedService> logger, WpfThread wpfThread, IWpfContext wpfContext) : IHostedService
{
    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.CompletedTask;
        }

        // Make the UI thread go
        wpfThread.Start();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (wpfContext.IsRunning)
        {
            logger.LogDebug("Stopping WPF due to application exit.");

            // Stop application
            await wpfContext.Dispatcher.InvokeAsync(() => wpfContext.WpfApplication?.Shutdown());
        }
    }
}
