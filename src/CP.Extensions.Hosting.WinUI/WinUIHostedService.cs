// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;
using CP.Extensions.Hosting.WinUI.Internals;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CP.Extensions.Hosting.WinUI
{
    /// <summary>
    /// This hosts a WinUI service, making sure the lifecycle is managed.
    /// </summary>
    public class WinUIHostedService : IHostedService
    {
        private readonly ILogger<WinUIHostedService> _logger;
        private readonly IWinUIContext _winUIContext;
        private readonly WinUIThread _winUIThread;

        /// <summary>
        /// Initializes a new instance of the <see cref="WinUIHostedService"/> class.
        /// The constructor which takes all the DI objects.
        /// </summary>
        /// <param name="logger">ILogger.</param>
        /// <param name="winUIThread">WinUIThread.</param>
        /// <param name="winUIContext">IWinUIContext.</param>
        public WinUIHostedService(ILogger<WinUIHostedService> logger, WinUIThread winUIThread, IWinUIContext winUIContext)
        {
            _logger = logger;
            _winUIThread = winUIThread;
            _winUIContext = winUIContext;
        }

        /// <inheritdoc />
        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.CompletedTask;
            }

            // Make the UI thread go
            _winUIThread.Start();
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_winUIContext.IsRunning)
            {
                _logger.LogDebug("Stopping WinUI due to application exit.");

                // Stop application
                var completion = new TaskCompletionSource();
                _winUIContext.Dispatcher?.TryEnqueue(() =>
                {
                    _winUIContext.WinUIApplication?.Exit();
                    completion.SetResult();
                });
                await completion.Task;
            }
        }
    }
}
