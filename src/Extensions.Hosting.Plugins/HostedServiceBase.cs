// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ReactiveMarbles.Extensions.Hosting.Plugins;

/// <summary>
/// Provides a base implementation for a hosted service with support for resource cleanup, logging, and application
/// lifetime events.
/// </summary>
/// <remarks>This base class integrates with the application's lifetime events to manage service startup,
/// shutdown, and resource disposal. It is intended to be inherited by services that require coordinated startup and
/// cleanup logic. The class is thread-safe for typical usage scenarios. Derived classes can override the OnStarted,
/// OnStopping, and OnStopped methods to implement custom behavior during the service lifecycle.</remarks>
/// <typeparam name="T">The type representing the hosted service. Used for logging and identification purposes.</typeparam>
/// <param name="logger">The logger instance used to record diagnostic and operational information for the hosted service. Cannot be null.</param>
/// <param name="hostApplicationLifetime">The application lifetime manager used to register callbacks for application start and stop events. Cannot be null.</param>
public class HostedServiceBase<T>(ILogger<T> logger, IHostApplicationLifetime hostApplicationLifetime) : IHostedService, ICancelable
{
    private bool _disposedValue;

    /// <summary>
    /// Gets the collection of disposables that are cleaned up when the owning object is disposed.
    /// </summary>
    /// <remarks>Use this property to register resources or subscriptions that should be disposed together
    /// with the parent object. Items added to this collection will be disposed automatically when the parent is
    /// disposed.</remarks>
    public CompositeDisposable CleanUp { get; private set; } = [];

    /// <summary>
    /// Gets the logger instance used for recording diagnostic and operational messages.
    /// </summary>
    public ILogger Logger { get; } = logger;

    /// <summary>
    /// Gets a value indicating whether the object has been disposed.
    /// </summary>
    public bool IsDisposed => CleanUp.IsDisposed;

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        hostApplicationLifetime.ApplicationStarted.Register(() =>
        {
            Logger!.LogInformation($"{nameof(T)} Service OnStarted has been called.");
            CleanUp = [];

            // Create a Task with cancellation on Taskpool call all from inside to ensure teardown.
            CleanUp.Add(Observable.Create<Unit>(async _ =>
              {
                  await OnStarted();
                  return CleanUp;
              })
                  .Retry()
                  .Subscribe());
        });
        hostApplicationLifetime.ApplicationStopping.Register(OnStopping);
        hostApplicationLifetime.ApplicationStopped.Register(OnStopped);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken) =>
        Task.CompletedTask;

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Invoked when the associated process or service has started.
    /// </summary>
    /// <remarks>Override this method to perform custom actions when the process or service starts. This
    /// method is called after the start operation has been initiated.</remarks>
    /// <returns>A task that represents the asynchronous operation. The default implementation returns a completed task.</returns>
    public virtual Task OnStarted() => Task.CompletedTask;

    /// <summary>
    /// Performs tasks required when the service is stopping, such as releasing resources and logging shutdown
    /// information.
    /// </summary>
    /// <remarks>Override this method to implement custom shutdown logic for the service. Call the base
    /// implementation to ensure standard cleanup is performed.</remarks>
    public virtual void OnStopping()
    {
        Logger!.LogInformation($"{nameof(T)} Service OnStopping has been called.");
        CleanUp?.Dispose();
    }

    /// <summary>
    /// Called when the service has stopped to perform any necessary cleanup or finalization logic.
    /// </summary>
    /// <remarks>Override this method in a derived class to implement custom actions that should occur when
    /// the service stops. The base implementation logs a message indicating that the service has stopped.</remarks>
    public virtual void OnStopped() =>
        Logger!.LogInformation($"{nameof(T)} Service OnStopped has been called.");

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                CleanUp.Dispose();
            }

            _disposedValue = true;
        }
    }
}
