// Copyright (c) 2019-2024 ReactiveUI Association Incorporated. All rights reserved.
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

namespace ReactiveMarbles.Extensions.Hosting.Plugins
{
    /// <summary>
    /// HostedServiceBase.
    /// </summary>
    /// <typeparam name="T">The type of Service.</typeparam>
    /// <seealso cref="IHostedService" />
    /// <seealso cref="IDisposable" />
    /// <remarks>
    /// Initializes a new instance of the <see cref="HostedServiceBase{T}"/> class.
    /// </remarks>
    /// <param name="logger">The logger.</param>
    /// <param name="hostApplicationLifetime">The host application lifetime.</param>
    public class HostedServiceBase<T>(ILogger<T> logger, IHostApplicationLifetime hostApplicationLifetime) : IHostedService, ICancelable
    {
        private bool _disposedValue;

        /// <summary>
        /// Gets the clean up.
        /// </summary>
        /// <value>
        /// The clean up.
        /// </value>
        public CompositeDisposable CleanUp { get; private set; } = [];

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>
        /// The logger.
        /// </value>
        public ILogger Logger { get; } = logger;

        /// <summary>
        /// Gets a value indicating whether gets a value that indicates whether the object is disposed.
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
        /// Called when [started].
        /// </summary>
        /// <returns>A Task.</returns>
        public virtual Task OnStarted() => Task.CompletedTask;

        /// <summary>
        /// Called when [started].
        /// </summary>
        public virtual void OnStopping()
        {
            Logger!.LogInformation($"{nameof(T)} Service OnStopping has been called.");
            CleanUp?.Dispose();
        }

        /// <summary>
        /// Called when [stopped].
        /// </summary>
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
}
