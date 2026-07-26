// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ReactiveMarbles.Extensions.Hosting.Identity.EntityFrameworkCore;

/// <summary>Provides extension methods for configuring Entity Framework Core with SQLite and web host services on host and service builders.</summary>
/// <remarks>These extensions simplify the setup of Entity Framework Core with SQLite and ASP.NET Core Identity in
/// .NET host-based applications. They also provide utilities for configuring web host services and customizing the web
/// host and application pipeline during host building. All methods are intended to be used during application startup
/// configuration.</remarks>
public static class HostBuilderEntityFrameworkCoreExtensions
{
    /// <summary>Provides extension members for this receiver.</summary>
    /// <param name="builder">The receiver instance.</param>
    extension(IHostApplicationBuilder builder)
    {
        /// <summary>Configures Entity Framework Core with SQLite using the IHostApplicationBuilder pattern.</summary>
        /// <remarks>This method provides integration with the modern IHostApplicationBuilder pattern
        /// introduced in .NET 7+. It registers the DbContext with SQLite using the connection string
        /// from configuration.</remarks>
        /// <typeparam name="TContext">The type of the Entity Framework Core DbContext to use for data access.</typeparam>
        /// <param name="connectionStringName">The name of the connection string in the configuration. Cannot be null or whitespace.</param>
        /// <returns>The same IHostApplicationBuilder instance so that additional calls can be chained.</returns>
        public IHostApplicationBuilder AddSqliteDbContext<TContext>(string connectionStringName)
            where TContext : DbContext =>
            builder.AddSqliteDbContext<TContext>(connectionStringName, ServiceLifetime.Scoped);

        /// <summary>Configures Entity Framework Core with SQLite and an explicit service lifetime.</summary>
        /// <typeparam name="TContext">The Entity Framework Core DbContext type.</typeparam>
        /// <param name="connectionStringName">The configured connection string name.</param>
        /// <param name="serviceLifetime">The lifetime with which to register the DbContext service.</param>
        /// <returns>The same IHostApplicationBuilder instance so that additional calls can be chained.</returns>
        /// <exception cref="ArgumentException">Thrown if connectionStringName is null or consists only of white-space characters.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the specified connection string is not found in the configuration.</exception>
        public IHostApplicationBuilder AddSqliteDbContext<TContext>(
            string connectionStringName,
            ServiceLifetime serviceLifetime)
            where TContext : DbContext
        {
            _ = builder ?? throw new ArgumentNullException(nameof(builder));

            ArgumentException.ThrowIfNullOrWhiteSpace(connectionStringName);

            var conString = builder.Configuration.GetConnectionString(connectionStringName);
            _ = builder.Services.AddDbContext<TContext>(
                options => options.UseSqlite(conString ?? throw new InvalidOperationException($"Connection string '{connectionStringName}' not found.")),
                serviceLifetime);
            return builder;
        }

        /// <summary>Configures Entity Framework Core with SQLite and ASP.NET Core Identity using IHostApplicationBuilder.</summary>
        /// <remarks>This method provides integration with the modern IHostApplicationBuilder pattern and
        /// sets up both Entity Framework Core and ASP.NET Core Identity with the specified user and role types.</remarks>
        /// <typeparam name="TContext">The type of the Entity Framework Core DbContext to use for data access.</typeparam>
        /// <typeparam name="TUser">The type representing application users for ASP.NET Core Identity.</typeparam>
        /// <typeparam name="TRole">The type representing application roles for ASP.NET Core Identity.</typeparam>
        /// <param name="connectionStringName">The name of the connection string in the configuration. Cannot be null or whitespace.</param>
        /// <returns>The same IHostApplicationBuilder instance so that additional calls can be chained.</returns>
        public IHostApplicationBuilder AddSqliteWithIdentity<TContext, TUser, TRole>(string connectionStringName)
            where TContext : DbContext
            where TUser : class
            where TRole : class =>
            builder.AddSqliteWithIdentity<TContext, TUser, TRole>(connectionStringName, ServiceLifetime.Scoped);

        /// <summary>Configures SQLite and Identity with an explicit service lifetime.</summary>
        /// <typeparam name="TContext">The Entity Framework Core DbContext type.</typeparam>
        /// <typeparam name="TUser">The Identity user type.</typeparam>
        /// <typeparam name="TRole">The Identity role type.</typeparam>
        /// <param name="connectionStringName">The configured connection string name.</param>
        /// <param name="serviceLifetime">The lifetime with which to register the DbContext service.</param>
        /// <returns>The same IHostApplicationBuilder instance so that additional calls can be chained.</returns>
        /// <exception cref="ArgumentException">Thrown if connectionStringName is null or consists only of white-space characters.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the specified connection string is not found in the configuration.</exception>
        public IHostApplicationBuilder AddSqliteWithIdentity<TContext, TUser, TRole>(
            string connectionStringName,
            ServiceLifetime serviceLifetime)
            where TContext : DbContext
            where TUser : class
            where TRole : class
        {
            _ = builder ?? throw new ArgumentNullException(nameof(builder));

            ArgumentException.ThrowIfNullOrWhiteSpace(connectionStringName);

            var conString = builder.Configuration.GetConnectionString(connectionStringName);
            _ = builder.Services
                .AddDbContext<TContext>(
                    options => options.UseSqlite(conString ?? throw new InvalidOperationException($"Connection string '{connectionStringName}' not found.")),
                    serviceLifetime)
                .AddDefaultIdentity<TUser>()
                .AddRoles<TRole>()
                .AddEntityFrameworkStores<TContext>();
            return builder;
        }

        /// <summary>Configures Entity Framework Core with SQLite and ASP.NET Core Identity (user only) using IHostApplicationBuilder.</summary>
        /// <remarks>This method provides integration with the modern IHostApplicationBuilder pattern and
        /// sets up both Entity Framework Core and ASP.NET Core Identity with the specified user type only.</remarks>
        /// <typeparam name="TContext">The type of the Entity Framework Core DbContext to use for data access.</typeparam>
        /// <typeparam name="TUser">The type representing application users for ASP.NET Core Identity.</typeparam>
        /// <param name="connectionStringName">The name of the connection string in the configuration. Cannot be null or whitespace.</param>
        /// <returns>The same IHostApplicationBuilder instance so that additional calls can be chained.</returns>
        public IHostApplicationBuilder AddSqliteWithIdentity<TContext, TUser>(string connectionStringName)
            where TContext : DbContext
            where TUser : class =>
            builder.AddSqliteWithIdentity<TContext, TUser>(connectionStringName, ServiceLifetime.Scoped);

        /// <summary>Configures SQLite and user-only Identity with an explicit service lifetime.</summary>
        /// <typeparam name="TContext">The Entity Framework Core DbContext type.</typeparam>
        /// <typeparam name="TUser">The Identity user type.</typeparam>
        /// <param name="connectionStringName">The configured connection string name.</param>
        /// <param name="serviceLifetime">The lifetime with which to register the DbContext service.</param>
        /// <returns>The same IHostApplicationBuilder instance so that additional calls can be chained.</returns>
        /// <exception cref="ArgumentException">Thrown if connectionStringName is null or consists only of white-space characters.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the specified connection string is not found in the configuration.</exception>
        public IHostApplicationBuilder AddSqliteWithIdentity<TContext, TUser>(
            string connectionStringName,
            ServiceLifetime serviceLifetime)
            where TContext : DbContext
            where TUser : class
        {
            _ = builder ?? throw new ArgumentNullException(nameof(builder));

            ArgumentException.ThrowIfNullOrWhiteSpace(connectionStringName);

            var conString = builder.Configuration.GetConnectionString(connectionStringName);
            _ = builder.Services
                .AddDbContext<TContext>(
                    options => options.UseSqlite(conString ?? throw new InvalidOperationException($"Connection string '{connectionStringName}' not found.")),
                    serviceLifetime)
                .AddDefaultIdentity<TUser>()
                .AddEntityFrameworkStores<TContext>();
            return builder;
        }
    }

    /// <summary>Provides extension members for this receiver.</summary>
    /// <param name="hostBuilder">The receiver instance.</param>
    extension(IHostBuilder hostBuilder)
    {
        /// <summary>Configures the host builder to use web host services with custom service configuration and optional scope validation.</summary>
        /// <remarks>This method sets up minimal web host defaults and allows custom service registration for
        /// scenarios where a full web application pipeline is not required. It is useful for integrating web host services
        /// into generic host scenarios or for testing purposes.</remarks>
        /// <param name="configureServices">A delegate that configures the application's service collection. Invoked with the web host builder context and
        /// the service collection.</param>
        /// <returns>The same instance of the host builder for chaining further configuration.</returns>
        public IHostBuilder UseWebHostServices(
            Action<WebHostBuilderContext, IServiceCollection> configureServices) =>
            hostBuilder.UseWebHostServices(configureServices, false);

        /// <summary>Configures web host services and explicitly controls scope validation.</summary>
        /// <param name="configureServices">The web host service configuration delegate.</param>
        /// <param name="validateScopes">true to validate service scopes; otherwise, false.</param>
        /// <returns>The same instance of the host builder for chaining further configuration.</returns>
        public IHostBuilder UseWebHostServices(
            Action<WebHostBuilderContext, IServiceCollection> configureServices,
            bool validateScopes)
        {
            _ = hostBuilder ?? throw new ArgumentNullException(nameof(hostBuilder));

            // Register a no-op pipeline so the web host creates an application service provider.
            return hostBuilder.ConfigureWebHostDefaults(webBuilder =>
                webBuilder.UseDefaultServiceProvider(options => options.ValidateScopes = validateScopes)
                    .Configure(static app => app.Run(static async (_) => await Task.CompletedTask))
                    .ConfigureServices((context, services) => configureServices(context, services)));
        }

        /// <summary>Configures the host builder to use ASP.NET Core web host services with custom service and web host configuration.</summary>
        /// <remarks>This method is intended for advanced scenarios where you need to customize both the web host
        /// and its services during host building. It sets up a minimal web application to ensure the web host services are
        /// available, even if no application logic is provided.</remarks>
        /// <param name="configureServices">A delegate that configures services for the web host. Receives the web host builder context and the service
        /// collection.</param>
        /// <param name="configureWebHost">A delegate that further configures the web host builder. Receives and returns an instance of <see
        /// cref="IWebHostBuilder"/>.</param>
        /// <returns>The same <see cref="IHostBuilder"/> instance for chaining further configuration.</returns>
        public IHostBuilder UseWebHostServices(
            Action<WebHostBuilderContext, IServiceCollection> configureServices,
            Func<IWebHostBuilder, IWebHostBuilder> configureWebHost) =>
            hostBuilder.UseWebHostServices(configureServices, configureWebHost, false);

        /// <summary>Configures web host services and the web host with explicit scope validation.</summary>
        /// <param name="configureServices">The web host service configuration delegate.</param>
        /// <param name="configureWebHost">The web host configuration delegate.</param>
        /// <param name="validateScopes">true to validate service scopes; otherwise, false.</param>
        /// <returns>The same <see cref="IHostBuilder"/> instance for chaining further configuration.</returns>
        public IHostBuilder UseWebHostServices(
            Action<WebHostBuilderContext, IServiceCollection> configureServices,
            Func<IWebHostBuilder, IWebHostBuilder> configureWebHost,
            bool validateScopes)
        {
            _ = hostBuilder ?? throw new ArgumentNullException(nameof(hostBuilder));

            // Register a no-op pipeline so the web host creates an application service provider.
            return hostBuilder.ConfigureWebHostDefaults(webBuilder =>
                configureWebHost(webBuilder)
                    .UseDefaultServiceProvider(options => options.ValidateScopes = validateScopes)
                    .Configure(static app => app.Run(static async (_) => await Task.CompletedTask))
                    .ConfigureServices((context, services) => configureServices(context, services)));
        }

        /// <summary>Configures the specified host builder to use web host services with custom service, web host, and application configuration.</summary>
        /// <remarks>This method enables advanced scenarios where you need to customize the web host's services,
        /// configuration, and application pipeline within a generic host. It is intended for use when integrating ASP.NET
        /// Core web hosting into a generic host setup. The method applies the provided delegates in the order: web host
        /// configuration, service configuration, and application pipeline configuration.</remarks>
        /// <param name="configureServices">A delegate that configures services for the web host. Receives the web host builder context and the service
        /// collection to configure.</param>
        /// <param name="configureWebHost">A delegate that configures the web host builder. Receives the current web host builder and returns the
        /// configured builder.</param>
        /// <param name="configureApp">A delegate that configures the application's request pipeline. Receives the application builder and returns the
        /// configured builder.</param>
        /// <returns>The same instance of the host builder for chaining further configuration.</returns>
        public IHostBuilder UseWebHostServices(
            Action<WebHostBuilderContext, IServiceCollection> configureServices,
            Func<IWebHostBuilder, IWebHostBuilder> configureWebHost,
            Func<IApplicationBuilder, IApplicationBuilder> configureApp) =>
            hostBuilder.UseWebHostServices(configureServices, configureWebHost, configureApp, false);

        /// <summary>Configures web services, web host, and application with explicit scope validation.</summary>
        /// <param name="configureServices">The web host service configuration delegate.</param>
        /// <param name="configureWebHost">The web host configuration delegate.</param>
        /// <param name="configureApp">The application pipeline configuration delegate.</param>
        /// <param name="validateScopes">true to validate service scopes; otherwise, false.</param>
        /// <returns>The same instance of the host builder for chaining further configuration.</returns>
        public IHostBuilder UseWebHostServices(
            Action<WebHostBuilderContext, IServiceCollection> configureServices,
            Func<IWebHostBuilder, IWebHostBuilder> configureWebHost,
            Func<IApplicationBuilder, IApplicationBuilder> configureApp,
            bool validateScopes)
        {
            _ = hostBuilder ?? throw new ArgumentNullException(nameof(hostBuilder));

            // Register a no-op pipeline so the web host creates an application service provider.
            return hostBuilder.ConfigureWebHostDefaults(webBuilder =>
                configureWebHost(webBuilder)
                    .UseDefaultServiceProvider(options => options.ValidateScopes = validateScopes)
                    .Configure(app => configureApp(app).Run(static async (_) => await Task.CompletedTask))
                    .ConfigureServices((context, services) => configureServices(context, services)));
        }
    }

    /// <summary>Provides extension members for this receiver.</summary>
    /// <param name="services">The receiver instance.</param>
    extension(IServiceCollection services)
    {
        /// <summary>Configures Entity Framework Core with a SQLite provider and sets up ASP.NET Core Identity using the specified context, user, and role types.</summary>
        /// <remarks>This method registers the DbContext with the SQLite provider and configures ASP.NET Core
        /// Identity to use the specified user and role types with Entity Framework Core stores. It is typically called
        /// during application startup to enable authentication and authorization using SQLite as the backing
        /// store.</remarks>
        /// <typeparam name="TContext">The type of the Entity Framework Core DbContext to use for data access.</typeparam>
        /// <typeparam name="TUser">The type representing application users for ASP.NET Core Identity.</typeparam>
        /// <typeparam name="TRole">The type representing application roles for ASP.NET Core Identity.</typeparam>
        /// <param name="context">The web host builder context containing configuration and environment information. Cannot be null.</param>
        /// <param name="connectionStringName">The name of the connection string in the configuration to use for the SQLite database. Cannot be null or
        /// whitespace.</param>
        /// <returns>The same IServiceCollection instance so that additional calls can be chained.</returns>
        public IServiceCollection UseEntityFrameworkCoreSqlite<TContext, TUser, TRole>(
            WebHostBuilderContext context,
            string connectionStringName)
            where TContext : DbContext
            where TUser : class
            where TRole : class =>
            services.UseEntityFrameworkCoreSqlite<TContext, TUser, TRole>(
                context,
                connectionStringName,
                ServiceLifetime.Scoped);

        /// <summary>Configures SQLite and Identity with an explicit service lifetime.</summary>
        /// <typeparam name="TContext">The Entity Framework Core DbContext type.</typeparam>
        /// <typeparam name="TUser">The Identity user type.</typeparam>
        /// <typeparam name="TRole">The Identity role type.</typeparam>
        /// <param name="context">The web host builder context.</param>
        /// <param name="connectionStringName">The configured connection string name.</param>
        /// <param name="serviceLifetime">The lifetime with which to register the DbContext service.</param>
        /// <returns>The same IServiceCollection instance so that additional calls can be chained.</returns>
        /// <exception cref="ArgumentException">Thrown if connectionStringName is null or consists only of white-space characters.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the specified connection string is not found in the configuration.</exception>
        public IServiceCollection UseEntityFrameworkCoreSqlite<TContext, TUser, TRole>(
            WebHostBuilderContext context,
            string connectionStringName,
            ServiceLifetime serviceLifetime)
            where TContext : DbContext
            where TUser : class
            where TRole : class
        {
            _ = services ?? throw new ArgumentNullException(nameof(services));
            _ = context ?? throw new ArgumentNullException(nameof(context));

            ArgumentException.ThrowIfNullOrWhiteSpace(connectionStringName);

            var conString = context.Configuration.GetConnectionString(connectionStringName);
            _ = services
                .AddDbContext<TContext>(
                    options => options.UseSqlite(conString ?? throw new InvalidOperationException($"Connection string '{connectionStringName}' not found.")),
                    serviceLifetime)
                .AddDefaultIdentity<TUser>()
                .AddRoles<TRole>()
                .AddEntityFrameworkStores<TContext>();
            return services;
        }

        /// <summary>Configures Entity Framework Core with a SQLite provider and ASP.NET Core Identity using the specified DbContext and user type.</summary>
        /// <remarks>This method registers the specified DbContext with a SQLite provider and configures ASP.NET
        /// Core Identity to use Entity Framework Core stores. It is typically called during application startup to enable
        /// authentication and data access using SQLite.</remarks>
        /// <typeparam name="TContext">The type of the DbContext to use for Entity Framework Core operations.</typeparam>
        /// <typeparam name="TUser">The type representing the user entity for ASP.NET Core Identity.</typeparam>
        /// <param name="context">The WebHostBuilderContext containing application configuration and environment information.</param>
        /// <param name="connectionStringName">The name of the connection string in the configuration to use for the SQLite database. Cannot be null or
        /// whitespace.</param>
        /// <returns>The IServiceCollection instance configured for SQLite and Identity.</returns>
        public IServiceCollection UseEntityFrameworkCoreSqlite<TContext, TUser>(
            WebHostBuilderContext context,
            string connectionStringName)
            where TContext : DbContext
            where TUser : class =>
            services.UseEntityFrameworkCoreSqlite<TContext, TUser>(
                context,
                connectionStringName,
                ServiceLifetime.Scoped);

        /// <summary>Configures SQLite and user-only Identity with an explicit service lifetime.</summary>
        /// <typeparam name="TContext">The Entity Framework Core DbContext type.</typeparam>
        /// <typeparam name="TUser">The Identity user type.</typeparam>
        /// <param name="context">The web host builder context.</param>
        /// <param name="connectionStringName">The configured connection string name.</param>
        /// <param name="serviceLifetime">The lifetime with which to register the DbContext service.</param>
        /// <returns>The IServiceCollection instance with Entity Framework Core and Identity services configured for SQLite.</returns>
        /// <exception cref="ArgumentException">Thrown if connectionStringName is null or consists only of white-space characters.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the specified connection string is not found in the configuration.</exception>
        public IServiceCollection UseEntityFrameworkCoreSqlite<TContext, TUser>(
            WebHostBuilderContext context,
            string connectionStringName,
            ServiceLifetime serviceLifetime)
            where TContext : DbContext
            where TUser : class
        {
            _ = services ?? throw new ArgumentNullException(nameof(services));
            _ = context ?? throw new ArgumentNullException(nameof(context));

            ArgumentException.ThrowIfNullOrWhiteSpace(connectionStringName);

            var conString = context.Configuration.GetConnectionString(connectionStringName);
            _ = services
                .AddDbContext<TContext>(
                    options => options.UseSqlite(conString ?? throw new InvalidOperationException($"Connection string '{connectionStringName}' not found.")),
                    serviceLifetime)
                .AddDefaultIdentity<TUser>()
                .AddEntityFrameworkStores<TContext>();
            return services;
        }

        /// <summary>Configures Entity Framework Core with SQLite for the specified DbContext without ASP.NET Core Identity.</summary>
        /// <remarks>Use this method when you need Entity Framework Core with SQLite but do not require
        /// ASP.NET Core Identity services. This is useful for applications that handle authentication externally or
        /// do not need user management.</remarks>
        /// <typeparam name="TContext">The type of the Entity Framework Core DbContext to use for data access.</typeparam>
        /// <param name="configuration">The configuration instance containing the connection string. Cannot be null.</param>
        /// <param name="connectionStringName">The name of the connection string in the configuration. Cannot be null or whitespace.</param>
        /// <returns>The same IServiceCollection instance so that additional calls can be chained.</returns>
        public IServiceCollection AddSqliteDbContext<TContext>(
            IConfiguration configuration,
            string connectionStringName)
            where TContext : DbContext =>
            services.AddSqliteDbContext<TContext>(
                configuration,
                connectionStringName,
                ServiceLifetime.Scoped);

        /// <summary>Configures a SQLite DbContext with an explicit service lifetime.</summary>
        /// <typeparam name="TContext">The Entity Framework Core DbContext type.</typeparam>
        /// <param name="configuration">The application configuration.</param>
        /// <param name="connectionStringName">The configured connection string name.</param>
        /// <param name="serviceLifetime">The lifetime with which to register the DbContext service.</param>
        /// <returns>The same IServiceCollection instance so that additional calls can be chained.</returns>
        /// <exception cref="ArgumentException">Thrown if connectionStringName is null or consists only of white-space characters.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the specified connection string is not found in the configuration.</exception>
        public IServiceCollection AddSqliteDbContext<TContext>(
            IConfiguration configuration,
            string connectionStringName,
            ServiceLifetime serviceLifetime)
            where TContext : DbContext
        {
            _ = services ?? throw new ArgumentNullException(nameof(services));
            _ = configuration ?? throw new ArgumentNullException(nameof(configuration));

            ArgumentException.ThrowIfNullOrWhiteSpace(connectionStringName);

            var conString = configuration.GetConnectionString(connectionStringName);
            _ = services.AddDbContext<TContext>(
                options => options.UseSqlite(conString ?? throw new InvalidOperationException($"Connection string '{connectionStringName}' not found.")),
                serviceLifetime);
            return services;
        }

        /// <summary>Configures Entity Framework Core with SQLite for the specified DbContext using a direct connection string.</summary>
        /// <remarks>Use this overload when you have a connection string available directly rather than from
        /// configuration. This is useful for testing scenarios or when connection strings are obtained from
        /// other sources such as environment variables or secret managers.</remarks>
        /// <typeparam name="TContext">The type of the Entity Framework Core DbContext to use for data access.</typeparam>
        /// <param name="connectionString">The SQLite connection string. Cannot be null or whitespace.</param>
        /// <returns>The same IServiceCollection instance so that additional calls can be chained.</returns>
        public IServiceCollection AddSqliteDbContextWithConnectionString<TContext>(string connectionString)
            where TContext : DbContext =>
            services.AddSqliteDbContextWithConnectionString<TContext>(
                connectionString,
                ServiceLifetime.Scoped);

        /// <summary>Configures a SQLite DbContext from a connection string with an explicit lifetime.</summary>
        /// <typeparam name="TContext">The Entity Framework Core DbContext type.</typeparam>
        /// <param name="connectionString">The SQLite connection string.</param>
        /// <param name="serviceLifetime">The lifetime with which to register the DbContext service.</param>
        /// <returns>The same IServiceCollection instance so that additional calls can be chained.</returns>
        /// <exception cref="ArgumentException">Thrown if connectionString is null or consists only of white-space characters.</exception>
        public IServiceCollection AddSqliteDbContextWithConnectionString<TContext>(
            string connectionString,
            ServiceLifetime serviceLifetime)
            where TContext : DbContext
        {
            _ = services ?? throw new ArgumentNullException(nameof(services));

            ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

            _ = services.AddDbContext<TContext>(
                options => options.UseSqlite(connectionString),
                serviceLifetime);
            return services;
        }
    }

    /// <summary>Creates a SQLite connection string for an in-memory database.</summary>
    /// <remarks>In-memory SQLite databases are useful for testing scenarios where you need a fast,
    /// isolated database that doesn't persist to disk. Note that in-memory databases only persist
    /// as long as the connection remains open.</remarks>
    /// <returns>A SQLite connection string for an in-memory database.</returns>
    public static string CreateInMemoryConnectionString() =>
        CreateInMemoryConnectionString(null);

    /// <summary>Creates a SQLite connection string for a named in-memory database.</summary>
    /// <param name="databaseName">The in-memory database name, or null to use an isolated in-memory database.</param>
    /// <returns>A SQLite connection string for an in-memory database.</returns>
    public static string CreateInMemoryConnectionString(string? databaseName) =>
        string.IsNullOrWhiteSpace(databaseName)
            ? "DataSource=:memory:"
            : $"DataSource={databaseName};Mode=Memory;Cache=Shared";

    /// <summary>Creates a SQLite connection string for a file-based database.</summary>
    /// <remarks>Use this helper to create properly formatted SQLite connection strings for
    /// file-based databases. The path can be absolute or relative.</remarks>
    /// <param name="filePath">The path to the SQLite database file. Cannot be null or whitespace.</param>
    /// <returns>A SQLite connection string for the specified file.</returns>
    /// <exception cref="ArgumentException">Thrown if filePath is null or consists only of white-space characters.</exception>
    public static string CreateFileConnectionString(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        return $"DataSource={filePath}";
    }
}
