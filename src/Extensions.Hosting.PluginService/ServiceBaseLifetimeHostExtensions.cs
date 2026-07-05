// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;

namespace ReactiveMarbles.Extensions.Hosting.PluginService;

/// <summary>Provides extension methods for configuring host lifetimes to enable running .NET applications as Windows services or with console lifetime support.</summary>
/// <remarks>These extensions allow integration of service-based or console-based lifetimes into host builders,
/// enabling applications to run as Windows services or respond to console signals for graceful shutdown. Methods in
/// this class are typically used during application startup to configure the desired lifetime behavior.</remarks>
public static class ServiceBaseLifetimeHostExtensions
{
    /// <summary>Provides extension members for this receiver.</summary>
    /// <param name="hostBuilder">The receiver instance.</param>
    extension(HostApplicationBuilder hostBuilder)
    {
        /// <summary>Runs the application as a Windows service using the specified host builder.</summary>
        /// <remarks>This method configures the application to use Windows service lifetime management. It should
        /// be called when running the application as a Windows service. If called in a non-Windows environment, the
        /// behavior may differ.</remarks>
        /// <param name="cancellationToken">A cancellation token that can be used to request cancellation of the service run operation. The default value is
        /// <see cref="CancellationToken.None"/>.</param>
        /// <returns>A task that represents the lifetime of the running service. The task completes when the service stops.</returns>
        public Task RunAsServiceAsync(CancellationToken cancellationToken = default)
        {
            _ = UseServiceBaseLifetime((IHostApplicationBuilder)hostBuilder);
            return hostBuilder.Build().RunAsync(cancellationToken);
        }
    }

    /// <summary>Provides extension members for this receiver.</summary>
    /// <param name="hostBuilder">The receiver instance.</param>
    extension(IHostApplicationBuilder hostBuilder)
    {
        /// <summary>Enables Windows Service lifetime management for the application, allowing it to be run as a Windows Service using ServiceBase.</summary>
        /// <remarks>This method configures the application to use <see cref="ServiceBaseLifetime"/>, which
        /// enables integration with Windows Service control events. Use this method when deploying the application as a
        /// Windows Service. This should not be used in environments where Windows Services are not supported.</remarks>
        /// <returns>The same instance of <see cref="IHostApplicationBuilder"/> for chaining further configuration.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="hostBuilder"/> is null.</exception>
        public IHostApplicationBuilder UseServiceBaseLifetime()
        {
            if (hostBuilder is null)
            {
                throw new ArgumentNullException(nameof(hostBuilder));
            }

            _ = hostBuilder.Services.AddSingleton<IHostLifetime, ServiceBaseLifetime>();
            return hostBuilder;
        }

        /// <summary>Configures the host to use a console lifetime, enabling the application to be controlled by console events such as Ctrl+C or SIGTERM.</summary>
        /// <remarks>This method registers <see cref="ConsoleLifetime"/> as the <see cref="IHostLifetime"/>
        /// implementation, allowing the application to respond to console signals for graceful shutdown. Use this method
        /// when building console applications that require proper handling of shutdown events.</remarks>
        /// <returns>The same instance of <see cref="IHostApplicationBuilder"/> for chaining further configuration.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="hostBuilder"/> is null.</exception>
        public IHostApplicationBuilder UseConsoleLifetime()
        {
            if (hostBuilder is null)
            {
                throw new ArgumentNullException(nameof(hostBuilder));
            }

            _ = hostBuilder.Services.AddSingleton<IHostLifetime, ConsoleLifetime>();
            return hostBuilder;
        }
    }

    /// <summary>Provides extension members for this receiver.</summary>
    /// <param name="hostBuilder">The receiver instance.</param>
    extension(IHostBuilder hostBuilder)
    {
        /// <summary>Enables Windows Service lifetime management for the host, allowing the application to be run as a Windows Service.</summary>
        /// <remarks>This method configures the host to use <see cref="ServiceBaseLifetime"/>, which enables
        /// integration with Windows Service control events. Use this method when deploying applications as Windows Services
        /// to ensure proper start and stop behavior.</remarks>
        /// <returns>The same instance of <see cref="IHostBuilder"/> for chaining, or <see langword="null"/> if <paramref
        /// name="hostBuilder"/> is null.</returns>
        public IHostBuilder? UseServiceBaseLifetime() =>
            hostBuilder?.ConfigureServices(services => _ = services.AddSingleton<IHostLifetime, ServiceBaseLifetime>());

        /// <summary>Runs the host as a Windows service or systemd service, enabling integration with the operating system's service management.</summary>
        /// <remarks>This method configures the host to run as a background service using the appropriate service
        /// infrastructure for the current platform (Windows service or systemd on Linux). It should be called instead of
        /// RunAsync when deploying as a service.</remarks>
        /// <param name="cancellationToken">A cancellation token that can be used to request cancellation of the service run operation.</param>
        /// <returns>A task that represents the lifetime of the service. The task completes when the service stops.</returns>
        public Task RunAsServiceAsync(CancellationToken cancellationToken = default) =>
            hostBuilder.UseServiceBaseLifetime()!.Build().RunAsync(cancellationToken);
    }
}
