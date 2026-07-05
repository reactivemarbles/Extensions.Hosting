// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Extensions.Hosting;
using ReactiveUI.Builder;
using Splat;
using Splat.Microsoft.Extensions.DependencyInjection;

#if REACTIVE_SHIM
namespace ReactiveMarbles.Extensions.Hosting.Reactive.ReactiveUI;
#else
namespace ReactiveMarbles.Extensions.Hosting.ReactiveUI;
#endif

/// <summary>Provides extension methods for configuring ReactiveUI with Microsoft dependency injection in .NET host builder scenarios.</summary>
/// <remarks>These extension methods enable seamless integration of ReactiveUI and Splat with the Microsoft
/// dependency injection system, supporting both generic host and application host builder patterns. Use these methods
/// to set up ReactiveUI infrastructure when building WPF or other .NET applications that rely on dependency
/// injection.</remarks>
public static class HostBuilderReactiveUiExtensions
{
    /// <summary>Provides extension members for this receiver.</summary>
    /// <param name="host">The receiver instance.</param>
    extension(IHost host)
    {
        /// <summary>Configures the Microsoft dependency resolver and invokes a custom container factory using the host's service provider.</summary>
        /// <remarks>This method enables integration with the Microsoft dependency injection system and allows
        /// further customization of the service container. The method returns the same host instance to support fluent
        /// configuration.</remarks>
        /// <param name="containerFactory">An action that receives the host's service provider for additional container configuration. Cannot be null.</param>
        /// <returns>The original host instance after configuration, or null if the input host is null.</returns>
        public IHost? MapSplatLocator(Action<IServiceProvider?> containerFactory)
        {
            var c = host?.Services;
            c?.UseMicrosoftDependencyResolver();
            containerFactory?.Invoke(c);
            return host;
        }
    }

    /// <summary>Provides extension members for this receiver.</summary>
    /// <param name="hostBuilder">The receiver instance.</param>
    extension(IHostApplicationBuilder hostBuilder)
    {
        /// <summary>Configures Splat to use the Microsoft dependency resolver within the specified host application builder.</summary>
        /// <remarks>This method enables Splat and ReactiveUI to resolve dependencies using the
        /// Microsoft.Extensions.DependencyInjection container. It should be called during application startup before
        /// building the host.</remarks>
        /// <returns>The same instance of <see cref="IHostApplicationBuilder"/> with Splat configured to use the Microsoft dependency
        /// resolver.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="hostBuilder"/> is null.</exception>
        public IHostApplicationBuilder ConfigureSplatForMicrosoftDependencyResolver()
        {
            if (hostBuilder is null)
            {
                throw new ArgumentNullException(nameof(hostBuilder));
            }

            hostBuilder.Services.UseMicrosoftDependencyResolver();
            var reactiveUiBuilder = AppLocator.CurrentMutable.CreateReactiveUIBuilder()
                .WithRegistration(r => r.InitializeSplat())
                .WithWpf();
            _ = reactiveUiBuilder.BuildApp();
            return hostBuilder;
        }
    }

    /// <summary>Provides extension members for this receiver.</summary>
    /// <param name="hostBuilder">The receiver instance.</param>
    extension(IHostBuilder hostBuilder)
    {
        /// <summary>Configures the specified host builder to use Splat with the Microsoft dependency resolver for dependency injection in a WPF application.</summary>
        /// <remarks>This method sets up Splat and ReactiveUI to use the Microsoft dependency resolver, enabling
        /// seamless integration with the application's dependency injection system. It is intended for use in WPF
        /// applications that utilize ReactiveUI and Splat for service location.</remarks>
        /// <returns>The same <see cref="IHostBuilder"/> instance so that additional configuration calls can be chained.</returns>
        public IHostBuilder ConfigureSplatForMicrosoftDependencyResolver() =>
            hostBuilder.ConfigureServices((serviceCollection) =>
            {
                serviceCollection.UseMicrosoftDependencyResolver();
                var reactiveUiBuilder = AppLocator.CurrentMutable.CreateReactiveUIBuilder()
                    .WithRegistration(r => r.InitializeSplat())
                    .WithWpf();
                _ = reactiveUiBuilder.BuildApp();
            });
    }
}
