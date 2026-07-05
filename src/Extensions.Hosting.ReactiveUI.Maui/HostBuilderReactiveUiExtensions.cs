// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Extensions.Hosting;
#if REACTIVE_SHIM
using ReactiveUI.Reactive.Builder;
#else
using ReactiveUI.Builder;
#endif
using Splat;
using Splat.Microsoft.Extensions.DependencyInjection;

#if REACTIVE_SHIM
namespace ReactiveMarbles.Extensions.Hosting.Reactive.ReactiveUI;
#else
namespace ReactiveMarbles.Extensions.Hosting.ReactiveUI;
#endif

/// <summary>Provides extension methods for configuring ReactiveUI integration with Microsoft dependency injection in .NET host builders.</summary>
/// <remarks>These extensions enable seamless setup of ReactiveUI and Splat with Microsoft.Extensions.Hosting and
/// dependency injection. Use these methods to configure ReactiveUI services and ensure that Splat uses the Microsoft
/// dependency resolver within your application's host configuration pipeline.</remarks>
public static class HostBuilderReactiveUiExtensions
{
    /// <summary>Provides extension members for this receiver.</summary>
    /// <param name="host">The receiver instance.</param>
    extension(IHost host)
    {
        /// <summary>Configures the dependency resolver for the specified host and invokes a custom container factory action.</summary>
        /// <remarks>This method enables integration with the Microsoft dependency resolver and allows for further
        /// customization of the service provider through the specified action. The method returns the same host instance to
        /// support fluent configuration.</remarks>
        /// <param name="containerFactory">An action that receives the host's service provider for additional container configuration. Cannot be null.</param>
        /// <returns>The original host instance after applying the dependency resolver and container factory configuration.</returns>
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
        /// <summary>Configures Splat and ReactiveUI to use the Microsoft dependency injection system within the specified host application builder.</summary>
        /// <remarks>This method sets up Splat and ReactiveUI to use the Microsoft.Extensions.DependencyInjection
        /// service provider, ensuring that dependencies are resolved through the application's DI container. Call this
        /// method during application startup before building the host.</remarks>
        /// <returns>The same <see cref="IHostApplicationBuilder"/> instance, enabling method chaining.</returns>
        public IHostApplicationBuilder ConfigureSplatForMicrosoftDependencyResolver()
        {
            ArgumentNullException.ThrowIfNull(hostBuilder);

            hostBuilder.Services.UseMicrosoftDependencyResolver();
            var reactiveUiBuilder = AppLocator.CurrentMutable.CreateReactiveUIBuilder()
                .WithRegistration(r => r.InitializeSplat())
                .WithMaui();
            _ = reactiveUiBuilder.BuildApp();
            return hostBuilder;
        }
    }

    /// <summary>Provides extension members for this receiver.</summary>
    /// <param name="hostBuilder">The receiver instance.</param>
    extension(IHostBuilder hostBuilder)
    {
        /// <summary>Configures the specified host builder to use the Microsoft dependency resolver for Splat and initializes ReactiveUI integration with .NET MAUI.</summary>
        /// <remarks>This method registers the Microsoft dependency resolver for Splat and sets up ReactiveUI for
        /// use with .NET MAUI applications. Call this method during application startup to ensure that Splat and ReactiveUI
        /// are properly integrated with the dependency injection system.</remarks>
        /// <returns>The same <see cref="IHostBuilder"/> instance so that additional configuration calls can be chained.</returns>
        public IHostBuilder ConfigureSplatForMicrosoftDependencyResolver() =>
            hostBuilder.ConfigureServices((serviceCollection) =>
            {
                serviceCollection.UseMicrosoftDependencyResolver();
                var reactiveUiBuilder = AppLocator.CurrentMutable.CreateReactiveUIBuilder()
                    .WithRegistration(r => r.InitializeSplat())
                    .WithMaui();
                _ = reactiveUiBuilder.BuildApp();
            });
    }
}
