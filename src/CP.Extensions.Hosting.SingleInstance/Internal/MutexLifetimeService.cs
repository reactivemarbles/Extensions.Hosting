// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CP.Extensions.Hosting.AppServices.Internal;

/// <summary>
/// This maintains the mutex lifetime.
/// </summary>
internal class MutexLifetimeService(ILogger<MutexLifetimeService> logger, IHostEnvironment hostEnvironment, IHostApplicationLifetime hostApplicationLifetime, IMutexBuilder mutexBuilder) : IHostedService
{
    private readonly ILogger _logger = logger;
    private ResourceMutex? _resourceMutex;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _resourceMutex = ResourceMutex.Create(null, mutexBuilder.MutexId, hostEnvironment.ApplicationName, mutexBuilder.IsGlobal);

        hostApplicationLifetime.ApplicationStopping.Register(OnStopping);
        if (!_resourceMutex.IsLocked)
        {
            mutexBuilder.WhenNotFirstInstance?.Invoke(hostEnvironment, _logger);
            _logger.LogDebug("Application {applicationName} already running, stopping application.", hostEnvironment.ApplicationName);
            hostApplicationLifetime.StopApplication();
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private void OnStopping()
    {
        _logger.LogInformation("OnStopping has been called, closing mutex.");
        _resourceMutex?.Dispose();
    }
}
