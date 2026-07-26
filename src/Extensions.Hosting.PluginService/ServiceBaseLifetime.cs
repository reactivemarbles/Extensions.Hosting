// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ServiceProcess;
using Microsoft.Extensions.Hosting;

#if REACTIVE_SHIM
namespace ReactiveMarbles.Extensions.Hosting.Reactive.PluginService;
#else
namespace ReactiveMarbles.Extensions.Hosting.PluginService;
#endif

/// <summary>Provides an <see cref="IHostLifetime"/> implementation backed by <see cref="ServiceBase"/>.</summary>
/// <seealso cref="ServiceBase" />
/// <seealso cref="IHostLifetime" />
public class ServiceBaseLifetime : ServiceBase, IHostLifetime
{
    /// <summary>Stores the delay start value.</summary>
    private readonly TaskCompletionSource<object> _delayStart = new(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <summary>Stores the operating-system service runtime.</summary>
    private readonly IServiceHostRuntime _runtime;

    /// <summary>Initializes a new instance of the <see cref="ServiceBaseLifetime"/> class.</summary>
    /// <param name="applicationLifetime">The application lifetime.</param>
    public ServiceBaseLifetime(IHostApplicationLifetime applicationLifetime)
        : this(applicationLifetime, ServiceHost.Runtime)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="ServiceBaseLifetime"/> class with a service runtime.</summary>
    /// <param name="applicationLifetime">The application lifetime.</param>
    /// <param name="runtime">The operating-system service runtime.</param>
    internal ServiceBaseLifetime(IHostApplicationLifetime applicationLifetime, IServiceHostRuntime runtime)
    {
        ApplicationLifetime = applicationLifetime ?? throw new ArgumentNullException(nameof(applicationLifetime));
        _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
    }

    /// <summary>Gets the host application lifetime used to stop the application from service callbacks.</summary>
    private IHostApplicationLifetime ApplicationLifetime { get; }

    /// <summary>Called at the start of <see cref="IHost.StartAsync(CancellationToken)" /> and waits until startup is signaled by an external event.</summary>
    /// <param name="cancellationToken">Used to indicate when stop should no longer be graceful.</param>
    /// <returns>
    /// A <see cref="Task" />.
    /// </returns>
    public Task WaitForStartAsync(CancellationToken cancellationToken)
    {
        _ = cancellationToken.Register(
            static state => _ = ((TaskCompletionSource<object>)state!).TrySetCanceled(),
            _delayStart);
        _ = ApplicationLifetime.ApplicationStopping.Register(
            static state => ((ServiceBaseLifetime)state!).StopService(),
            this);

        new Thread(Run).Start(); // Otherwise this would block and prevent IHost.StartAsync from finishing.
        return _delayStart.Task;
    }

    /// <summary>Called from <see cref="IHost.StopAsync(CancellationToken)" /> to indicate that the host is stopping and it's time to shut down.</summary>
    /// <param name="cancellationToken">Used to indicate when stop should no longer be graceful.</param>
    /// <returns>
    /// A <see cref="Task" />.
    /// </returns>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        StopService();
        return Task.CompletedTask;
    }

    /// <summary>
    /// When implemented in a derived class, executes when a Start command is sent to the service by the Service Control
    /// Manager or when the operating system starts. Specifies actions to take when the service starts.
    /// </summary>
    /// <param name="args">Data passed by the start command.</param>
    protected override void OnStart(string[] args)
    {
        _ = _delayStart.TrySetResult(null!);
        base.OnStart(args);
    }

    /// <inheritdoc/>
    protected override void OnStop()
    {
        ApplicationLifetime.StopApplication();
        base.OnStop();
    }

    /// <summary>Runs the service control loop on a background thread.</summary>
    private void Run()
    {
        try
        {
            _runtime.RunService(this); // This blocks until the service is stopped.
            _ = _delayStart.TrySetException(new InvalidOperationException("Stopped without starting"));
        }
        catch (Exception ex)
        {
            _ = _delayStart.TrySetException(ex);
        }
    }

    /// <summary>Requests that the configured runtime stops this service.</summary>
    private void StopService() => _runtime.StopService(this);
}
