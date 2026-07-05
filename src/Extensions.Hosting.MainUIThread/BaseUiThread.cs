// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ReactiveMarbles.Extensions.Hosting.UiThread;

/// <summary>Provides a base class for managing a UI thread and its associated context using dependency injection. Intended for use with UI frameworks that require operations to run on a dedicated thread.</summary>
/// <remarks>This class is designed to be inherited by platform-specific UI thread implementations. It manages the
/// lifecycle of a UI thread, including initialization, startup synchronization, and graceful shutdown. The class uses
/// dependency injection to provide services and context to the UI thread. Derived classes must implement the <see
/// cref="PreUiThreadStart"/> and <see cref="UiThreadStart"/> methods to define custom initialization and UI execution
/// logic.</remarks>
/// <typeparam name="T">The type of UI context to associate with the thread. Must implement <see cref="IUiContext"/>.</typeparam>
public abstract class BaseUiThread<T> : IDisposable
    where T : class, IUiContext
{
    /// <summary>Stores the service manual reset event value.</summary>
    private readonly ManualResetEventSlim _serviceManualResetEvent = new(false);

    /// <summary>Stores the host application lifetime value.</summary>
    private readonly IHostApplicationLifetime? _hostApplicationLifetime;

    /// <summary>Stores the use dedicated ui thread value.</summary>
    private readonly bool _useDedicatedUiThread;

    /// <summary>Stores the disposed value.</summary>
    private bool _disposedValue;

    /// <summary>Initializes a new instance of the <see cref="BaseUiThread{T}"/> class and starts a dedicated UI thread using the specified. service provider.</summary>
    /// <remarks>The constructor creates and starts a new background thread to run the UI. On Windows
    /// platforms, the thread is set to single-threaded apartment (STA) state to support UI frameworks that require it.
    /// The provided service provider is used to resolve dependencies needed by the UI thread and is stored for later
    /// use.</remarks>
    /// <param name="serviceProvider">The service provider used to resolve required services for the UI thread. Cannot be null.</param>
    /// <param name="useDedicatedUiThread">If set to <c>true</c>, a dedicated UI thread is created and started immediately; otherwise, UI initialization and startup run on the caller thread when <see cref="Start"/> is invoked.</param>
    protected BaseUiThread(IServiceProvider serviceProvider, bool useDedicatedUiThread = true)
    {
        if (serviceProvider is null)
        {
            throw new ArgumentNullException(nameof(serviceProvider));
        }

        UiContext = serviceProvider.GetRequiredService<T>();
        _hostApplicationLifetime = serviceProvider.GetService<IHostApplicationLifetime>();
        ServiceProvider = serviceProvider;
        _useDedicatedUiThread = useDedicatedUiThread;

        if (!_useDedicatedUiThread)
        {
            return;
        }

        // Create a thread which runs the UI
        var newUiThread = new Thread(InternalUiThreadStart)
        {
            IsBackground = true
        };

#if NET5_0_OR_GREATER
        if (OperatingSystem.IsWindows())
#else
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
#endif
        {
            // Set the apartment state for Windows desktop UI frameworks.
            newUiThread.SetApartmentState(ApartmentState.STA);
        }

        // Start the new UI thread
        newUiThread.Start();
    }

    /// <summary>Gets the UI context associated with the current instance.</summary>
    protected T UiContext { get; }

    /// <summary>Gets the service provider used to resolve application services.</summary>
    protected IServiceProvider ServiceProvider { get; }

    /// <summary>Signals the service to begin processing or resume operation.</summary>
    /// <remarks>Call this method to allow the service to proceed if it is waiting for a start signal. This
    /// method is typically used to control the execution flow of a service that waits for an external trigger before
    /// starting.</remarks>
    public void Start()
    {
        if (_useDedicatedUiThread)
        {
            _serviceManualResetEvent.Set(); // Make the UI thread go
            return;
        }

        InternalUiThreadStart();
    }

    /// <summary>Releases all resources used by the current instance of the class.</summary>
    /// <remarks>Call this method when you are finished using the object to release unmanaged resources and
    /// perform other cleanup operations. After calling Dispose, the object should not be used further.</remarks>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>Performs custom initialization logic before the UI thread starts.</summary>
    /// <remarks>Override this method to execute any setup required prior to launching the UI thread. This
    /// method is called during the application startup sequence, before any UI components are created or
    /// shown.</remarks>
    protected abstract void PreUiThreadStart();

    /// <summary>Executes the main logic for the UI thread. Called when the UI thread is started.</summary>
    /// <remarks>Override this method to implement the operations that should run on the UI thread. This
    /// method is invoked on the thread designated for UI processing and typically contains the application's message
    /// loop or event handling logic.</remarks>
    protected abstract void UiThreadStart();

    /// <summary>Handles application exit by updating the UI context and initiating application shutdown if appropriate.</summary>
    /// <remarks>This method sets the UI context to indicate that the application is no longer running. If the
    /// UI context is configured to link its lifetime to the host application, this method will request application
    /// shutdown unless the application is already stopping or has stopped. Intended to be called during application
    /// exit procedures to ensure proper shutdown coordination.</remarks>
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
            _serviceManualResetEvent.Dispose();
        }

        _disposedValue = true;
    }

    /// <summary>Initializes and starts the UI thread, performing any required pre-initialization and waiting for the service to signal readiness before running the main UI logic.</summary>
    /// <remarks>This method is intended for internal use to coordinate the startup sequence of the UI thread.
    /// It ensures that any necessary pre-initialization is completed and that the service is ready before the UI thread
    /// begins execution. This method should not be called directly by user code.</remarks>
    private void InternalUiThreadStart()
    {
        // Do the pre initialization, if any
        PreUiThreadStart();

        if (_useDedicatedUiThread)
        {
            // Wait for the startup
            try
            {
                _serviceManualResetEvent.Wait();
            }
            catch (ObjectDisposedException)
            {
                // If the event was disposed during shutdown just return
                return;
            }
        }

        // Run the application
        UiContext!.IsRunning = true;

        // Run the actual code
        UiThreadStart();
    }
}
