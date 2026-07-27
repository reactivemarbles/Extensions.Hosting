// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ServiceProcess;
using Microsoft.Extensions.Hosting;

#if REACTIVE_SHIM
namespace ReactiveMarbles.Extensions.Hosting.Reactive.PluginService;
#else
namespace ReactiveMarbles.Extensions.Hosting.PluginService;
#endif

/// <summary>Provides the operating-system interactions required to run a hosted Windows service.</summary>
internal interface IServiceHostRuntime
{
    /// <summary>Runs a host for the supplied cancellation token.</summary>
    /// <param name="host">The host to run.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the host stops.</returns>
    Task RunAsync(IHost host, CancellationToken cancellationToken);

    /// <summary>Runs a Windows service through the service control manager.</summary>
    /// <param name="service">The service to run.</param>
    void RunService(ServiceBase service);

    /// <summary>Requests that a Windows service stops.</summary>
    /// <param name="service">The service to stop.</param>
    void StopService(ServiceBase service);
}
