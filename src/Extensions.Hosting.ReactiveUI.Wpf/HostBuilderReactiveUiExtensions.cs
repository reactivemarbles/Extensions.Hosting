// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Extensions.Hosting;
using ReactiveUI.Builder;
using Splat;
using Splat.Microsoft.Extensions.DependencyInjection;

namespace ReactiveMarbles.Extensions.Hosting.ReactiveUI;

/// <summary>
/// Provides extension methods for configuring ReactiveUI with Microsoft dependency injection in .NET host builder
/// scenarios.
/// </summary>
/// <remarks>These extension methods enable seamless integration of ReactiveUI and Splat with the Microsoft
/// dependency injection system, supporting both generic host and application host builder patterns. Use these methods
/// to set up ReactiveUI infrastructure when building WPF or other .NET applications that rely on dependency
/// injection.</remarks>
public static class HostBuilderReactiveUiExtensions
{
    /// <summary>
    /// Configures the specified host builder to use Splat with the Microsoft dependency resolver for dependency
    /// injection in a WPF application.
    /// </summary>
    /// <remarks>This method sets up Splat and ReactiveUI to use the Microsoft dependency resolver, enabling
    /// seamless integration with the application's dependency injection system. It is intended for use in WPF
    /// applications that utilize ReactiveUI and Splat for service location.</remarks>
    /// <param name="hostBuilder">The host builder to configure with Splat and the Microsoft dependency resolver. Cannot be null.</param>
    /// <returns>The same <see cref="IHostBuilder"/> instance so that additional configuration calls can be chained.</returns>
    public static IHostBuilder ConfigureSplatForMicrosoftDependencyResolver(this IHostBuilder hostBuilder) =>
        hostBuilder.ConfigureServices((serviceCollection) =>
        {
            serviceCollection.UseMicrosoftDependencyResolver();
            AppLocator.CurrentMutable.CreateReactiveUIBuilder()
                .WithRegistration(r => r.InitializeSplat())
                .WithWpf()
                .BuildApp();
        });

    /// <summary>
    /// Configures Splat to use the Microsoft dependency resolver within the specified host application builder.
    /// </summary>
    /// <remarks>This method enables Splat and ReactiveUI to resolve dependencies using the
    /// Microsoft.Extensions.DependencyInjection container. It should be called during application startup before
    /// building the host.</remarks>
    /// <param name="hostBuilder">The host application builder to configure for Splat and Microsoft dependency injection integration. Cannot be
    /// null.</param>
    /// <returns>The same instance of <see cref="IHostApplicationBuilder"/> with Splat configured to use the Microsoft dependency
    /// resolver.</returns>
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
            .WithWpf()
            .BuildApp();
        return hostBuilder;
    }

    /// <summary>
    /// Configures the Microsoft dependency resolver and invokes a custom container factory using the host's service
    /// provider.
    /// </summary>
    /// <remarks>This method enables integration with the Microsoft dependency injection system and allows
    /// further customization of the service container. The method returns the same host instance to support fluent
    /// configuration.</remarks>
    /// <param name="host">The host whose service provider will be used to configure the dependency resolver. Cannot be null.</param>
    /// <param name="containerFactory">An action that receives the host's service provider for additional container configuration. Cannot be null.</param>
    /// <returns>The original host instance after configuration, or null if the input host is null.</returns>
    public static IHost? MapSplatLocator(this IHost host, Action<IServiceProvider?> containerFactory)
    {
        var c = host?.Services;
        c?.UseMicrosoftDependencyResolver();
        containerFactory?.Invoke(c);
        return host;
    }
}
