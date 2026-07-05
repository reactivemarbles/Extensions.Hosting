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

/// <summary>Provides extension methods for configuring ReactiveUI integration with Microsoft dependency injection in .NET host builders.</summary>
/// <remarks>These extensions enable seamless setup of ReactiveUI and Splat with Microsoft.Extensions.Hosting and
/// dependency injection. They are intended to be used during application startup to ensure that ReactiveUI services are
/// properly registered and available throughout the application's lifetime.</remarks>
public static class HostBuilderReactiveUiExtensions
{
    /// <summary>Provides extension members for this receiver.</summary>
    /// <param name="host">The receiver instance.</param>
    extension(IHost host)
    {
        /// <summary>Configures the dependency resolver for the specified host and invokes a custom container factory action.</summary>
        /// <remarks>This method enables integration with the Microsoft dependency resolver and allows for further
        /// customization of the service provider through the specified action. The method does not modify the host instance
        /// itself.</remarks>
        /// <param name="containerFactory">An action to perform additional configuration on the service provider. Can be null.</param>
        /// <returns>The original host instance. Returns null if <paramref name="host"/> is null.</returns>
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
        /// <summary>Configures Splat and ReactiveUI to use the Microsoft dependency injection system within the specified host builder.</summary>
        /// <remarks>This method sets up Splat and ReactiveUI to resolve dependencies using the
        /// Microsoft.Extensions.DependencyInjection container. It should be called during application startup before
        /// building the host. This configuration is required for proper integration of ReactiveUI and Splat services in
        /// applications using Microsoft dependency injection.</remarks>
        /// <returns>The same <see cref="IHostApplicationBuilder"/> instance, enabling method chaining.</returns>
        public IHostApplicationBuilder ConfigureSplatForMicrosoftDependencyResolver()
        {
            ArgumentNullException.ThrowIfNull(hostBuilder);

            hostBuilder.Services.UseMicrosoftDependencyResolver();
            var reactiveUiBuilder = AppLocator.CurrentMutable.CreateReactiveUIBuilder()
                .WithRegistration(r => r.InitializeSplat())
                .WithWinUI();
            _ = reactiveUiBuilder.BuildApp();
            return hostBuilder;
        }
    }

    /// <summary>Provides extension members for this receiver.</summary>
    /// <param name="hostBuilder">The receiver instance.</param>
    extension(IHostBuilder hostBuilder)
    {
        /// <summary>Configures the Splat and ReactiveUI dependency resolvers to use Microsoft.Extensions.DependencyInjection within the specified host builder.</summary>
        /// <remarks>Call this method before building the host to ensure that Splat and ReactiveUI services are
        /// registered with the Microsoft dependency injection container. This enables seamless integration of ReactiveUI
        /// and Splat services with the application's dependency injection system.</remarks>
        /// <returns>The same <see cref="IHostBuilder"/> instance, configured to use Microsoft.Extensions.DependencyInjection for
        /// Splat and ReactiveUI dependency resolution.</returns>
        public IHostBuilder ConfigureSplatForMicrosoftDependencyResolver() =>
            hostBuilder.ConfigureServices((serviceCollection) =>
            {
                serviceCollection.UseMicrosoftDependencyResolver();
                var reactiveUiBuilder = AppLocator.CurrentMutable.CreateReactiveUIBuilder()
                    .WithRegistration(r => r.InitializeSplat())
                    .WithWinUI();
                _ = reactiveUiBuilder.BuildApp();
            });
    }
}
