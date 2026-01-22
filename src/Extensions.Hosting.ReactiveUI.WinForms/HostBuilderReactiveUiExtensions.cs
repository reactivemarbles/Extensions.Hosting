// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Extensions.Hosting;
using ReactiveUI.Builder;
using Splat;
using Splat.Microsoft.Extensions.DependencyInjection;

namespace ReactiveMarbles.Extensions.Hosting.ReactiveUI;

/// <summary>
/// Provides extension methods for configuring ReactiveUI with Microsoft dependency injection in .NET host builders.
/// </summary>
/// <remarks>These extensions enable integration of ReactiveUI's dependency resolution with the
/// Microsoft.Extensions.Hosting infrastructure, allowing applications to use ReactiveUI with standard .NET dependency
/// injection. Methods in this class support both IHostBuilder and IHostApplicationBuilder, as well as mapping the Splat
/// locator to the application's IServiceProvider.</remarks>
public static class HostBuilderReactiveUiExtensions
{
    /// <summary>
    /// Configures the specified host builder to use the Microsoft dependency resolver with Splat and initializes
    /// ReactiveUI integration for WinForms applications.
    /// </summary>
    /// <remarks>This method enables Splat to use the Microsoft dependency injection container and sets up
    /// ReactiveUI for WinForms. Call this method before building the host to ensure that Splat and ReactiveUI services
    /// are available throughout the application.</remarks>
    /// <param name="hostBuilder">The host builder to configure with Splat and ReactiveUI services. Cannot be null.</param>
    /// <returns>The same <see cref="IHostBuilder"/> instance so that additional configuration calls can be chained.</returns>
    public static IHostBuilder ConfigureSplatForMicrosoftDependencyResolver(this IHostBuilder hostBuilder) =>
        hostBuilder.ConfigureServices((serviceCollection) =>
        {
            serviceCollection.UseMicrosoftDependencyResolver();
            AppLocator.CurrentMutable.CreateReactiveUIBuilder()
                .WithRegistration(r => r.InitializeSplat())
                .WithWinForms()
                .Build();
        });

    /// <summary>
    /// Configures Splat to use the Microsoft dependency resolver within the specified host application builder.
    /// </summary>
    /// <remarks>This method enables Splat and ReactiveUI to resolve dependencies using the
    /// Microsoft.Extensions.DependencyInjection container. Call this method during application startup to ensure that
    /// Splat services are registered with the application's dependency injection system.</remarks>
    /// <param name="hostBuilder">The host application builder to configure for Splat integration. Cannot be null.</param>
    /// <returns>The same <see cref="IHostApplicationBuilder"/> instance, configured to use the Microsoft dependency resolver for
    /// Splat.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="hostBuilder"/> is null.</exception>
    public static IHostApplicationBuilder ConfigureSplatForMicrosoftDependencyResolver(this IHostApplicationBuilder hostBuilder)
    {
        if (hostBuilder == null)
        {
            throw new ArgumentNullException(nameof(hostBuilder));
        }

        hostBuilder.Services.UseMicrosoftDependencyResolver();
        AppLocator.CurrentMutable.CreateReactiveUIBuilder()
            .WithRegistration(r => r.InitializeSplat())
            .WithWinForms()
            .Build();
        return hostBuilder;
    }

    /// <summary>
    /// Configures the dependency resolver for the specified host and invokes a custom container factory action.
    /// </summary>
    /// <remarks>This method enables integration with the Microsoft dependency resolver and allows for further
    /// customization of the service provider through the specified action. The method does not modify the host instance
    /// itself, but may affect global dependency resolution behavior.</remarks>
    /// <param name="host">The host whose service provider will be used to configure the dependency resolver. Cannot be null.</param>
    /// <param name="containerFactory">An action that receives the host's service provider for additional container configuration. Can be null if no
    /// additional configuration is required.</param>
    /// <returns>The original host instance after applying the dependency resolver and invoking the container factory action, or
    /// null if the input host is null.</returns>
    public static IHost? MapSplatLocator(this IHost host, Action<IServiceProvider?> containerFactory)
    {
        var c = host?.Services;
        c?.UseMicrosoftDependencyResolver();
        containerFactory?.Invoke(c);
        return host;
    }
}
