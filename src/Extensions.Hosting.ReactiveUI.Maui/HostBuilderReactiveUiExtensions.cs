// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Extensions.Hosting;
using ReactiveUI.Builder;
using Splat;
using Splat.Microsoft.Extensions.DependencyInjection;

namespace ReactiveMarbles.Extensions.Hosting.ReactiveUI;

/// <summary>
/// Provides extension methods for configuring ReactiveUI integration with Microsoft dependency injection in .NET host
/// builders.
/// </summary>
/// <remarks>These extensions enable seamless setup of ReactiveUI and Splat with Microsoft.Extensions.Hosting and
/// dependency injection. Use these methods to configure ReactiveUI services and ensure that Splat uses the Microsoft
/// dependency resolver within your application's host configuration pipeline.</remarks>
public static class HostBuilderReactiveUiExtensions
{
    /// <summary>
    /// Configures the specified host builder to use the Microsoft dependency resolver for Splat and initializes
    /// ReactiveUI integration with .NET MAUI.
    /// </summary>
    /// <remarks>This method registers the Microsoft dependency resolver for Splat and sets up ReactiveUI for
    /// use with .NET MAUI applications. Call this method during application startup to ensure that Splat and ReactiveUI
    /// are properly integrated with the dependency injection system.</remarks>
    /// <param name="hostBuilder">The host builder to configure with Splat and ReactiveUI services. Cannot be null.</param>
    /// <returns>The same <see cref="IHostBuilder"/> instance so that additional configuration calls can be chained.</returns>
    public static IHostBuilder ConfigureSplatForMicrosoftDependencyResolver(this IHostBuilder hostBuilder) =>
        hostBuilder.ConfigureServices((serviceCollection) =>
        {
            serviceCollection.UseMicrosoftDependencyResolver();
            AppLocator.CurrentMutable.CreateReactiveUIBuilder()
                .WithRegistration(r => r.InitializeSplat())
                .WithMaui()
                .BuildApp();
        });

    /// <summary>
    /// Configures Splat and ReactiveUI to use the Microsoft dependency injection system within the specified host
    /// application builder.
    /// </summary>
    /// <remarks>This method sets up Splat and ReactiveUI to use the Microsoft.Extensions.DependencyInjection
    /// service provider, ensuring that dependencies are resolved through the application's DI container. Call this
    /// method during application startup before building the host.</remarks>
    /// <param name="hostBuilder">The host application builder to configure for Splat and ReactiveUI integration. Cannot be null.</param>
    /// <returns>The same <see cref="IHostApplicationBuilder"/> instance, enabling method chaining.</returns>
    public static IHostApplicationBuilder ConfigureSplatForMicrosoftDependencyResolver(this IHostApplicationBuilder hostBuilder)
    {
        ArgumentNullException.ThrowIfNull(hostBuilder);

        hostBuilder.Services.UseMicrosoftDependencyResolver();
        AppLocator.CurrentMutable.CreateReactiveUIBuilder()
            .WithRegistration(r => r.InitializeSplat())
            .WithMaui()
            .BuildApp();
        return hostBuilder;
    }

    /// <summary>
    /// Configures the dependency resolver for the specified host and invokes a custom container factory action.
    /// </summary>
    /// <remarks>This method enables integration with the Microsoft dependency resolver and allows for further
    /// customization of the service provider through the specified action. The method returns the same host instance to
    /// support fluent configuration.</remarks>
    /// <param name="host">The host whose service provider will be used to configure the dependency resolver. Cannot be null.</param>
    /// <param name="containerFactory">An action that receives the host's service provider for additional container configuration. Cannot be null.</param>
    /// <returns>The original host instance after applying the dependency resolver and container factory configuration.</returns>
    public static IHost? MapSplatLocator(this IHost host, Action<IServiceProvider?> containerFactory)
    {
        var c = host?.Services;
        c?.UseMicrosoftDependencyResolver();
        containerFactory?.Invoke(c);
        return host;
    }
}
