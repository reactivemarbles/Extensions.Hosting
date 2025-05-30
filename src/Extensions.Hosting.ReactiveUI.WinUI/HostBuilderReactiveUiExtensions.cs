// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Extensions.Hosting;
using ReactiveUI;
using Splat;
using Splat.Microsoft.Extensions.DependencyInjection;

namespace ReactiveMarbles.Extensions.Hosting.ReactiveUI;

/// <summary>
/// This contains the ReactiveUi extensions for Microsoft.Extensions.Hosting.
/// </summary>
public static class HostBuilderReactiveUiExtensions
{
    /// <summary>
    /// Configure a ReactiveUI application.
    /// </summary>
    /// <param name="hostBuilder">IHostBuilder.</param>
    /// <returns>I Host Builder.</returns>
    public static IHostBuilder ConfigureSplatForMicrosoftDependencyResolver(this IHostBuilder hostBuilder) =>
        hostBuilder.ConfigureServices((serviceCollection) =>
        {
            serviceCollection.UseMicrosoftDependencyResolver();
            var resolver = Locator.CurrentMutable;
            resolver.InitializeSplat();
            resolver.InitializeReactiveUI();
        });

    /// <summary>
    /// Maps the splat locator to the IServiceProvider.
    /// </summary>
    /// <param name="host">The host.</param>
    /// <param name="containerFactory">The IServiceProvider factory.</param>
    /// <returns>A Value.</returns>
    public static IHost? MapSplatLocator(this IHost host, Action<IServiceProvider?> containerFactory)
    {
        var c = host?.Services;
        c?.UseMicrosoftDependencyResolver();
        containerFactory?.Invoke(c);
        return host;
    }
}
