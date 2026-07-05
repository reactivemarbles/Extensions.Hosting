// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReactiveMarbles.Extensions.Hosting.Plugins;

namespace ReactiveMarbles.Plugin.Example;

/// <summary>Provides a timed background service example.</summary>
/// <param name="logger">The logger used to record timed service activity.</param>
/// <param name="hostApplicationLifetime">The host lifetime used to receive application lifecycle notifications.</param>
public class TimedHostedService(ILogger<TimedHostedService> logger, IHostApplicationLifetime hostApplicationLifetime) : HostedServiceBase<TimedHostedService>(logger, hostApplicationLifetime)
{
    /// <summary>Logs that the timed service is starting.</summary>
    private static readonly Action<ILogger, Exception?> ServiceStarting =
        LoggerMessage.Define(LogLevel.Information, new EventId(1, nameof(OnStarted)), "Timed Background Service is starting.");

    /// <summary>Logs that the timed service is stopping.</summary>
    private static readonly Action<ILogger, Exception?> ServiceStopping =
        LoggerMessage.Define(LogLevel.Information, new EventId(2, nameof(OnStopping)), "Timed Background Service is stopping.");

    /// <summary>Logs that the timed service has stopped.</summary>
    private static readonly Action<ILogger, Exception?> ServiceStopped =
        LoggerMessage.Define(LogLevel.Information, new EventId(3, nameof(OnStopping)), "Plugin Service Stopped");

    /// <summary>Logs that the timed service is processing work.</summary>
    private static readonly Action<ILogger, Exception?> ServiceWorking =
        LoggerMessage.Define(LogLevel.Information, new EventId(4, nameof(ServiceWorking)), "Timed Background Service is working.");

    /// <summary>Stores the timer value.</summary>
    private Timer? _timer;

    /// <summary>Called when [started].</summary>
    /// <returns>
    /// A Task.
    /// </returns>
    public override Task OnStarted()
    {
        ServiceStarting(Logger, null);

        _timer = new(_ => ServiceWorking(Logger, null), null, TimeSpan.Zero, TimeSpan.FromSeconds(5));

        return Task.CompletedTask;
    }

    /// <summary>Called when [started].</summary>
    public override void OnStopping()
    {
        ServiceStopping(Logger, null);

        _timer?.Change(Timeout.Infinite, 0);
        ServiceStopped(Logger, null);
        base.OnStopping();
    }

    /// <summary>Releases unmanaged and - optionally - managed resources.</summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _timer?.Dispose();
        }

        base.Dispose(disposing);
    }
}
