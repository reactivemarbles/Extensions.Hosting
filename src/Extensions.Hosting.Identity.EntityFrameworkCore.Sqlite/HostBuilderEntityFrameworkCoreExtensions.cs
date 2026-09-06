// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
        /// <param name="contextFactory">Creates the context instance from the service provider and typed context options.</param>
        /// <returns>The same IHostApplicationBuilder instance so that additional calls can be chained.</returns>
        public IHostApplicationBuilder AddSqliteDbContext<TContext>(
            string connectionStringName,
            Func<IServiceProvider, DbContextOptions<TContext>, TContext> contextFactory)
            where TContext : DbContext =>
            builder.AddSqliteDbContext(connectionStringName, contextFactory, ServiceLifetime.Scoped);

        /// <summary>Configures Entity Framework Core with SQLite and an explicit service lifetime.</summary>
        /// <typeparam name="TContext">The Entity Framework Core DbContext type.</typeparam>
        /// <param name="connectionStringName">The configured connection string name.</param>
        /// <param name="contextFactory">Creates the context instance from the service provider and typed context options.</param>
        /// <param name="serviceLifetime">The lifetime with which to register the DbContext service.</param>
        /// <returns>The same IHostApplicationBuilder instance so that additional calls can be chained.</returns>
        /// <exception cref="ArgumentException">Thrown if connectionStringName is null or consists only of white-space characters.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the specified connection string is not found in the configuration.</exception>
        public IHostApplicationBuilder AddSqliteDbContext<TContext>(
            string connectionStringName,
            Func<IServiceProvider, DbContextOptions<TContext>, TContext> contextFactory,
            ServiceLifetime serviceLifetime)
            where TContext : DbContext
        {
            _ = builder ?? throw new ArgumentNullException(nameof(builder));
            _ = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));

            ArgumentException.ThrowIfNullOrWhiteSpace(connectionStringName);

            var conString = builder.Configuration.GetConnectionString(connectionStringName)
                ?? throw new InvalidOperationException($"Connection string '{connectionStringName}' not found.");
            _ = AddSqliteDbContextCore(builder.Services, conString, contextFactory, serviceLifetime);
            return builder;
        }

        /// <summary>Configures Entity Framework Core with SQLite and ASP.NET Core Identity using IHostApplicationBuilder.</summary>
        /// <remarks>This method provides integration with the modern IHostApplicationBuilder pattern and
        /// sets up both Entity Framework Core and ASP.NET Core Identity with caller-provided identity configuration.</remarks>
        /// <typeparam name="TContext">The type of the Entity Framework Core DbContext to use for data access.</typeparam>
        /// <param name="connectionStringName">The name of the connection string in the configuration. Cannot be null or whitespace.</param>
        /// <param name="contextFactory">Creates the context instance from the service provider and typed context options.</param>
        /// <param name="configureIdentity">Configures Identity services and returns the Identity builder to attach Entity Framework stores to.</param>
        /// <returns>The same IHostApplicationBuilder instance so that additional calls can be chained.</returns>
        public IHostApplicationBuilder AddSqliteWithIdentity<TContext>(
            string connectionStringName,
            Func<IServiceProvider, DbContextOptions<TContext>, TContext> contextFactory,
            Func<IServiceCollection, IdentityBuilder> configureIdentity)
            where TContext : DbContext =>
            builder.AddSqliteWithIdentity(connectionStringName, contextFactory, configureIdentity, ServiceLifetime.Scoped);

        /// <summary>Configures SQLite and Identity with an explicit service lifetime.</summary>
        /// <typeparam name="TContext">The Entity Framework Core DbContext type.</typeparam>
        /// <param name="connectionStringName">The configured connection string name.</param>
        /// <param name="contextFactory">Creates the context instance from the service provider and typed context options.</param>
        /// <param name="configureIdentity">Configures Identity services and returns the Identity builder to attach Entity Framework stores to.</param>
        /// <param name="serviceLifetime">The lifetime with which to register the DbContext service.</param>
        /// <returns>The same IHostApplicationBuilder instance so that additional calls can be chained.</returns>
        /// <exception cref="ArgumentException">Thrown if connectionStringName is null or consists only of white-space characters.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the specified connection string is not found in the configuration.</exception>
        public IHostApplicationBuilder AddSqliteWithIdentity<TContext>(
            string connectionStringName,
            Func<IServiceProvider, DbContextOptions<TContext>, TContext> contextFactory,
            Func<IServiceCollection, IdentityBuilder> configureIdentity,
            ServiceLifetime serviceLifetime)
            where TContext : DbContext
        {
            _ = builder ?? throw new ArgumentNullException(nameof(builder));
            _ = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _ = configureIdentity ?? throw new ArgumentNullException(nameof(configureIdentity));

            ArgumentException.ThrowIfNullOrWhiteSpace(connectionStringName);

            var conString = builder.Configuration.GetConnectionString(connectionStringName)
                ?? throw new InvalidOperationException($"Connection string '{connectionStringName}' not found.");
            _ = AddSqliteDbContextCore(builder.Services, conString, contextFactory, serviceLifetime);
            _ = (configureIdentity(builder.Services) ?? throw new InvalidOperationException("The identity configuration delegate must return an IdentityBuilder."))
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
        /// <summary>Configures Entity Framework Core with a SQLite provider and caller-provided Identity services.</summary>
        /// <remarks>This method registers the DbContext with the SQLite provider and configures ASP.NET Core
        /// Identity using the supplied callback. It is typically called during application startup to enable
        /// authentication and authorization using SQLite as the backing store.</remarks>
        /// <typeparam name="TContext">The type of the Entity Framework Core DbContext to use for data access.</typeparam>
        /// <param name="context">The web host builder context containing configuration and environment information. Cannot be null.</param>
        /// <param name="connectionStringName">The name of the connection string in the configuration to use for the SQLite database. Cannot be null or
        /// whitespace.</param>
        /// <param name="contextFactory">Creates the context instance from the service provider and typed context options.</param>
        /// <param name="configureIdentity">Configures Identity services and returns the Identity builder to attach Entity Framework stores to.</param>
        /// <returns>The same IServiceCollection instance so that additional calls can be chained.</returns>
        public IServiceCollection UseEntityFrameworkCoreSqlite<TContext>(
            WebHostBuilderContext context,
            string connectionStringName,
            Func<IServiceProvider, DbContextOptions<TContext>, TContext> contextFactory,
            Func<IServiceCollection, IdentityBuilder> configureIdentity)
            where TContext : DbContext =>
            services.UseEntityFrameworkCoreSqlite(
                context,
                connectionStringName,
                contextFactory,
                configureIdentity,
                ServiceLifetime.Scoped);

        /// <summary>Configures SQLite and Identity with an explicit service lifetime.</summary>
        /// <typeparam name="TContext">The Entity Framework Core DbContext type.</typeparam>
        /// <param name="context">The web host builder context.</param>
        /// <param name="connectionStringName">The configured connection string name.</param>
        /// <param name="contextFactory">Creates the context instance from the service provider and typed context options.</param>
        /// <param name="configureIdentity">Configures Identity services and returns the Identity builder to attach Entity Framework stores to.</param>
        /// <param name="serviceLifetime">The lifetime with which to register the DbContext service.</param>
        /// <returns>The same IServiceCollection instance so that additional calls can be chained.</returns>
        /// <exception cref="ArgumentException">Thrown if connectionStringName is null or consists only of white-space characters.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the specified connection string is not found in the configuration.</exception>
        public IServiceCollection UseEntityFrameworkCoreSqlite<TContext>(
            WebHostBuilderContext context,
            string connectionStringName,
            Func<IServiceProvider, DbContextOptions<TContext>, TContext> contextFactory,
            Func<IServiceCollection, IdentityBuilder> configureIdentity,
            ServiceLifetime serviceLifetime)
            where TContext : DbContext
        {
            _ = services ?? throw new ArgumentNullException(nameof(services));
            _ = context ?? throw new ArgumentNullException(nameof(context));
            _ = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _ = configureIdentity ?? throw new ArgumentNullException(nameof(configureIdentity));

            ArgumentException.ThrowIfNullOrWhiteSpace(connectionStringName);

            var conString = context.Configuration.GetConnectionString(connectionStringName)
                ?? throw new InvalidOperationException($"Connection string '{connectionStringName}' not found.");
            _ = AddSqliteDbContextCore(services, conString, contextFactory, serviceLifetime);
            _ = (configureIdentity(services) ?? throw new InvalidOperationException("The identity configuration delegate must return an IdentityBuilder."))
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
        /// <param name="contextFactory">Creates the context instance from the service provider and typed context options.</param>
        /// <returns>The same IServiceCollection instance so that additional calls can be chained.</returns>
        public IServiceCollection AddSqliteDbContext<TContext>(
            IConfiguration configuration,
            string connectionStringName,
            Func<IServiceProvider, DbContextOptions<TContext>, TContext> contextFactory)
            where TContext : DbContext =>
            services.AddSqliteDbContext(
                configuration,
                connectionStringName,
                contextFactory,
                ServiceLifetime.Scoped);

        /// <summary>Configures a SQLite DbContext with an explicit service lifetime.</summary>
        /// <typeparam name="TContext">The Entity Framework Core DbContext type.</typeparam>
        /// <param name="configuration">The application configuration.</param>
        /// <param name="connectionStringName">The configured connection string name.</param>
        /// <param name="contextFactory">Creates the context instance from the service provider and typed context options.</param>
        /// <param name="serviceLifetime">The lifetime with which to register the DbContext service.</param>
        /// <returns>The same IServiceCollection instance so that additional calls can be chained.</returns>
        /// <exception cref="ArgumentException">Thrown if connectionStringName is null or consists only of white-space characters.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the specified connection string is not found in the configuration.</exception>
        public IServiceCollection AddSqliteDbContext<TContext>(
            IConfiguration configuration,
            string connectionStringName,
            Func<IServiceProvider, DbContextOptions<TContext>, TContext> contextFactory,
            ServiceLifetime serviceLifetime)
            where TContext : DbContext
        {
            _ = services ?? throw new ArgumentNullException(nameof(services));
            _ = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _ = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));

            ArgumentException.ThrowIfNullOrWhiteSpace(connectionStringName);

            var conString = configuration.GetConnectionString(connectionStringName)
                ?? throw new InvalidOperationException($"Connection string '{connectionStringName}' not found.");
            _ = AddSqliteDbContextCore(services, conString, contextFactory, serviceLifetime);
            return services;
        }

        /// <summary>Configures Entity Framework Core with SQLite for the specified DbContext using a direct connection string.</summary>
        /// <remarks>Use this overload when you have a connection string available directly rather than from
        /// configuration. This is useful for testing scenarios or when connection strings are obtained from
        /// other sources such as environment variables or secret managers.</remarks>
        /// <typeparam name="TContext">The type of the Entity Framework Core DbContext to use for data access.</typeparam>
        /// <param name="connectionString">The SQLite connection string. Cannot be null or whitespace.</param>
        /// <param name="contextFactory">Creates the context instance from the service provider and typed context options.</param>
        /// <returns>The same IServiceCollection instance so that additional calls can be chained.</returns>
        public IServiceCollection AddSqliteDbContextWithConnectionString<TContext>(
            string connectionString,
            Func<IServiceProvider, DbContextOptions<TContext>, TContext> contextFactory)
            where TContext : DbContext =>
            services.AddSqliteDbContextWithConnectionString(
                connectionString,
                contextFactory,
                ServiceLifetime.Scoped);

        /// <summary>Configures a SQLite DbContext from a connection string with an explicit lifetime.</summary>
        /// <typeparam name="TContext">The Entity Framework Core DbContext type.</typeparam>
        /// <param name="connectionString">The SQLite connection string.</param>
        /// <param name="contextFactory">Creates the context instance from the service provider and typed context options.</param>
        /// <param name="serviceLifetime">The lifetime with which to register the DbContext service.</param>
        /// <returns>The same IServiceCollection instance so that additional calls can be chained.</returns>
        /// <exception cref="ArgumentException">Thrown if connectionString is null or consists only of white-space characters.</exception>
        public IServiceCollection AddSqliteDbContextWithConnectionString<TContext>(
            string connectionString,
            Func<IServiceProvider, DbContextOptions<TContext>, TContext> contextFactory,
            ServiceLifetime serviceLifetime)
            where TContext : DbContext
        {
            _ = services ?? throw new ArgumentNullException(nameof(services));
            _ = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));

            ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

            _ = AddSqliteDbContextCore(services, connectionString, contextFactory, serviceLifetime);
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

    /// <summary>Registers SQLite options and a typed context factory without overriding existing context registrations.</summary>
    /// <typeparam name="TContext">The Entity Framework Core DbContext type.</typeparam>
    /// <param name="services">The services to configure.</param>
    /// <param name="connectionString">The SQLite connection string.</param>
    /// <param name="contextFactory">Creates the context instance from the service provider and typed context options.</param>
    /// <param name="serviceLifetime">The lifetime with which to register the DbContext service.</param>
    /// <returns>The configured service collection.</returns>
    private static IServiceCollection AddSqliteDbContextCore<TContext>(
        IServiceCollection services,
        string connectionString,
        Func<IServiceProvider, DbContextOptions<TContext>, TContext> contextFactory,
        ServiceLifetime serviceLifetime)
        where TContext : DbContext
    {
        services.TryAdd(ServiceDescriptor.Describe(
            typeof(TContext),
            serviceProvider => contextFactory(
                serviceProvider,
                serviceProvider.GetRequiredService<DbContextOptions<TContext>>()),
            serviceLifetime));
        _ = services.AddDbContext<TContext>(
            options => options.UseSqlite(connectionString),
            serviceLifetime);
        return services;
    }
}
