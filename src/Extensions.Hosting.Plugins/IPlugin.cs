// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;

namespace ReactiveMarbles.Extensions.Hosting.Plugins;

/// <summary>
/// Defines a contract for plug-ins that can configure services and settings for the application host during startup.
/// </summary>
/// <remarks>Implementations of this interface can be used to extend or modify the application's dependency
/// injection container and host configuration. Plug-ins should use the provided context and service collection to
/// register services or perform other setup tasks required for their functionality.</remarks>
public interface IPlugin
{
    /// <summary>
    /// Configures host-specific services and settings using the provided context and service collection.
    /// </summary>
    /// <remarks>This method is typically called during application startup to customize the host's dependency
    /// injection container and configuration. The implementation should not assume a specific type for
    /// hostBuilderContext unless documented by the caller.</remarks>
    /// <param name="hostBuilderContext">An object containing contextual information about the host configuration. The expected type and contents depend
    /// on the hosting environment.</param>
    /// <param name="serviceCollection">The service collection to which services should be added. Used to register application and infrastructure
    /// services for dependency injection.</param>
    void ConfigureHost(object hostBuilderContext, IServiceCollection serviceCollection);
}
