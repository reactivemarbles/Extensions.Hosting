// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ReactiveMarbles.Extensions.Hosting.Identity.EntityFrameworkCore;

/// <summary>
/// Provides extension methods for configuring Entity Framework Core with SQLite and web host services on host and
/// service builders.
/// </summary>
/// <remarks>These extensions simplify the setup of Entity Framework Core with SQLite and ASP.NET Core Identity in
/// .NET host-based applications. They also provide utilities for configuring web host services and customizing the web
/// host and application pipeline during host building. All methods are intended to be used during application startup
/// configuration.</remarks>
public static class HostBuilderEntityFrameworkCoreExtensions
{
    /// <summary>
    /// Configures Entity Framework Core with a SQLite provider and sets up ASP.NET Core Identity using the specified
    /// context, user, and role types.
    /// </summary>
    /// <remarks>This method registers the DbContext with the SQLite provider and configures ASP.NET Core
    /// Identity to use the specified user and role types with Entity Framework Core stores. It is typically called
    /// during application startup to enable authentication and authorization using SQLite as the backing
    /// store.</remarks>
    /// <typeparam name="TContext">The type of the Entity Framework Core DbContext to use for data access.</typeparam>
    /// <typeparam name="TUser">The type representing application users for ASP.NET Core Identity.</typeparam>
    /// <typeparam name="TRole">The type representing application roles for ASP.NET Core Identity.</typeparam>
    /// <param name="services">The service collection to which the Entity Framework Core and Identity services will be added. Cannot be null.</param>
    /// <param name="context">The web host builder context containing configuration and environment information. Cannot be null.</param>
    /// <param name="connectionStringName">The name of the connection string in the configuration to use for the SQLite database. Cannot be null or
    /// whitespace.</param>
    /// <param name="serviceLifetime">The lifetime with which to register the DbContext service. The default is Scoped.</param>
    /// <returns>The same IServiceCollection instance so that additional calls can be chained.</returns>
    /// <exception cref="ArgumentException">Thrown if connectionStringName is null or consists only of white-space characters.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the specified connection string is not found in the configuration.</exception>
    public static IServiceCollection UseEntityFrameworkCoreSqlite<TContext, TUser, TRole>(this IServiceCollection services, WebHostBuilderContext context, string connectionStringName, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        where TContext : DbContext
        where TUser : class
        where TRole : class
    {
        ArgumentNullException.ThrowIfNull(services);

        ArgumentNullException.ThrowIfNull(context);

        if (string.IsNullOrWhiteSpace(connectionStringName))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(connectionStringName));
        }

        var conString = context.Configuration.GetConnectionString(connectionStringName);
        services
            .AddDbContext<TContext>(
                options => options.UseSqlite(conString ?? throw new InvalidOperationException($"Connection string '{connectionStringName}' not found.")),
                serviceLifetime)
            .AddDefaultIdentity<TUser>()
            .AddRoles<TRole>()
            .AddEntityFrameworkStores<TContext>();
        return services;
    }

    /// <summary>
    /// Configures Entity Framework Core with a SQLite provider and ASP.NET Core Identity using the specified DbContext
    /// and user type.
    /// </summary>
    /// <remarks>This method registers the specified DbContext with a SQLite provider and configures ASP.NET
    /// Core Identity to use Entity Framework Core stores. It is typically called during application startup to enable
    /// authentication and data access using SQLite.</remarks>
    /// <typeparam name="TContext">The type of the DbContext to use for Entity Framework Core operations.</typeparam>
    /// <typeparam name="TUser">The type representing the user entity for ASP.NET Core Identity.</typeparam>
    /// <param name="services">The IServiceCollection to which the Entity Framework Core and Identity services will be added.</param>
    /// <param name="context">The WebHostBuilderContext containing application configuration and environment information.</param>
    /// <param name="connectionStringName">The name of the connection string in the configuration to use for the SQLite database. Cannot be null or
    /// whitespace.</param>
    /// <param name="serviceLifetime">The lifetime with which to register the DbContext service. The default is Scoped.</param>
    /// <returns>The IServiceCollection instance with Entity Framework Core and Identity services configured for SQLite.</returns>
    /// <exception cref="ArgumentException">Thrown if connectionStringName is null or consists only of white-space characters.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the specified connection string is not found in the configuration.</exception>
    public static IServiceCollection UseEntityFrameworkCoreSqlite<TContext, TUser>(this IServiceCollection services, WebHostBuilderContext context, string connectionStringName, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        where TContext : DbContext
        where TUser : class
    {
        ArgumentNullException.ThrowIfNull(services);

        ArgumentNullException.ThrowIfNull(context);

        if (string.IsNullOrWhiteSpace(connectionStringName))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(connectionStringName));
        }

        var conString = context.Configuration.GetConnectionString(connectionStringName);
        services
            .AddDbContext<TContext>(
                options => options.UseSqlite(conString ?? throw new InvalidOperationException($"Connection string '{connectionStringName}' not found.")),
                serviceLifetime)
            .AddDefaultIdentity<TUser>()
            .AddEntityFrameworkStores<TContext>();
        return services;
    }

    /// <summary>
    /// Configures the host builder to use web host services with custom service configuration and optional scope
    /// validation.
    /// </summary>
    /// <remarks>This method sets up minimal web host defaults and allows custom service registration for
    /// scenarios where a full web application pipeline is not required. It is useful for integrating web host services
    /// into generic host scenarios or for testing purposes.</remarks>
    /// <param name="hostBuilder">The host builder to configure. Cannot be null.</param>
    /// <param name="configureServices">A delegate that configures the application's service collection. Invoked with the web host builder context and
    /// the service collection.</param>
    /// <param name="validateScopes">true to enable scope validation for the service provider; otherwise, false. The default is false.</param>
    /// <returns>The same instance of the host builder for chaining further configuration.</returns>
    public static IHostBuilder UseWebHostServices(this IHostBuilder hostBuilder, Action<WebHostBuilderContext, IServiceCollection> configureServices, bool validateScopes = false)
    {
        ArgumentNullException.ThrowIfNull(hostBuilder);

        return hostBuilder.ConfigureWebHostDefaults(webBuilder =>
                      webBuilder.UseDefaultServiceProvider(options => options.ValidateScopes = validateScopes)
                         .Configure(app => app.Run(async (_) => await Task.CompletedTask)) // Dummy app.Run to prevent 'No application service provider was found' error.
                         .ConfigureServices((context, services) => configureServices(context, services)));
    }

    /// <summary>
    /// Configures the host builder to use ASP.NET Core web host services with custom service and web host
    /// configuration.
    /// </summary>
    /// <remarks>This method is intended for advanced scenarios where you need to customize both the web host
    /// and its services during host building. It sets up a minimal web application to ensure the web host services are
    /// available, even if no application logic is provided.</remarks>
    /// <param name="hostBuilder">The host builder to configure. Cannot be null.</param>
    /// <param name="configureServices">A delegate that configures services for the web host. Receives the web host builder context and the service
    /// collection.</param>
    /// <param name="configureWebHost">A delegate that further configures the web host builder. Receives and returns an instance of <see
    /// cref="IWebHostBuilder"/>.</param>
    /// <param name="validateScopes">A value indicating whether to validate service scopes when building the service provider. Set to <see
    /// langword="true"/> to enable scope validation; otherwise, <see langword="false"/>. The default is <see
    /// langword="false"/>.</param>
    /// <returns>The same <see cref="IHostBuilder"/> instance for chaining further configuration.</returns>
    public static IHostBuilder UseWebHostServices(this IHostBuilder hostBuilder, Action<WebHostBuilderContext, IServiceCollection> configureServices, Func<IWebHostBuilder, IWebHostBuilder> configureWebHost, bool validateScopes = false)
    {
        ArgumentNullException.ThrowIfNull(hostBuilder);

        return hostBuilder.ConfigureWebHostDefaults(webBuilder =>
            configureWebHost(webBuilder)
                .UseDefaultServiceProvider(options => options.ValidateScopes = validateScopes)
                .Configure(app => app.Run(async (_) => await Task.CompletedTask)) // Dummy app.Run to prevent 'No application service provider was found' error.
                .ConfigureServices((context, services) => configureServices(context, services)));
    }

    /// <summary>
    /// Configures the specified host builder to use web host services with custom service, web host, and application
    /// configuration.
    /// </summary>
    /// <remarks>This method enables advanced scenarios where you need to customize the web host's services,
    /// configuration, and application pipeline within a generic host. It is intended for use when integrating ASP.NET
    /// Core web hosting into a generic host setup. The method applies the provided delegates in the order: web host
    /// configuration, service configuration, and application pipeline configuration.</remarks>
    /// <param name="hostBuilder">The host builder to configure. Cannot be null.</param>
    /// <param name="configureServices">A delegate that configures services for the web host. Receives the web host builder context and the service
    /// collection to configure.</param>
    /// <param name="configureWebHost">A delegate that configures the web host builder. Receives the current web host builder and returns the
    /// configured builder.</param>
    /// <param name="configureApp">A delegate that configures the application's request pipeline. Receives the application builder and returns the
    /// configured builder.</param>
    /// <param name="validateScopes">true to validate service scopes when building the service provider; otherwise, false. The default is false.</param>
    /// <returns>The same instance of the host builder for chaining further configuration.</returns>
    public static IHostBuilder UseWebHostServices(this IHostBuilder hostBuilder, Action<WebHostBuilderContext, IServiceCollection> configureServices, Func<IWebHostBuilder, IWebHostBuilder> configureWebHost, Func<IApplicationBuilder, IApplicationBuilder> configureApp, bool validateScopes = false)
    {
        ArgumentNullException.ThrowIfNull(hostBuilder);

        return hostBuilder.ConfigureWebHostDefaults(webBuilder =>
            configureWebHost(webBuilder)
                .UseDefaultServiceProvider(options => options.ValidateScopes = validateScopes)
                .Configure(app => configureApp(app).Run(async (_) => await Task.CompletedTask)) // Dummy app.Run to prevent 'No application service provider was found' error.
                .ConfigureServices((context, services) => configureServices(context, services)));
    }
}
