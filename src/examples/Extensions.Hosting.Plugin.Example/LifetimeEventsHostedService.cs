// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReactiveMarbles.Extensions.Hosting.Plugins;

namespace ReactiveMarbles.Plugin.Example;

/// <summary>Example for a IHostedService which tracks live-time events.</summary>
/// <param name="logger">The logger used to record lifetime events.</param>
/// <param name="hostApplicationLifetime">The host lifetime used to receive application lifecycle notifications.</param>
public class LifetimeEventsHostedService(ILogger<LifetimeEventsHostedService> logger, IHostApplicationLifetime hostApplicationLifetime) : HostedServiceBase<LifetimeEventsHostedService>(logger, hostApplicationLifetime)
{
    /// <summary>Logs that the hosted service started callback has run.</summary>
    private static readonly Action<ILogger, Exception?> OnStartedCalled =
        LoggerMessage.Define(LogLevel.Information, new EventId(1, nameof(OnStarted)), "OnStarted has been called.");

    /// <summary>Logs that the hosted service stopping callback has run.</summary>
    private static readonly Action<ILogger, Exception?> OnStoppingCalled =
        LoggerMessage.Define(LogLevel.Information, new EventId(2, nameof(OnStopping)), "OnStopping has been called.");

    /// <summary>Logs that the hosted service stopped callback has run.</summary>
    private static readonly Action<ILogger, Exception?> OnStoppedCalled =
        LoggerMessage.Define(LogLevel.Information, new EventId(3, nameof(OnStopped)), "OnStopped has been called.");

    /// <summary>Logs that the hosted service dispose callback has run.</summary>
    private static readonly Action<ILogger, Exception?> DisposeCalled =
        LoggerMessage.Define(LogLevel.Information, new EventId(4, nameof(Dispose)), "Dispose has been called.");

    /// <summary>Called when [started].</summary>
    /// <returns>
    /// A Task.
    /// </returns>
    public override Task OnStarted()
    {
        OnStartedCalled(Logger, null);

        // Perform post-startup activities here
        return base.OnStarted();
    }

    /// <summary>Called when [started].</summary>
    public override void OnStopping()
    {
        OnStoppingCalled(Logger, null);

        // Perform on-stopping activities here
        base.OnStopping();
    }

    /// <summary>Called when [stopped].</summary>
    public override void OnStopped()
    {
        OnStoppedCalled(Logger, null);

        // Perform post-stopping activities here
        base.OnStopped();
    }

    /// <summary>Releases unmanaged and - optionally - managed resources.</summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected override void Dispose(bool disposing)
    {
        DisposeCalled(Logger, null);
        base.Dispose(disposing);
    }
}
