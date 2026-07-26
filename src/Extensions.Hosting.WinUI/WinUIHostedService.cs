// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReactiveMarbles.Extensions.Hosting.WinUI.Internals;

namespace ReactiveMarbles.Extensions.Hosting.WinUI;

/// <summary>Provides an implementation of <see cref="IHostedService"/> that manages the lifecycle of a WinUI application within a generic host environment.</summary>
/// <remarks>This service enables integration of a WinUI application with the .NET Generic Host, allowing the
/// application to participate in the host's startup and shutdown processes. It is typically registered as a singleton
/// in the application's dependency injection container.</remarks>
public class WinUIHostedService : IHostedService
{
    /// <summary>Logs when the WinUI application is stopping.</summary>
    private static readonly Action<ILogger, Exception?> LogStoppingWinUI =
        LoggerMessage.Define(LogLevel.Debug, new(1, nameof(LogStoppingWinUI)), "Stopping WinUI due to application exit.");

    /// <summary>Stores the logger used to record lifecycle events.</summary>
    private readonly ILogger<WinUIHostedService> _logger;

    /// <summary>Stores the WinUI UI-thread starter.</summary>
    private readonly IUiThreadStarter _winUIThreadStarter;

    /// <summary>Stores the context for the WinUI application.</summary>
    private readonly IWinUIContext _winUIContext;

    /// <summary>Optionally stores a testable callback for scheduling work on the WinUI dispatcher.</summary>
    private readonly Func<Action, bool>? _tryEnqueue;

    /// <summary>Initializes a new instance of the <see cref="WinUIHostedService"/> class.</summary>
    /// <param name="logger">The logger used to record diagnostic messages and operational events for the hosted service.</param>
    /// <param name="winUIThread">The WinUI thread responsible for running the application's UI event loop.</param>
    /// <param name="winUIContext">The context that provides access to the WinUI application's dispatcher and lifecycle management.</param>
    public WinUIHostedService(ILogger<WinUIHostedService> logger, WinUIThread winUIThread, IWinUIContext winUIContext)
        : this(logger, new WinUIThreadStarter(winUIThread), winUIContext)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="WinUIHostedService"/> class with a composed UI-thread starter.</summary>
    /// <param name="logger">The logger used to record diagnostic messages and operational events for the hosted service.</param>
    /// <param name="winUIThreadStarter">The component that starts the WinUI UI thread.</param>
    /// <param name="winUIContext">The context that provides access to the WinUI application's dispatcher and lifecycle management.</param>
    internal WinUIHostedService(ILogger<WinUIHostedService> logger, IUiThreadStarter winUIThreadStarter, IWinUIContext winUIContext)
        : this(logger, winUIThreadStarter, winUIContext, null)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="WinUIHostedService"/> class with a composed dispatcher callback.</summary>
    /// <param name="logger">The logger used to record diagnostic messages and operational events for the hosted service.</param>
    /// <param name="winUIThreadStarter">The component that starts the WinUI UI thread.</param>
    /// <param name="winUIContext">The context that provides access to the WinUI application's dispatcher and lifecycle management.</param>
    /// <param name="tryEnqueue">The callback that schedules work on the WinUI dispatcher, or <see langword="null"/> to use the context dispatcher.</param>
    internal WinUIHostedService(ILogger<WinUIHostedService> logger, IUiThreadStarter winUIThreadStarter, IWinUIContext winUIContext, Func<Action, bool>? tryEnqueue)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _winUIThreadStarter = winUIThreadStarter ?? throw new ArgumentNullException(nameof(winUIThreadStarter));
        _winUIContext = winUIContext ?? throw new ArgumentNullException(nameof(winUIContext));
        _tryEnqueue = tryEnqueue;
    }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.CompletedTask;
        }

        // Make the UI thread go
        _winUIThreadStarter.Start();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (!_winUIContext.IsRunning)
        {
            return;
        }

        LogStoppingWinUI(_logger, null);

        // Stop application
        cancellationToken.ThrowIfCancellationRequested();
        var tryEnqueue = _tryEnqueue ?? (_winUIContext.Dispatcher is { } dispatcher ? (Func<Action, bool>)(callback => dispatcher.TryEnqueue(() => callback())) : null)
            ?? throw new InvalidOperationException("The WinUI dispatcher is not available while the application is running.");

        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        if (!tryEnqueue(() =>
        {
            _winUIContext.WinUIApplication?.Exit();
            _ = completion.TrySetResult();
        }))
        {
            throw new InvalidOperationException("The WinUI dispatcher rejected the application shutdown callback.");
        }

        await completion.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
    }
}
