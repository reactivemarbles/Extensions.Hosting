// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

#if REACTIVE_SHIM
namespace ReactiveMarbles.Extensions.Hosting.Reactive.Plugins;
#else
namespace ReactiveMarbles.Extensions.Hosting.Plugins;
#endif

/// <summary>Provides a base implementation for a hosted service with support for resource cleanup, logging, and application lifetime events.</summary>
/// <remarks>This base class integrates with the application's lifetime events to manage service startup,
/// shutdown, and resource disposal. It is intended to be inherited by services that require coordinated startup and
/// cleanup logic. The class is thread-safe for typical usage scenarios. Derived classes can override the OnStarted,
/// OnStopping, and OnStopped methods to implement custom behavior during the service lifecycle.</remarks>
/// <typeparam name="T">The type representing the hosted service. Used for logging and identification purposes.</typeparam>
/// <param name="logger">The logger instance used to record diagnostic and operational information for the hosted service. Cannot be null.</param>
/// <param name="hostApplicationLifetime">The application lifetime manager used to register callbacks for application start and stop events. Cannot be null.</param>
public class HostedServiceBase<T>(ILogger<T> logger, IHostApplicationLifetime hostApplicationLifetime) : IHostedService, ReactiveUI.Primitives.Disposables.IsDisposed
{
    /// <summary>Logs that the hosted service started.</summary>
    private static readonly Action<ILogger, string, Exception?> _serviceStarted =
        LoggerMessage.Define<string>(LogLevel.Information, new EventId(1, nameof(_serviceStarted)), "{ServiceName} Service OnStarted has been called.");

    /// <summary>Logs that the hosted service is stopping.</summary>
    private static readonly Action<ILogger, string, Exception?> _serviceStopping =
        LoggerMessage.Define<string>(LogLevel.Information, new EventId(2, nameof(_serviceStopping)), "{ServiceName} Service OnStopping has been called.");

    /// <summary>Logs that the hosted service stopped.</summary>
    private static readonly Action<ILogger, string, Exception?> _serviceStopped =
        LoggerMessage.Define<string>(LogLevel.Information, new EventId(3, nameof(_serviceStopped)), "{ServiceName} Service OnStopped has been called.");

    /// <summary>Logs that the hosted service start callback failed.</summary>
    private static readonly Action<ILogger, string, Exception?> _serviceStartFailed =
        LoggerMessage.Define<string>(LogLevel.Error, new EventId(4, nameof(_serviceStartFailed)), "{ServiceName} Service OnStarted failed.");

    /// <summary>Stores the disposed value.</summary>
    private bool _disposedValue;

    /// <summary>Gets the collection of disposables that are cleaned up when the owning object is disposed.</summary>
    /// <remarks>Use this property to register resources or subscriptions that should be disposed together
    /// with the parent object. Items added to this collection will be disposed automatically when the parent is
    /// disposed.</remarks>
    public CompositeDisposable CleanUp { get; private set; } = [];

    /// <summary>Gets the logger instance used for recording diagnostic and operational messages.</summary>
    public ILogger Logger { get; } = logger;

    /// <summary>Gets a value indicating whether the object has been disposed.</summary>
    public bool IsDisposed => CleanUp.IsDisposed;

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ = hostApplicationLifetime.ApplicationStarted.Register(() =>
        {
            _serviceStarted(Logger, nameof(T), null);
            CleanUp = [];
            _ = RunOnStartedAsync();
        });
        _ = hostApplicationLifetime.ApplicationStopping.Register(OnStopping);
        _ = hostApplicationLifetime.ApplicationStopped.Register(OnStopped);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken) =>
        Task.CompletedTask;

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>Invoked when the associated process or service has started.</summary>
    /// <remarks>Override this method to perform custom actions when the process or service starts. This
    /// method is called after the start operation has been initiated.</remarks>
    /// <returns>A task that represents the asynchronous operation. The default implementation returns a completed task.</returns>
    public virtual Task OnStarted() => Task.CompletedTask;

    /// <summary>Performs tasks required when the service is stopping, such as releasing resources and logging shutdown information.</summary>
    /// <remarks>Override this method to implement custom shutdown logic for the service. Call the base
    /// implementation to ensure standard cleanup is performed.</remarks>
    public virtual void OnStopping()
    {
        _serviceStopping(Logger, nameof(T), null);
        CleanUp.Dispose();
    }

    /// <summary>Called when the service has stopped to perform any necessary cleanup or finalization logic.</summary>
    /// <remarks>Override this method in a derived class to implement custom actions that should occur when
    /// the service stops. The base implementation logs a message indicating that the service has stopped.</remarks>
    public virtual void OnStopped() =>
        _serviceStopped(Logger, nameof(T), null);

    /// <summary>Releases unmanaged and - optionally - managed resources.</summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposedValue)
        {
            return;
        }

        if (disposing)
        {
            CleanUp.Dispose();
        }

        _disposedValue = true;
    }

    /// <summary>Runs the started callback and logs any exception without surfacing it on the lifetime callback.</summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task RunOnStartedAsync()
    {
        try
        {
            await OnStarted().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _serviceStartFailed(Logger, nameof(T), ex);
        }
    }
}
