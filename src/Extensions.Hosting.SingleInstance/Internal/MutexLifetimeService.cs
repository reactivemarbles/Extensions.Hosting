// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
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
    private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IHostEnvironment _hostEnvironment = hostEnvironment ?? throw new ArgumentNullException(nameof(hostEnvironment));
    private readonly IHostApplicationLifetime _hostApplicationLifetime = hostApplicationLifetime ?? throw new ArgumentNullException(nameof(hostApplicationLifetime));
    private readonly IMutexBuilder _mutexBuilder = mutexBuilder ?? throw new ArgumentNullException(nameof(mutexBuilder));
    private ResourceMutex? _resourceMutex;

    /// <summary>
    /// Initializes the resource mutex and starts the application asynchronously, handling scenarios where another
    /// instance may already be running.
    /// </summary>
    /// <remarks>If another instance of the application is already running, the method invokes the configured
    /// callback for non-primary instances and initiates application shutdown. This method should be called during
    /// application startup to ensure proper resource locking and instance management.</remarks>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the start operation before it completes.</param>
    /// <returns>A task that represents the asynchronous start operation. The task is completed when initialization is finished.</returns>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _resourceMutex = ResourceMutex.Create(_logger, _mutexBuilder.MutexId, _hostEnvironment.ApplicationName, _mutexBuilder.IsGlobal);

        _hostApplicationLifetime.ApplicationStopping.Register(OnStopping);
        if (!_resourceMutex.IsLocked)
        {
            _mutexBuilder.WhenNotFirstInstance?.Invoke(_hostEnvironment, _logger);
            _logger.LogDebug("Application {applicationName} already running, stopping application.", _hostEnvironment.ApplicationName);
            _hostApplicationLifetime.StopApplication();
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Attempts to stop the operation asynchronously, honoring cancellation requests.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to request cancellation of the stop operation.</param>
    /// <returns>A task that represents the asynchronous stop operation.</returns>
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    /// <summary>
    /// Performs cleanup operations when the service is stopping, including releasing resources held by the mutex.
    /// </summary>
    /// <remarks>This method should be called during the service shutdown sequence to ensure that any held
    /// resources are properly released. Failing to call this method may result in resource leaks or contention issues
    /// if the mutex is not disposed.</remarks>
    private void OnStopping()
    {
        _logger.LogInformation("OnStopping has been called, closing mutex.");
        _resourceMutex?.Dispose();
        _resourceMutex = null;
    }
}
