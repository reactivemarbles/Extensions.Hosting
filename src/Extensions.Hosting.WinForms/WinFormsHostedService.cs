// Copyright (c) 2019-2024 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReactiveMarbles.Extensions.Hosting.WinForms.Internals;

namespace ReactiveMarbles.Extensions.Hosting.WinForms;

/// <summary>
/// This hosts a WinForms service, making sure the lifecycle is managed.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="WinFormsHostedService"/> class.
/// The constructor which takes all the DI objects.
/// </remarks>
/// <param name="logger">ILogger.</param>
/// <param name="winFormsThread">WinFormsThread.</param>
/// <param name="winFormsContext">IWinFormsContext.</param>
public class WinFormsHostedService(ILogger<WinFormsHostedService> logger, WinFormsThread winFormsThread, IWinFormsContext winFormsContext) : IHostedService
{
    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.CompletedTask;
        }

        winFormsThread.Start();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (winFormsContext.IsRunning)
        {
            logger.LogDebug("Stopping WinForms application.");
            await winFormsContext.Dispatcher!.InvokeAsync(() =>
            {
                // Graceful close, otherwise finalizes try to dispose forms.
                foreach (var form in Application.OpenForms.Cast<Form>().ToList())
                {
                    try
                    {
                        form.Close();
                        form.Dispose();
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Couldn't cleanup a Form");
                    }
                }

                Application.ExitThread();
            });
        }
    }
}
