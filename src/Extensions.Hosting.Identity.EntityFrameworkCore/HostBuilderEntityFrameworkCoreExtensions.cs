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
/// Provides extension methods for configuring Entity Framework Core with SQL Server and web host services on ASP.NET
/// Core host builders and service collections.
/// </summary>
/// <remarks>These extensions simplify the setup of Entity Framework Core and related identity services in ASP.NET
/// Core applications, as well as the configuration of web host services for integration and testing scenarios. Methods
/// in this class are intended to be used during application startup to register required services and configure the
/// host environment.</remarks>
public static class HostBuilderEntityFrameworkCoreExtensions
{
    /// <summary>
    /// Configures Entity Framework Core with SQL Server and ASP.NET Core Identity using the specified context, user,
    /// and role types, and registers the required services in the dependency injection container.
    /// </summary>
    /// <remarks>This method sets up Entity Framework Core to use SQL Server as the database provider and
    /// configures ASP.NET Core Identity with the specified user and role types. It is typically called during
    /// application startup to register data access and identity services. The method also adds the Entity Framework
    /// stores for Identity, enabling user and role management backed by the specified DbContext.</remarks>
    /// <typeparam name="TContext">The type of the Entity Framework Core DbContext to use for data access.</typeparam>
    /// <typeparam name="TUser">The type representing application users for ASP.NET Core Identity. Must be a reference type.</typeparam>
    /// <typeparam name="TRole">The type representing application roles for ASP.NET Core Identity. Must be a reference type.</typeparam>
    /// <param name="services">The service collection to which the Entity Framework Core and Identity services will be added. Cannot be null.</param>
    /// <param name="context">The web host builder context containing configuration and environment information. Cannot be null.</param>
    /// <param name="connectionStringName">The name of the connection string in the application's configuration to use for the SQL Server database. Cannot
    /// be null or whitespace.</param>
    /// <param name="serviceLifetime">The lifetime with which to register the DbContext service. Defaults to ServiceLifetime.Scoped.</param>
    /// <returns>The same IServiceCollection instance so that additional calls can be chained.</returns>
    /// <exception cref="ArgumentException">Thrown if connectionStringName is null or consists only of white-space characters.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the specified connection string is not found in the configuration.</exception>
    public static IServiceCollection UseEntityFrameworkCoreSqlServer<TContext, TUser, TRole>(this IServiceCollection services, WebHostBuilderContext context, string connectionStringName, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
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
                options => options.UseSqlServer(conString ?? throw new InvalidOperationException($"Connection string '{connectionStringName}' not found.")),
                serviceLifetime)
            .AddDefaultIdentity<TUser>()
            .AddRoles<TRole>()
            .AddEntityFrameworkStores<TContext>();
        return services;
    }

    /// <summary>
    /// Configures Entity Framework Core to use SQL Server with the specified DbContext and identity user types, and
    /// registers the necessary services for identity and data access.
    /// </summary>
    /// <remarks>This method adds the DbContext, ASP.NET Core Identity, and Entity Framework stores to the
    /// service collection, enabling authentication and data access using SQL Server. The connection string must be
    /// defined in the application's configuration under the provided name.</remarks>
    /// <typeparam name="TContext">The type of the DbContext to register for use with SQL Server.</typeparam>
    /// <typeparam name="TUser">The type of the user entity to use with ASP.NET Core Identity.</typeparam>
    /// <param name="services">The service collection to which the Entity Framework Core and identity services will be added. Cannot be null.</param>
    /// <param name="context">The web host builder context containing configuration information. Cannot be null.</param>
    /// <param name="connectionStringName">The name of the connection string in the configuration to use for the SQL Server database. Cannot be null or
    /// whitespace.</param>
    /// <param name="serviceLifetime">The lifetime with which to register the DbContext service. The default is Scoped.</param>
    /// <returns>The IServiceCollection instance with Entity Framework Core and identity services configured for SQL Server.</returns>
    /// <exception cref="ArgumentException">Thrown if connectionStringName is null or consists only of white-space characters.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the specified connection string is not found in the configuration.</exception>
    public static IServiceCollection UseEntityFrameworkCoreSqlServer<TContext, TUser>(this IServiceCollection services, WebHostBuilderContext context, string connectionStringName, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
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
                options => options.UseSqlServer(conString ?? throw new InvalidOperationException($"Connection string '{connectionStringName}' not found.")),
                serviceLifetime)
            .AddDefaultIdentity<TUser>()
            .AddEntityFrameworkStores<TContext>();
        return services;
    }

    /// <summary>
    /// Configures Entity Framework Core with SQL Server for the specified DbContext without ASP.NET Core Identity.
    /// </summary>
    /// <remarks>Use this method when you need Entity Framework Core with SQL Server but do not require
    /// ASP.NET Core Identity services. This is useful for applications that handle authentication externally or
    /// do not need user management.</remarks>
    /// <typeparam name="TContext">The type of the Entity Framework Core DbContext to use for data access.</typeparam>
    /// <param name="services">The service collection to which the Entity Framework Core services will be added. Cannot be null.</param>
    /// <param name="configuration">The configuration instance containing the connection string. Cannot be null.</param>
    /// <param name="connectionStringName">The name of the connection string in the configuration. Cannot be null or whitespace.</param>
    /// <param name="serviceLifetime">The lifetime with which to register the DbContext service. Defaults to ServiceLifetime.Scoped.</param>
    /// <returns>The same IServiceCollection instance so that additional calls can be chained.</returns>
    /// <exception cref="ArgumentException">Thrown if connectionStringName is null or consists only of white-space characters.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the specified connection string is not found in the configuration.</exception>
    public static IServiceCollection AddSqlServerDbContext<TContext>(this IServiceCollection services, IConfiguration configuration, string connectionStringName, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        where TContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        if (string.IsNullOrWhiteSpace(connectionStringName))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(connectionStringName));
        }

        var conString = configuration.GetConnectionString(connectionStringName);
        services.AddDbContext<TContext>(
            options => options.UseSqlServer(conString ?? throw new InvalidOperationException($"Connection string '{connectionStringName}' not found.")),
            serviceLifetime);
        return services;
    }

    /// <summary>
    /// Configures Entity Framework Core with SQL Server for the specified DbContext using a direct connection string.
    /// </summary>
    /// <remarks>Use this overload when you have a connection string available directly rather than from
    /// configuration. This is useful for testing scenarios or when connection strings are obtained from
    /// other sources such as environment variables or secret managers.</remarks>
    /// <typeparam name="TContext">The type of the Entity Framework Core DbContext to use for data access.</typeparam>
    /// <param name="services">The service collection to which the Entity Framework Core services will be added. Cannot be null.</param>
    /// <param name="connectionString">The SQL Server connection string. Cannot be null or whitespace.</param>
    /// <param name="serviceLifetime">The lifetime with which to register the DbContext service. Defaults to ServiceLifetime.Scoped.</param>
    /// <returns>The same IServiceCollection instance so that additional calls can be chained.</returns>
    /// <exception cref="ArgumentException">Thrown if connectionString is null or consists only of white-space characters.</exception>
    public static IServiceCollection AddSqlServerDbContextWithConnectionString<TContext>(this IServiceCollection services, string connectionString, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        where TContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(services);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(connectionString));
        }

        services.AddDbContext<TContext>(
            options => options.UseSqlServer(connectionString),
            serviceLifetime);
        return services;
    }

    /// <summary>
    /// Configures Entity Framework Core with SQL Server using the IHostApplicationBuilder pattern.
    /// </summary>
    /// <remarks>This method provides integration with the modern IHostApplicationBuilder pattern
    /// introduced in .NET 7+. It registers the DbContext with SQL Server using the connection string
    /// from configuration.</remarks>
    /// <typeparam name="TContext">The type of the Entity Framework Core DbContext to use for data access.</typeparam>
    /// <param name="builder">The host application builder. Cannot be null.</param>
    /// <param name="connectionStringName">The name of the connection string in the configuration. Cannot be null or whitespace.</param>
    /// <param name="serviceLifetime">The lifetime with which to register the DbContext service. Defaults to ServiceLifetime.Scoped.</param>
    /// <returns>The same IHostApplicationBuilder instance so that additional calls can be chained.</returns>
    /// <exception cref="ArgumentException">Thrown if connectionStringName is null or consists only of white-space characters.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the specified connection string is not found in the configuration.</exception>
    public static IHostApplicationBuilder AddSqlServerDbContext<TContext>(this IHostApplicationBuilder builder, string connectionStringName, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        where TContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (string.IsNullOrWhiteSpace(connectionStringName))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(connectionStringName));
        }

        var conString = builder.Configuration.GetConnectionString(connectionStringName);
        builder.Services.AddDbContext<TContext>(
            options => options.UseSqlServer(conString ?? throw new InvalidOperationException($"Connection string '{connectionStringName}' not found.")),
            serviceLifetime);
        return builder;
    }

    /// <summary>
    /// Configures Entity Framework Core with SQL Server and ASP.NET Core Identity using IHostApplicationBuilder.
    /// </summary>
    /// <remarks>This method provides integration with the modern IHostApplicationBuilder pattern and
    /// sets up both Entity Framework Core and ASP.NET Core Identity with the specified user and role types.</remarks>
    /// <typeparam name="TContext">The type of the Entity Framework Core DbContext to use for data access.</typeparam>
    /// <typeparam name="TUser">The type representing application users for ASP.NET Core Identity.</typeparam>
    /// <typeparam name="TRole">The type representing application roles for ASP.NET Core Identity.</typeparam>
    /// <param name="builder">The host application builder. Cannot be null.</param>
    /// <param name="connectionStringName">The name of the connection string in the configuration. Cannot be null or whitespace.</param>
    /// <param name="serviceLifetime">The lifetime with which to register the DbContext service. Defaults to ServiceLifetime.Scoped.</param>
    /// <returns>The same IHostApplicationBuilder instance so that additional calls can be chained.</returns>
    /// <exception cref="ArgumentException">Thrown if connectionStringName is null or consists only of white-space characters.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the specified connection string is not found in the configuration.</exception>
    public static IHostApplicationBuilder AddSqlServerWithIdentity<TContext, TUser, TRole>(this IHostApplicationBuilder builder, string connectionStringName, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        where TContext : DbContext
        where TUser : class
        where TRole : class
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (string.IsNullOrWhiteSpace(connectionStringName))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(connectionStringName));
        }

        var conString = builder.Configuration.GetConnectionString(connectionStringName);
        builder.Services
            .AddDbContext<TContext>(
                options => options.UseSqlServer(conString ?? throw new InvalidOperationException($"Connection string '{connectionStringName}' not found.")),
                serviceLifetime)
            .AddDefaultIdentity<TUser>()
            .AddRoles<TRole>()
            .AddEntityFrameworkStores<TContext>();
        return builder;
    }

    /// <summary>
    /// Configures Entity Framework Core with SQL Server and ASP.NET Core Identity (user only) using IHostApplicationBuilder.
    /// </summary>
    /// <remarks>This method provides integration with the modern IHostApplicationBuilder pattern and
    /// sets up both Entity Framework Core and ASP.NET Core Identity with the specified user type only.</remarks>
    /// <typeparam name="TContext">The type of the Entity Framework Core DbContext to use for data access.</typeparam>
    /// <typeparam name="TUser">The type representing application users for ASP.NET Core Identity.</typeparam>
    /// <param name="builder">The host application builder. Cannot be null.</param>
    /// <param name="connectionStringName">The name of the connection string in the configuration. Cannot be null or whitespace.</param>
    /// <param name="serviceLifetime">The lifetime with which to register the DbContext service. Defaults to ServiceLifetime.Scoped.</param>
    /// <returns>The same IHostApplicationBuilder instance so that additional calls can be chained.</returns>
    /// <exception cref="ArgumentException">Thrown if connectionStringName is null or consists only of white-space characters.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the specified connection string is not found in the configuration.</exception>
    public static IHostApplicationBuilder AddSqlServerWithIdentity<TContext, TUser>(this IHostApplicationBuilder builder, string connectionStringName, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        where TContext : DbContext
        where TUser : class
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (string.IsNullOrWhiteSpace(connectionStringName))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(connectionStringName));
        }

        var conString = builder.Configuration.GetConnectionString(connectionStringName);
        builder.Services
            .AddDbContext<TContext>(
                options => options.UseSqlServer(conString ?? throw new InvalidOperationException($"Connection string '{connectionStringName}' not found.")),
                serviceLifetime)
            .AddDefaultIdentity<TUser>()
            .AddEntityFrameworkStores<TContext>();
        return builder;
    }

    /// <summary>
    /// Validates that a connection string exists in the configuration.
    /// </summary>
    /// <param name="configuration">The configuration instance to check.</param>
    /// <param name="connectionStringName">The name of the connection string to validate.</param>
    /// <returns>True if the connection string exists and is not empty; otherwise, false.</returns>
    public static bool HasConnectionString(this IConfiguration configuration, string connectionStringName)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        if (string.IsNullOrWhiteSpace(connectionStringName))
        {
            return false;
        }

        var connectionString = configuration.GetConnectionString(connectionStringName);
        return !string.IsNullOrWhiteSpace(connectionString);
    }

    /// <summary>
    /// Gets a connection string from configuration, throwing a descriptive exception if not found.
    /// </summary>
    /// <param name="configuration">The configuration instance.</param>
    /// <param name="connectionStringName">The name of the connection string to retrieve.</param>
    /// <returns>The connection string value.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the connection string is not found or is empty.</exception>
    public static string GetRequiredConnectionString(this IConfiguration configuration, string connectionStringName)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        if (string.IsNullOrWhiteSpace(connectionStringName))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(connectionStringName));
        }

        var connectionString = configuration.GetConnectionString(connectionStringName);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException($"Connection string '{connectionStringName}' not found in configuration. Ensure it is defined in appsettings.json or environment variables under 'ConnectionStrings:{connectionStringName}'.");
        }

        return connectionString;
    }

    /// <summary>
    /// Configures the host to use web host services and allows additional service configuration for the web host
    /// builder.
    /// </summary>
    /// <remarks>This method sets up the web host with default configurations and applies the specified
    /// service configuration. It is useful for scenarios where you want to customize the web host's service
    /// registrations within a generic host setup.</remarks>
    /// <param name="hostBuilder">The host builder to configure. Cannot be null.</param>
    /// <param name="configureServices">A delegate that configures services for the web host. Receives the web host builder context and the service
    /// collection to configure.</param>
    /// <param name="validateScopes">true to validate service scopes when building the service provider; otherwise, false. The default is false.</param>
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
    /// Configures the specified host builder to use ASP.NET Core web host services with custom service and web host
    /// configuration.
    /// </summary>
    /// <remarks>This method is intended for advanced scenarios where custom service registration and web host
    /// configuration are required. It provides a way to integrate ASP.NET Core web hosting features into a generic host
    /// builder pipeline.</remarks>
    /// <param name="hostBuilder">The host builder to configure. Cannot be null.</param>
    /// <param name="configureServices">A delegate that configures services for the web host. Receives the web host builder context and the service
    /// collection.</param>
    /// <param name="configureWebHost">A delegate that configures the web host builder. Receives the current web host builder and returns the
    /// configured instance.</param>
    /// <param name="validateScopes">true to enable scope validation for the default service provider; otherwise, false. The default is false.</param>
    /// <returns>The same IHostBuilder instance for chaining further configuration.</returns>
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
    /// Configures the host builder to use web host services with custom service, web host, and application
    /// configurations.
    /// </summary>
    /// <remarks>This method allows advanced customization of the web host's service collection, web host
    /// builder, and application pipeline during host configuration. It is intended for scenarios where the default web
    /// host setup needs to be extended or replaced with custom logic.</remarks>
    /// <param name="hostBuilder">The host builder to configure. Cannot be null.</param>
    /// <param name="configureServices">A delegate that configures services for the web host. Receives the web host builder context and the service
    /// collection to configure.</param>
    /// <param name="configureWebHost">A delegate that configures the web host builder. Receives the current web host builder and returns the
    /// configured builder.</param>
    /// <param name="configureApp">A delegate that configures the application builder. Receives the application builder and returns the configured
    /// builder.</param>
    /// <param name="validateScopes">true to validate service scopes when building the service provider; otherwise, false. The default is false.</param>
    /// <returns>The same IHostBuilder instance for chaining further configuration.</returns>
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
