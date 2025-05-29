// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReactiveMarbles.Extensions.Hosting.Plugins;

namespace ReactiveMarbles.Plugin.Example;

/// <summary>
/// Example for a IHostedService which tracks live-time events.
/// </summary>
public class LifetimeEventsHostedService(ILogger<LifetimeEventsHostedService> logger, IHostApplicationLifetime hostApplicationLifetime) : HostedServiceBase<LifetimeEventsHostedService>(logger, hostApplicationLifetime)
{
    /// <summary>
    /// Called when [started].
    /// </summary>
    /// <returns>
    /// A Task.
    /// </returns>
    public override Task OnStarted()
    {
        Logger.LogInformation("OnStarted has been called.");

        // Perform post-startup activities here
        return base.OnStarted();
    }

    /// <summary>
    /// Called when [started].
    /// </summary>
    public override void OnStopping()
    {
        Logger.LogInformation("OnStopping has been called.");

        // Perform on-stopping activities here
        base.OnStopping();
    }

    /// <summary>
    /// Called when [stopped].
    /// </summary>
    public override void OnStopped()
    {
        Logger.LogInformation("OnStopped has been called.");

        // Perform post-stopping activities here
        base.OnStopped();
    }

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected override void Dispose(bool disposing)
    {
        Logger.LogInformation("Dispose has been called.");
        base.Dispose(disposing);
    }
}
