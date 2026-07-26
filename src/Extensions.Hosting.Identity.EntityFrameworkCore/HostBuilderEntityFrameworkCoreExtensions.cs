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
        /// <returns>The same IHostApplicationBuilder instance so that additional calls can be chained.</returns>
        public IHostApplicationBuilder AddSqlServerDbContext<TContext>(string connectionStringName)
            where TContext : DbContext =>
            builder.AddSqlServerDbContext<TContext>(connectionStringName, ServiceLifetime.Scoped);

        /// <summary>Configures Entity Framework Core with SQL Server and an explicit service lifetime.</summary>
        /// <typeparam name="TContext">The Entity Framework Core DbContext type.</typeparam>
        /// <param name="connectionStringName">The configured connection string name.</param>
        /// <param name="serviceLifetime">The lifetime with which to register the DbContext service.</param>
        /// <returns>The same IHostApplicationBuilder instance so that additional calls can be chained.</returns>
        /// <exception cref="ArgumentException">Thrown if connectionStringName is null or consists only of white-space characters.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the specified connection string is not found in the configuration.</exception>
        public IHostApplicationBuilder AddSqlServerDbContext<TContext>(
            string connectionStringName,
            ServiceLifetime serviceLifetime)
            where TContext : DbContext
        {
            _ = builder ?? throw new ArgumentNullException(nameof(builder));

            ArgumentException.ThrowIfNullOrWhiteSpace(connectionStringName);

            var conString = builder.Configuration.GetConnectionString(connectionStringName);
            _ = builder.Services.AddDbContext<TContext>(
                options => options.UseSqlServer(conString ?? throw new InvalidOperationException($"Connection string '{connectionStringName}' not found.")),
                serviceLifetime);
            return builder;
        }

        /// <summary>Configures Entity Framework Core with SQL Server and ASP.NET Core Identity using IHostApplicationBuilder.</summary>
        /// <remarks>This method provides integration with the modern IHostApplicationBuilder pattern and
        /// sets up both Entity Framework Core and ASP.NET Core Identity with the specified user and role types.</remarks>
        /// <typeparam name="TContext">The type of the Entity Framework Core DbContext to use for data access.</typeparam>
        /// <typeparam name="TUser">The type representing application users for ASP.NET Core Identity.</typeparam>
        /// <typeparam name="TRole">The type representing application roles for ASP.NET Core Identity.</typeparam>
        /// <param name="connectionStringName">The name of the connection string in the configuration. Cannot be null or whitespace.</param>
        /// <returns>The same IHostApplicationBuilder instance so that additional calls can be chained.</returns>
        public IHostApplicationBuilder AddSqlServerWithIdentity<TContext, TUser, TRole>(string connectionStringName)
            where TContext : DbContext
            where TUser : class
            where TRole : class =>
            builder.AddSqlServerWithIdentity<TContext, TUser, TRole>(connectionStringName, ServiceLifetime.Scoped);

        /// <summary>Configures SQL Server and Identity with an explicit service lifetime.</summary>
        /// <typeparam name="TContext">The Entity Framework Core DbContext type.</typeparam>
        /// <typeparam name="TUser">The Identity user type.</typeparam>
        /// <typeparam name="TRole">The Identity role type.</typeparam>
        /// <param name="connectionStringName">The configured connection string name.</param>
        /// <param name="serviceLifetime">The lifetime with which to register the DbContext service.</param>
        /// <returns>The same IHostApplicationBuilder instance so that additional calls can be chained.</returns>
        /// <exception cref="ArgumentException">Thrown if connectionStringName is null or consists only of white-space characters.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the specified connection string is not found in the configuration.</exception>
        public IHostApplicationBuilder AddSqlServerWithIdentity<TContext, TUser, TRole>(
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
                    options => options.UseSqlServer(conString ?? throw new InvalidOperationException($"Connection string '{connectionStringName}' not found.")),
                    serviceLifetime)
                .AddDefaultIdentity<TUser>()
                .AddRoles<TRole>()
                .AddEntityFrameworkStores<TContext>();
            return builder;
        }

        /// <summary>Configures Entity Framework Core with SQL Server and ASP.NET Core Identity (user only) using IHostApplicationBuilder.</summary>
        /// <remarks>This method provides integration with the modern IHostApplicationBuilder pattern and
        /// sets up both Entity Framework Core and ASP.NET Core Identity with the specified user type only.</remarks>
        /// <typeparam name="TContext">The type of the Entity Framework Core DbContext to use for data access.</typeparam>
        /// <typeparam name="TUser">The type representing application users for ASP.NET Core Identity.</typeparam>
        /// <param name="connectionStringName">The name of the connection string in the configuration. Cannot be null or whitespace.</param>
        /// <returns>The same IHostApplicationBuilder instance so that additional calls can be chained.</returns>
        public IHostApplicationBuilder AddSqlServerWithIdentity<TContext, TUser>(string connectionStringName)
            where TContext : DbContext
            where TUser : class =>
            builder.AddSqlServerWithIdentity<TContext, TUser>(connectionStringName, ServiceLifetime.Scoped);

        /// <summary>Configures SQL Server and user-only Identity with an explicit service lifetime.</summary>
        /// <typeparam name="TContext">The Entity Framework Core DbContext type.</typeparam>
        /// <typeparam name="TUser">The Identity user type.</typeparam>
        /// <param name="connectionStringName">The configured connection string name.</param>
        /// <param name="serviceLifetime">The lifetime with which to register the DbContext service.</param>
        /// <returns>The same IHostApplicationBuilder instance so that additional calls can be chained.</returns>
        /// <exception cref="ArgumentException">Thrown if connectionStringName is null or consists only of white-space characters.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the specified connection string is not found in the configuration.</exception>
        public IHostApplicationBuilder AddSqlServerWithIdentity<TContext, TUser>(
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
                    options => options.UseSqlServer(conString ?? throw new InvalidOperationException($"Connection string '{connectionStringName}' not found.")),
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
        /// <summary>Configures SQL Server and ASP.NET Core Identity with the specified context, user, and role types.</summary>
        /// <remarks>This method sets up Entity Framework Core to use SQL Server as the database provider and
        /// configures ASP.NET Core Identity with the specified user and role types. It is typically called during
        /// application startup to register data access and identity services. The method also adds the Entity Framework
        /// stores for Identity, enabling user and role management backed by the specified DbContext.</remarks>
        /// <typeparam name="TContext">The type of the Entity Framework Core DbContext to use for data access.</typeparam>
        /// <typeparam name="TUser">The type representing application users for ASP.NET Core Identity. Must be a reference type.</typeparam>
        /// <typeparam name="TRole">The type representing application roles for ASP.NET Core Identity. Must be a reference type.</typeparam>
        /// <param name="context">The web host builder context containing configuration and environment information. Cannot be null.</param>
        /// <param name="connectionStringName">The name of the connection string in the application's configuration to use for the SQL Server database. Cannot
        /// be null or whitespace.</param>
        /// <returns>The same IServiceCollection instance so that additional calls can be chained.</returns>
        public IServiceCollection UseEntityFrameworkCoreSqlServer<TContext, TUser, TRole>(
            WebHostBuilderContext context,
            string connectionStringName)
            where TContext : DbContext
            where TUser : class
            where TRole : class =>
            services.UseEntityFrameworkCoreSqlServer<TContext, TUser, TRole>(
                context,
                connectionStringName,
                ServiceLifetime.Scoped);

        /// <summary>Configures SQL Server and Identity with an explicit service lifetime.</summary>
        /// <typeparam name="TContext">The Entity Framework Core DbContext type.</typeparam>
        /// <typeparam name="TUser">The Identity user type.</typeparam>
        /// <typeparam name="TRole">The Identity role type.</typeparam>
        /// <param name="context">The web host builder context.</param>
        /// <param name="connectionStringName">The configured connection string name.</param>
        /// <param name="serviceLifetime">The lifetime with which to register the DbContext service.</param>
        /// <returns>The same IServiceCollection instance so that additional calls can be chained.</returns>
        /// <exception cref="ArgumentException">Thrown if connectionStringName is null or consists only of white-space characters.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the specified connection string is not found in the configuration.</exception>
        public IServiceCollection UseEntityFrameworkCoreSqlServer<TContext, TUser, TRole>(
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
                    options => options.UseSqlServer(conString ?? throw new InvalidOperationException($"Connection string '{connectionStringName}' not found.")),
                    serviceLifetime)
                .AddDefaultIdentity<TUser>()
                .AddRoles<TRole>()
                .AddEntityFrameworkStores<TContext>();
            return services;
        }

        /// <summary>Configures SQL Server with the specified DbContext and Identity user type.</summary>
        /// <remarks>This method adds the DbContext, ASP.NET Core Identity, and Entity Framework stores to the
        /// service collection, enabling authentication and data access using SQL Server. The connection string must be
        /// defined in the application's configuration under the provided name.</remarks>
        /// <typeparam name="TContext">The type of the DbContext to register for use with SQL Server.</typeparam>
        /// <typeparam name="TUser">The type of the user entity to use with ASP.NET Core Identity.</typeparam>
        /// <param name="context">The web host builder context containing configuration information. Cannot be null.</param>
        /// <param name="connectionStringName">The name of the connection string in the configuration to use for the SQL Server database. Cannot be null or
        /// whitespace.</param>
        /// <returns>The IServiceCollection instance configured for SQL Server and Identity.</returns>
        public IServiceCollection UseEntityFrameworkCoreSqlServer<TContext, TUser>(
            WebHostBuilderContext context,
            string connectionStringName)
            where TContext : DbContext
            where TUser : class =>
            services.UseEntityFrameworkCoreSqlServer<TContext, TUser>(
                context,
                connectionStringName,
                ServiceLifetime.Scoped);

        /// <summary>Configures SQL Server and user-only Identity with an explicit service lifetime.</summary>
        /// <typeparam name="TContext">The Entity Framework Core DbContext type.</typeparam>
        /// <typeparam name="TUser">The Identity user type.</typeparam>
        /// <param name="context">The web host builder context.</param>
        /// <param name="connectionStringName">The configured connection string name.</param>
        /// <param name="serviceLifetime">The lifetime with which to register the DbContext service.</param>
        /// <returns>The IServiceCollection instance with Entity Framework Core and identity services configured for SQL Server.</returns>
        /// <exception cref="ArgumentException">Thrown if connectionStringName is null or consists only of white-space characters.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the specified connection string is not found in the configuration.</exception>
        public IServiceCollection UseEntityFrameworkCoreSqlServer<TContext, TUser>(
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
                    options => options.UseSqlServer(conString ?? throw new InvalidOperationException($"Connection string '{connectionStringName}' not found.")),
                    serviceLifetime)
                .AddDefaultIdentity<TUser>()
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
        /// <returns>The same IServiceCollection instance so that additional calls can be chained.</returns>
        public IServiceCollection AddSqlServerDbContext<TContext>(
            IConfiguration configuration,
            string connectionStringName)
            where TContext : DbContext =>
            services.AddSqlServerDbContext<TContext>(
                configuration,
                connectionStringName,
                ServiceLifetime.Scoped);

        /// <summary>Configures a SQL Server DbContext with an explicit service lifetime.</summary>
        /// <typeparam name="TContext">The Entity Framework Core DbContext type.</typeparam>
        /// <param name="configuration">The application configuration.</param>
        /// <param name="connectionStringName">The configured connection string name.</param>
        /// <param name="serviceLifetime">The lifetime with which to register the DbContext service.</param>
        /// <returns>The same IServiceCollection instance so that additional calls can be chained.</returns>
        /// <exception cref="ArgumentException">Thrown if connectionStringName is null or consists only of white-space characters.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the specified connection string is not found in the configuration.</exception>
        public IServiceCollection AddSqlServerDbContext<TContext>(
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
                options => options.UseSqlServer(conString ?? throw new InvalidOperationException($"Connection string '{connectionStringName}' not found.")),
                serviceLifetime);
            return services;
        }

        /// <summary>Configures Entity Framework Core with SQL Server for the specified DbContext using a direct connection string.</summary>
        /// <remarks>Use this overload when you have a connection string available directly rather than from
        /// configuration. This is useful for testing scenarios or when connection strings are obtained from
        /// other sources such as environment variables or secret managers.</remarks>
        /// <typeparam name="TContext">The type of the Entity Framework Core DbContext to use for data access.</typeparam>
        /// <param name="connectionString">The SQL Server connection string. Cannot be null or whitespace.</param>
        /// <returns>The same IServiceCollection instance so that additional calls can be chained.</returns>
        public IServiceCollection AddSqlServerDbContextWithConnectionString<TContext>(string connectionString)
            where TContext : DbContext =>
            services.AddSqlServerDbContextWithConnectionString<TContext>(
                connectionString,
                ServiceLifetime.Scoped);

        /// <summary>Configures a SQL Server DbContext from a connection string with an explicit lifetime.</summary>
        /// <typeparam name="TContext">The Entity Framework Core DbContext type.</typeparam>
        /// <param name="connectionString">The SQL Server connection string.</param>
        /// <param name="serviceLifetime">The lifetime with which to register the DbContext service.</param>
        /// <returns>The same IServiceCollection instance so that additional calls can be chained.</returns>
        /// <exception cref="ArgumentException">Thrown if connectionString is null or consists only of white-space characters.</exception>
        public IServiceCollection AddSqlServerDbContextWithConnectionString<TContext>(
            string connectionString,
            ServiceLifetime serviceLifetime)
            where TContext : DbContext
        {
            _ = services ?? throw new ArgumentNullException(nameof(services));

            ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

            _ = services.AddDbContext<TContext>(
                options => options.UseSqlServer(connectionString),
                serviceLifetime);
            return services;
        }
    }
}
