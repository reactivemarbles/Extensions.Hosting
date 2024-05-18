// Copyright (c) 2019-2024 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ServiceProcess;
using Microsoft.Extensions.Hosting;

namespace ReactiveMarbles.Extensions.Hosting.PluginService;

/// <summary>
/// ServiceBaseLifetime.
/// </summary>
/// <seealso cref="ServiceBase" />
/// <seealso cref="IHostLifetime" />
/// <remarks>
/// Initializes a new instance of the <see cref="ServiceBaseLifetime"/> class.
/// </remarks>
/// <param name="applicationLifetime">The application lifetime.</param>
/// <exception cref="ArgumentNullException">applicationLifetime.</exception>
public class ServiceBaseLifetime(IHostApplicationLifetime applicationLifetime) : ServiceBase, IHostLifetime
{
    private readonly TaskCompletionSource<object> _delayStart = new();

    private IHostApplicationLifetime ApplicationLifetime { get; } = applicationLifetime ?? throw new ArgumentNullException(nameof(applicationLifetime));

    /// <summary>
    /// Called at the start of <see cref="IHost.StartAsync(CancellationToken)" /> which will wait until it's complete before
    /// continuing. This can be used to delay startup until signaled by an external event.
    /// </summary>
    /// <param name="cancellationToken">Used to indicate when stop should no longer be graceful.</param>
    /// <returns>
    /// A <see cref="Task" />.
    /// </returns>
    public Task WaitForStartAsync(CancellationToken cancellationToken)
    {
        cancellationToken.Register(() => _delayStart.TrySetCanceled());
        ApplicationLifetime.ApplicationStopping.Register(Stop);

        new Thread(Run).Start(); // Otherwise this would block and prevent IHost.StartAsync from finishing.
        return _delayStart.Task;
    }

    /// <summary>
    /// Called from <see cref="IHost.StopAsync(CancellationToken)" /> to indicate that the host is stopping and it's time to shut down.
    /// </summary>
    /// <param name="cancellationToken">Used to indicate when stop should no longer be graceful.</param>
    /// <returns>
    /// A <see cref="Task" />.
    /// </returns>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        Stop();
        return Task.CompletedTask;
    }

    /// <summary>
    /// When implemented in a derived class, executes when a Start command is sent to the service by the Service Control Manager (SCM) or when the operating system starts (for a service that starts automatically). Specifies actions to take when the service starts.
    /// </summary>
    /// <param name="args">Data passed by the start command.</param>
    protected override void OnStart(string[] args)
    {
        _delayStart.TrySetResult(null!);
        base.OnStart(args);
    }

    /// <inheritdoc/>
    protected override void OnStop()
    {
        ApplicationLifetime.StopApplication();
        base.OnStop();
    }

    private void Run()
    {
        try
        {
            Run(this); // This blocks until the service is stopped.
            _delayStart.TrySetException(new InvalidOperationException("Stopped without starting"));
        }
        catch (Exception ex)
        {
            _delayStart.TrySetException(ex);
        }
    }
}
