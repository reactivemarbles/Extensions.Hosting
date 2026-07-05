// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReactiveMarbles.Extensions.Hosting.Wpf.Internals;

namespace ReactiveMarbles.Extensions.Hosting.Wpf;

/// <summary>Provides an implementation of IHostedService that manages the lifetime of a WPF application within a generic host environment.</summary>
/// <remarks>This service enables integration of a WPF application's main loop with the .NET Generic Host,
/// allowing WPF applications to participate in host-managed startup and shutdown sequences. It is typically registered
/// as a singleton in the application's dependency injection container.</remarks>
/// <param name="logger">The logger used to record diagnostic messages for the hosted service.</param>
/// <param name="wpfThread">The WpfThread instance responsible for managing the WPF application's UI thread.</param>
/// <param name="wpfContext">The IWpfContext that provides access to the WPF application's context and dispatcher.</param>
public class WpfHostedService(ILogger<WpfHostedService> logger, WpfThread wpfThread, IWpfContext wpfContext) : IHostedService
{
    /// <summary>Logs when the WPF application is stopping.</summary>
    private static readonly Action<ILogger, Exception?> LogStoppingWpf =
        LoggerMessage.Define(LogLevel.Debug, new EventId(1, nameof(LogStoppingWpf)), "Stopping WPF due to application exit.");

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
        if (!wpfContext.IsRunning)
        {
            return;
        }

        LogStoppingWpf(logger, null);

        // Stop application
        await wpfContext.Dispatcher.InvokeAsync(() => wpfContext.WpfApplication?.Shutdown());
    }
}
