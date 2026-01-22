// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ReactiveMarbles.Extensions.Hosting.AppServices.Internal;

/// <summary>
/// Provides an IHostedService implementation that ensures only a single instance of the application runs by acquiring a
/// named mutex at startup.
/// </summary>
/// <remarks>If another instance of the application is already running and holds the mutex, this service will stop
/// the current application during startup. This helps prevent multiple instances from running concurrently in
/// environments where only one instance should be active.</remarks>
/// <param name="logger">The logger used to record diagnostic and operational information.</param>
/// <param name="hostEnvironment">The host environment information for the current application instance.</param>
/// <param name="hostApplicationLifetime">The application lifetime control used to manage startup and shutdown events.</param>
/// <param name="mutexBuilder">The mutex builder that provides configuration for creating and identifying the application mutex.</param>
internal class MutexLifetimeService(ILogger<MutexLifetimeService> logger, IHostEnvironment hostEnvironment, IHostApplicationLifetime hostApplicationLifetime, IMutexBuilder mutexBuilder) : IHostedService
{
    private readonly ILogger _logger = logger;
    private ResourceMutex? _resourceMutex;

    /// <summary>
    /// Starts the application asynchronously, acquiring a resource mutex to ensure a single active instance.
    /// </summary>
    /// <remarks>If another instance of the application is already running, the startup process will be
    /// stopped and the application will shut down. This method should be called during application initialization to
    /// enforce single-instance behavior.</remarks>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the startup operation.</param>
    /// <returns>A task that represents the asynchronous startup operation.</returns>
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

    /// <summary>
    /// Stops the operation asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the stop operation.</param>
    /// <returns>A task that represents the asynchronous stop operation.</returns>
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    /// <summary>
    /// Performs cleanup operations when the service is stopping.
    /// </summary>
    /// <remarks>This method releases resources held by the service, such as disposing of the resource mutex.
    /// It should be called as part of the service shutdown process to ensure proper resource management.</remarks>
    private void OnStopping()
    {
        _logger.LogInformation("OnStopping has been called, closing mutex.");
        _resourceMutex?.Dispose();
    }
}
