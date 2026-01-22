// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReactiveMarbles.Extensions.Hosting.Plugins;

namespace ReactiveMarbles.Plugin.Example;

/// <summary>
/// TimedHostedService.
/// </summary>
public class TimedHostedService(ILogger<TimedHostedService> logger, IHostApplicationLifetime hostApplicationLifetime) : HostedServiceBase<TimedHostedService>(logger, hostApplicationLifetime)
{
    private Timer? _timer;

    /// <summary>
    /// Called when [started].
    /// </summary>
    /// <returns>
    /// A Task.
    /// </returns>
    public override Task OnStarted()
    {
        Logger.LogInformation("Timed Background Service is starting.");

        _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));

        return Task.CompletedTask;
    }

    /// <summary>
    /// Called when [started].
    /// </summary>
    public override void OnStopping()
    {
        Logger.LogInformation("Timed Background Service is stopping.");

        _timer?.Change(Timeout.Infinite, 0);
        Logger?.LogInformation("Plugin Service Stopped");
        base.OnStopping();
    }

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _timer?.Dispose();
        }

        base.Dispose(disposing);
    }

    private void DoWork(object? state) => Logger.LogInformation("Timed Background Service is working.");
}
