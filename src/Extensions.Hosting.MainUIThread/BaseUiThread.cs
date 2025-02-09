// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ReactiveMarbles.Extensions.Hosting.UiThread;

/// <summary>
/// This contains the base logic for the UI thread (WPF, WinForms).
/// </summary>
/// <typeparam name="T">The Type.</typeparam>
/// <seealso cref="System.IDisposable" />
public abstract class BaseUiThread<T> : IDisposable
    where T : class, IUiContext
{
    private readonly ManualResetEvent _serviceManualResetEvent = new(false);
    private readonly IHostApplicationLifetime? _hostApplicationLifetime;
    private bool _disposedValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseUiThread{T}"/> class.
    /// Constructor which is called from the IWinFormsContext.
    /// </summary>
    /// <param name="serviceProvider">IServiceProvider this is scoped, this disposed.</param>
    protected BaseUiThread(IServiceProvider serviceProvider)
    {
        UiContext = serviceProvider.GetService<T>();
        _hostApplicationLifetime = serviceProvider.GetService<IHostApplicationLifetime>();
        ServiceProvider = serviceProvider;

        // Create a thread which runs the UI
        var newUiThread = new Thread(InternalUiThreadStart)
        {
            IsBackground = true
        };

        // Set the apartment state
        newUiThread.SetApartmentState(ApartmentState.STA);

        // Start the new UI thread
        newUiThread.Start();
    }

    /// <summary>
    /// Gets the IUiContext.
    /// </summary>
    protected T? UiContext { get; }

    /// <summary>
    /// Gets the IServiceProvider used by all IUiContext implementations.
    /// </summary>
    protected IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// Start the DI service on the thread.
    /// </summary>
    public void Start() =>
        _serviceManualResetEvent.Set(); // Make the UI thread go

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
    /// Do all the pre work, before the UI thread can start.
    /// </summary>
    protected abstract void PreUiThreadStart();

    /// <summary>
    /// Implement all the code which is needed to run the actual UI.
    /// </summary>
    protected abstract void UiThreadStart();

    /// <summary>
    /// Handle the application exit.
    /// </summary>
    protected void HandleApplicationExit()
    {
        UiContext!.IsRunning = false;
        if (!UiContext.IsLifetimeLinked)
        {
            return;
        }

        if (_hostApplicationLifetime?.ApplicationStopped.IsCancellationRequested == true || _hostApplicationLifetime?.ApplicationStopping.IsCancellationRequested == true)
        {
            return;
        }

        _hostApplicationLifetime?.StopApplication();
    }

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
                _serviceManualResetEvent.Dispose();
            }

            _disposedValue = true;
        }
    }

    /// <summary>
    /// Start UI.
    /// </summary>
    private void InternalUiThreadStart()
    {
        // Do the pre initialization, if any
        PreUiThreadStart();

        // Wait for the startup
        _serviceManualResetEvent.WaitOne();

        // Run the application
        UiContext!.IsRunning = true;

        // Run the actual code
        UiThreadStart();
    }
}
