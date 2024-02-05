// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CP.Extensions.Hosting.Identity.EntityFrameworkCore;

/// <summary>
/// Host Builder Entity Framework Core Extensions.
/// </summary>
public static class HostBuilderEntityFrameworkCoreExtensions
{
    /// <summary>
    /// Uses the entity framework core with Sqlite.
    /// </summary>
    /// <typeparam name="TContext">The type of the context.</typeparam>
    /// <typeparam name="TUser">The type of the user.</typeparam>
    /// <typeparam name="TRole">The type of the role.</typeparam>
    /// <param name="services">The services.</param>
    /// <param name="context">The context.</param>
    /// <param name="connectionStringName">Name of the connection string.</param>
    /// <param name="serviceLifetime">The service lifetime.</param>
    /// <returns>
    /// IServiceCollection.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">
    /// services
    /// or
    /// context.
    /// </exception>
    /// <exception cref="System.ArgumentException">Value cannot be null or whitespace. - connectionStringName.</exception>
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
    /// Uses the entity framework core with Sqlite.
    /// </summary>
    /// <typeparam name="TContext">The type of the context.</typeparam>
    /// <typeparam name="TUser">The type of the user.</typeparam>
    /// <param name="services">The services.</param>
    /// <param name="context">The context.</param>
    /// <param name="connectionStringName">Name of the connection string.</param>
    /// <param name="serviceLifetime">The service lifetime.</param>
    /// <returns>
    /// IServiceCollection.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">
    /// services
    /// or
    /// context.
    /// </exception>
    /// <exception cref="System.ArgumentException">Value cannot be null or whitespace. - connectionStringName.</exception>
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
    /// Uses the web host services.
    /// </summary>
    /// <param name="hostBuilder">The host builder.</param>
    /// <param name="configureServices">Adds a delegate for configuring additional services for the host or web application.</param>
    /// <param name="validateScopes"><c>true</c> to perform check verifying that scoped services never gets resolved from root provider; otherwise <c>false</c>. Defaults to <c>false</c>.</param>
    /// <returns>IHostBuilder.</returns>
    /// <exception cref="System.ArgumentNullException">hostBuilder.</exception>
    public static IHostBuilder UseWebHostServices(this IHostBuilder hostBuilder, Action<WebHostBuilderContext, IServiceCollection> configureServices, bool validateScopes = false)
    {
        ArgumentNullException.ThrowIfNull(hostBuilder);

        return hostBuilder.ConfigureWebHostDefaults(webBuilder =>
                      webBuilder.UseDefaultServiceProvider(options => options.ValidateScopes = validateScopes)
                         .Configure(app => app.Run(async (_) => await Task.CompletedTask)) // Dummy app.Run to prevent 'No application service provider was found' error.
                         .ConfigureServices((context, services) => configureServices(context, services)));
    }
}
