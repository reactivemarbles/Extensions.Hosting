// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
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
public class MauiHostedService : IHostedService
{
    /// <summary>Logs when the MAUI application is stopping.</summary>
    private static readonly Action<ILogger, Exception?> LogStoppingMaui =
        LoggerMessage.Define(LogLevel.Debug, new(1, nameof(LogStoppingMaui)), "Stopping MAUI due to application exit.");

    /// <summary>Stores the logger used to record lifecycle events.</summary>
    private readonly ILogger<MauiHostedService> _logger;

    /// <summary>Stores the MAUI UI-thread starter.</summary>
    private readonly IUiThreadStarter _mauiThreadStarter;

    /// <summary>Stores the context for the MAUI application.</summary>
    private readonly IMauiContext _mauiContext;

    /// <summary>Initializes a new instance of the <see cref="MauiHostedService"/> class.</summary>
    /// <param name="logger">The logger used to record diagnostic messages and operational events for the hosted service.</param>
    /// <param name="mauiThread">The thread responsible for running the MAUI application's UI loop.</param>
    /// <param name="mauiContext">The context that provides access to the MAUI application's services and dispatcher.</param>
    public MauiHostedService(ILogger<MauiHostedService> logger, MauiThread mauiThread, IMauiContext mauiContext)
        : this(logger, new MauiThreadStarter(mauiThread), mauiContext)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="MauiHostedService"/> class with a composed UI-thread starter.</summary>
    /// <param name="logger">The logger used to record diagnostic messages and operational events for the hosted service.</param>
    /// <param name="mauiThreadStarter">The component that starts the MAUI UI thread.</param>
    /// <param name="mauiContext">The context that provides access to the MAUI application's services and dispatcher.</param>
    internal MauiHostedService(ILogger<MauiHostedService> logger, IUiThreadStarter mauiThreadStarter, IMauiContext mauiContext)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mauiThreadStarter = mauiThreadStarter ?? throw new ArgumentNullException(nameof(mauiThreadStarter));
        _mauiContext = mauiContext ?? throw new ArgumentNullException(nameof(mauiContext));
    }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.CompletedTask;
        }

        // Make the UI thread go
        _mauiThreadStarter.Start();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (!_mauiContext.IsRunning)
        {
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();
        LogStoppingMaui(_logger, null);

        // Stop application
        var dispatcher = _mauiContext.Dispatcher
            ?? throw new InvalidOperationException("The MAUI dispatcher is unavailable.");
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        if (!dispatcher.Dispatch(() =>
        {
            _mauiContext.MauiApplication?.Quit();
            completion.SetResult();
        }))
        {
            throw new InvalidOperationException("The MAUI dispatcher rejected the application shutdown request.");
        }

        await completion.Task.WaitAsync(cancellationToken);
    }
}
