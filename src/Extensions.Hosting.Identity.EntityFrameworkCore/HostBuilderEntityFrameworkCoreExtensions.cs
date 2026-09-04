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

/// <summary>Provides extension methods for configuring Entity Framework Core with SQL Server and web host services on ASP.NET Core host builders and service collections.</summary>
/// <remarks>These extensions simplify the setup of Entity Framework Core and related identity services in ASP.NET
/// Core applications, as well as the configuration of web host services for integration and testing scenarios. Methods
/// in this class are intended to be used during application startup to register required services and configure the
/// host environment.</remarks>
public static class HostBuilderEntityFrameworkCoreExtensions
{
    /// <summary>Provides extension members for this receiver.</summary>
    /// <param name="configuration">The receiver instance.</param>
    extension(IConfiguration configuration)
    {
        /// <summary>Validates that a connection string exists in the configuration.</summary>
        /// <param name="connectionStringName">The name of the connection string to validate.</param>
        /// <returns>True if the connection string exists and is not empty; otherwise, false.</returns>
        public bool HasConnectionString(string connectionStringName)
        {
            _ = configuration ?? throw new ArgumentNullException(nameof(configuration));

            if (string.IsNullOrWhiteSpace(connectionStringName))
            {
                return false;
            }

            var connectionString = configuration.GetConnectionString(connectionStringName);
            return !string.IsNullOrWhiteSpace(connectionString);
        }

        /// <summary>Gets a connection string from configuration, throwing a descriptive exception if not found.</summary>
        /// <param name="connectionStringName">The name of the connection string to retrieve.</param>
        /// <returns>The connection string value.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the connection string is not found or is empty.</exception>
        public string GetRequiredConnectionString(string connectionStringName)
        {
            _ = configuration ?? throw new ArgumentNullException(nameof(configuration));

            ArgumentException.ThrowIfNullOrWhiteSpace(connectionStringName);

            var connectionString = configuration.GetConnectionString(connectionStringName);
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    $"Connection string '{connectionStringName}' not found in configuration. Ensure it is defined "
                    + $"in appsettings.json or environment variables under 'ConnectionStrings:{connectionStringName}'.");
            }

            return connectionString;
        }
    }

    /// <summary>Provides extension members for this receiver.</summary>
    /// <param name="builder">The receiver instance.</param>
    extension(IHostApplicationBuilder builder)
    {
        /// <summary>Configures Entity Framework Core with SQL Server using the IHostApplicationBuilder pattern.</summary>
        /// <remarks>This method provides integration with the modern IHostApplicationBuilder pattern
        /// introduced in .NET 7+. It registers the DbContext with SQL Server using the connection string
        /// from configuration.</remarks>
        /// <typeparam name="TContext">The type of the Entity Framework Core DbContext to use for data access.</typeparam>
        /// <param name="connectionStringName">The name of the connection string in the configuration. Cannot be null or whitespace.</param>
        /// <param name="contextFactory">Creates the context instance from the service provider and typed context options.</param>
        /// <returns>The same IHostApplicationBuilder instance so that additional calls can be chained.</returns>
        public IHostApplicationBuilder AddSqlServerDbContext<TContext>(
            string connectionStringName,
            Func<IServiceProvider, DbContextOptions<TContext>, TContext> contextFactory)
            where TContext : DbContext =>
            builder.AddSqlServerDbContext(connectionStringName, contextFactory, ServiceLifetime.Scoped);

        /// <summary>Configures Entity Framework Core with SQL Server and an explicit service lifetime.</summary>
        /// <typeparam name="TContext">The Entity Framework Core DbContext type.</typeparam>
        /// <param name="connectionStringName">The configured connection string name.</param>
        /// <param name="contextFactory">Creates the context instance from the service provider and typed context options.</param>
        /// <param name="serviceLifetime">The lifetime with which to register the DbContext service.</param>
        /// <returns>The same IHostApplicationBuilder instance so that additional calls can be chained.</returns>
        /// <exception cref="ArgumentException">Thrown if connectionStringName is null or consists only of white-space characters.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the specified connection string is not found in the configuration.</exception>
        public IHostApplicationBuilder AddSqlServerDbContext<TContext>(
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
            _ = AddSqlServerDbContextCore(builder.Services, conString, contextFactory, serviceLifetime);
            return builder;
        }

        /// <summary>Configures Entity Framework Core with SQL Server and ASP.NET Core Identity using IHostApplicationBuilder.</summary>
        /// <remarks>This method provides integration with the modern IHostApplicationBuilder pattern and
        /// sets up both Entity Framework Core and ASP.NET Core Identity with caller-provided identity configuration.</remarks>
        /// <typeparam name="TContext">The type of the Entity Framework Core DbContext to use for data access.</typeparam>
        /// <param name="connectionStringName">The name of the connection string in the configuration. Cannot be null or whitespace.</param>
        /// <param name="contextFactory">Creates the context instance from the service provider and typed context options.</param>
        /// <param name="configureIdentity">Configures Identity services and returns the Identity builder to attach Entity Framework stores to.</param>
        /// <returns>The same IHostApplicationBuilder instance so that additional calls can be chained.</returns>
        public IHostApplicationBuilder AddSqlServerWithIdentity<TContext>(
            string connectionStringName,
            Func<IServiceProvider, DbContextOptions<TContext>, TContext> contextFactory,
            Func<IServiceCollection, IdentityBuilder> configureIdentity)
            where TContext : DbContext =>
            builder.AddSqlServerWithIdentity(connectionStringName, contextFactory, configureIdentity, ServiceLifetime.Scoped);

        /// <summary>Configures SQL Server and Identity with an explicit service lifetime.</summary>
        /// <typeparam name="TContext">The Entity Framework Core DbContext type.</typeparam>
        /// <param name="connectionStringName">The configured connection string name.</param>
        /// <param name="contextFactory">Creates the context instance from the service provider and typed context options.</param>
        /// <param name="configureIdentity">Configures Identity services and returns the Identity builder to attach Entity Framework stores to.</param>
        /// <param name="serviceLifetime">The lifetime with which to register the DbContext service.</param>
        /// <returns>The same IHostApplicationBuilder instance so that additional calls can be chained.</returns>
        /// <exception cref="ArgumentException">Thrown if connectionStringName is null or consists only of white-space characters.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the specified connection string is not found in the configuration.</exception>
        public IHostApplicationBuilder AddSqlServerWithIdentity<TContext>(
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
            _ = AddSqlServerDbContextCore(builder.Services, conString, contextFactory, serviceLifetime);
            _ = (configureIdentity(builder.Services) ?? throw new InvalidOperationException("The identity configuration delegate must return an IdentityBuilder."))
                .AddEntityFrameworkStores<TContext>();
            return builder;
        }
    }

    /// <summary>Provides extension members for this receiver.</summary>
    /// <param name="hostBuilder">The receiver instance.</param>
    extension(IHostBuilder hostBuilder)
    {
        /// <summary>Configures the host to use web host services and allows additional service configuration for the web host builder.</summary>
        /// <remarks>This method sets up the web host with default configurations and applies the specified
        /// service configuration. It is useful for scenarios where you want to customize the web host's service
        /// registrations within a generic host setup.</remarks>
        /// <param name="configureServices">A delegate that configures services for the web host. Receives the web host builder context and the service
        /// collection to configure.</param>
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

        /// <summary>Configures the specified host builder to use ASP.NET Core web host services with custom service and web host configuration.</summary>
        /// <remarks>This method is intended for advanced scenarios where custom service registration and web host
        /// configuration are required. It provides a way to integrate ASP.NET Core web hosting features into a generic host
        /// builder pipeline.</remarks>
        /// <param name="configureServices">A delegate that configures services for the web host. Receives the web host builder context and the service
        /// collection.</param>
        /// <param name="configureWebHost">A delegate that configures the web host builder. Receives the current web host builder and returns the
        /// configured instance.</param>
        /// <returns>The same IHostBuilder instance for chaining further configuration.</returns>
        public IHostBuilder UseWebHostServices(
            Action<WebHostBuilderContext, IServiceCollection> configureServices,
            Func<IWebHostBuilder, IWebHostBuilder> configureWebHost) =>
            hostBuilder.UseWebHostServices(configureServices, configureWebHost, false);

        /// <summary>Configures web host services and the web host with explicit scope validation.</summary>
        /// <param name="configureServices">The web host service configuration delegate.</param>
        /// <param name="configureWebHost">The web host configuration delegate.</param>
        /// <param name="validateScopes">true to validate service scopes; otherwise, false.</param>
        /// <returns>The same IHostBuilder instance for chaining further configuration.</returns>
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

        /// <summary>Configures the host builder to use web host services with custom service, web host, and application configurations.</summary>
        /// <remarks>This method allows advanced customization of the web host's service collection, web host
        /// builder, and application pipeline during host configuration. It is intended for scenarios where the default web
        /// host setup needs to be extended or replaced with custom logic.</remarks>
        /// <param name="configureServices">A delegate that configures services for the web host. Receives the web host builder context and the service
        /// collection to configure.</param>
        /// <param name="configureWebHost">A delegate that configures the web host builder. Receives the current web host builder and returns the
        /// configured builder.</param>
        /// <param name="configureApp">A delegate that configures the application builder. Receives the application builder and returns the configured
        /// builder.</param>
        /// <returns>The same IHostBuilder instance for chaining further configuration.</returns>
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
        /// <returns>The same IHostBuilder instance for chaining further configuration.</returns>
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
        /// <summary>Configures SQL Server and ASP.NET Core Identity with caller-provided identity services.</summary>
        /// <typeparam name="TContext">The type of the Entity Framework Core DbContext to use for data access.</typeparam>
        /// <param name="context">The web host builder context containing configuration and environment information. Cannot be null.</param>
        /// <param name="connectionStringName">The name of the connection string in the application's configuration to use for the SQL Server database. Cannot
        /// be null or whitespace.</param>
        /// <param name="contextFactory">Creates the context instance from the service provider and typed context options.</param>
        /// <param name="configureIdentity">Configures Identity services and returns the Identity builder to attach Entity Framework stores to.</param>
        /// <returns>The same IServiceCollection instance so that additional calls can be chained.</returns>
        public IServiceCollection UseEntityFrameworkCoreSqlServer<TContext>(
            WebHostBuilderContext context,
            string connectionStringName,
            Func<IServiceProvider, DbContextOptions<TContext>, TContext> contextFactory,
            Func<IServiceCollection, IdentityBuilder> configureIdentity)
            where TContext : DbContext =>
            services.UseEntityFrameworkCoreSqlServer(
                context,
                connectionStringName,
                contextFactory,
                configureIdentity,
                ServiceLifetime.Scoped);

        /// <summary>Configures SQL Server and Identity with an explicit service lifetime.</summary>
        /// <typeparam name="TContext">The Entity Framework Core DbContext type.</typeparam>
        /// <param name="context">The web host builder context.</param>
        /// <param name="connectionStringName">The configured connection string name.</param>
        /// <param name="contextFactory">Creates the context instance from the service provider and typed context options.</param>
        /// <param name="configureIdentity">Configures Identity services and returns the Identity builder to attach Entity Framework stores to.</param>
        /// <param name="serviceLifetime">The lifetime with which to register the DbContext service.</param>
        /// <returns>The same IServiceCollection instance so that additional calls can be chained.</returns>
        /// <exception cref="ArgumentException">Thrown if connectionStringName is null or consists only of white-space characters.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the specified connection string is not found in the configuration.</exception>
        public IServiceCollection UseEntityFrameworkCoreSqlServer<TContext>(
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
            _ = AddSqlServerDbContextCore(services, conString, contextFactory, serviceLifetime);
            _ = (configureIdentity(services) ?? throw new InvalidOperationException("The identity configuration delegate must return an IdentityBuilder."))
                .AddEntityFrameworkStores<TContext>();
            return services;
        }

        /// <summary>Configures Entity Framework Core with SQL Server for the specified DbContext without ASP.NET Core Identity.</summary>
        /// <remarks>Use this method when you need Entity Framework Core with SQL Server but do not require
        /// ASP.NET Core Identity services. This is useful for applications that handle authentication externally or
        /// do not need user management.</remarks>
        /// <typeparam name="TContext">The type of the Entity Framework Core DbContext to use for data access.</typeparam>
        /// <param name="configuration">The configuration instance containing the connection string. Cannot be null.</param>
        /// <param name="connectionStringName">The name of the connection string in the configuration. Cannot be null or whitespace.</param>
        /// <param name="contextFactory">Creates the context instance from the service provider and typed context options.</param>
        /// <returns>The same IServiceCollection instance so that additional calls can be chained.</returns>
        public IServiceCollection AddSqlServerDbContext<TContext>(
            IConfiguration configuration,
            string connectionStringName,
            Func<IServiceProvider, DbContextOptions<TContext>, TContext> contextFactory)
            where TContext : DbContext =>
            services.AddSqlServerDbContext(
                configuration,
                connectionStringName,
                contextFactory,
                ServiceLifetime.Scoped);

        /// <summary>Configures a SQL Server DbContext with an explicit service lifetime.</summary>
        /// <typeparam name="TContext">The Entity Framework Core DbContext type.</typeparam>
        /// <param name="configuration">The application configuration.</param>
        /// <param name="connectionStringName">The configured connection string name.</param>
        /// <param name="contextFactory">Creates the context instance from the service provider and typed context options.</param>
        /// <param name="serviceLifetime">The lifetime with which to register the DbContext service.</param>
        /// <returns>The same IServiceCollection instance so that additional calls can be chained.</returns>
        /// <exception cref="ArgumentException">Thrown if connectionStringName is null or consists only of white-space characters.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the specified connection string is not found in the configuration.</exception>
        public IServiceCollection AddSqlServerDbContext<TContext>(
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
            _ = AddSqlServerDbContextCore(services, conString, contextFactory, serviceLifetime);
            return services;
        }

        /// <summary>Configures Entity Framework Core with SQL Server for the specified DbContext using a direct connection string.</summary>
        /// <remarks>Use this overload when you have a connection string available directly rather than from
        /// configuration. This is useful for testing scenarios or when connection strings are obtained from
        /// other sources such as environment variables or secret managers.</remarks>
        /// <typeparam name="TContext">The type of the Entity Framework Core DbContext to use for data access.</typeparam>
        /// <param name="connectionString">The SQL Server connection string. Cannot be null or whitespace.</param>
        /// <param name="contextFactory">Creates the context instance from the service provider and typed context options.</param>
        /// <returns>The same IServiceCollection instance so that additional calls can be chained.</returns>
        public IServiceCollection AddSqlServerDbContextWithConnectionString<TContext>(
            string connectionString,
            Func<IServiceProvider, DbContextOptions<TContext>, TContext> contextFactory)
            where TContext : DbContext =>
            services.AddSqlServerDbContextWithConnectionString(
                connectionString,
                contextFactory,
                ServiceLifetime.Scoped);

        /// <summary>Configures a SQL Server DbContext from a connection string with an explicit lifetime.</summary>
        /// <typeparam name="TContext">The Entity Framework Core DbContext type.</typeparam>
        /// <param name="connectionString">The SQL Server connection string.</param>
        /// <param name="contextFactory">Creates the context instance from the service provider and typed context options.</param>
        /// <param name="serviceLifetime">The lifetime with which to register the DbContext service.</param>
        /// <returns>The same IServiceCollection instance so that additional calls can be chained.</returns>
        /// <exception cref="ArgumentException">Thrown if connectionString is null or consists only of white-space characters.</exception>
        public IServiceCollection AddSqlServerDbContextWithConnectionString<TContext>(
            string connectionString,
            Func<IServiceProvider, DbContextOptions<TContext>, TContext> contextFactory,
            ServiceLifetime serviceLifetime)
            where TContext : DbContext
        {
            _ = services ?? throw new ArgumentNullException(nameof(services));
            _ = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));

            ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

            _ = AddSqlServerDbContextCore(services, connectionString, contextFactory, serviceLifetime);
            return services;
        }
    }

    /// <summary>Registers SQL Server options and a typed context factory without overriding existing context registrations.</summary>
    /// <typeparam name="TContext">The Entity Framework Core DbContext type.</typeparam>
    /// <param name="services">The services to configure.</param>
    /// <param name="connectionString">The SQL Server connection string.</param>
    /// <param name="contextFactory">Creates the context instance from the service provider and typed context options.</param>
    /// <param name="serviceLifetime">The lifetime with which to register the DbContext service.</param>
    /// <returns>The configured service collection.</returns>
    private static IServiceCollection AddSqlServerDbContextCore<TContext>(
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
            options => options.UseSqlServer(connectionString),
            serviceLifetime);
        return services;
    }
}
