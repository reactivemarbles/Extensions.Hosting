// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
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
/// Provides an implementation of IHostedService that manages the lifecycle of a WinForms application within a generic
/// host environment.
/// </summary>
/// <remarks>This service enables integration of a WinForms application into a .NET generic host, allowing the
/// application to participate in the host's startup and shutdown processes. It is typically used to coordinate graceful
/// startup and shutdown of WinForms UI within background services or desktop applications that leverage dependency
/// injection and hosting infrastructure.</remarks>
/// <param name="logger">The logger used to record diagnostic messages and operational events for the hosted service.</param>
/// <param name="winFormsThread">The thread responsible for running the WinForms message loop.</param>
/// <param name="winFormsContext">The context that provides access to the WinForms application's state and dispatcher.</param>
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
