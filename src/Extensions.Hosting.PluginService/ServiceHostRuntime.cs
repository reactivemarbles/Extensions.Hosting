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

/// <summary>Provides the production implementation of <see cref="IServiceHostRuntime"/>.</summary>
internal sealed class ServiceHostRuntime : IServiceHostRuntime
{
    /// <inheritdoc/>
    public Task RunAsync(IHost host, CancellationToken cancellationToken) => host.RunAsync(cancellationToken);

    /// <inheritdoc/>
    public void RunService(ServiceBase service) => ServiceBase.Run(service);

    /// <inheritdoc/>
    public void StopService(ServiceBase service) => service.Stop();
}
