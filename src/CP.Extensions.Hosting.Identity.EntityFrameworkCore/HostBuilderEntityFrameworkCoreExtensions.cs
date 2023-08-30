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
    /// Uses the entity framework core.
    /// </summary>
    /// <typeparam name="TContext">The type of the context.</typeparam>
    /// <param name="hostBuilder">The host builder.</param>
    /// <param name="connectionStringName">The connection string Name.</param>
    /// <param name="configureDatabase">An optional action to configure the <see cref="DbContextOptions" /> for the context. This provides an
    /// alternative to performing configuration of the context by overriding the
    /// <see cref="DbContext.OnConfiguring" /> method in your derived context.</param>
    /// <param name="validateScopes"><c>true</c> to perform check verifying that scoped services never gets resolved from root provider; otherwise <c>false</c>. Defaults to <c>false</c>.</param>
    /// <returns>
    /// IHostBuilder.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">hostBuilder.</exception>
    public static IHostBuilder UseEntityFrameworkCore<TContext>(this IHostBuilder hostBuilder, string? connectionStringName, Action<WebHostBuilderContext, IServiceCollection, DbContextOptionsBuilder, string> configureDatabase, bool validateScopes = false)
        where TContext : DbContext
    {
        if (hostBuilder is null)
        {
            throw new ArgumentNullException(nameof(hostBuilder));
        }

        return hostBuilder.UseWebHostServices(
            (context, services) =>
            {
                var constring = context.Configuration.GetConnectionString(connectionStringName!) ?? throw new InvalidOperationException($"Connection string '{connectionStringName}' not found.");
                services.AddDbContext<TContext>(options => configureDatabase(context, services, options, constring));
            },
            validateScopes);
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
        if (hostBuilder is null)
        {
            throw new ArgumentNullException(nameof(hostBuilder));
        }

        return hostBuilder.ConfigureWebHostDefaults(webBuilder =>
                      webBuilder.UseDefaultServiceProvider(options => options.ValidateScopes = validateScopes)
                         .Configure(app => app.Run(async (_) => await Task.CompletedTask)) // Dummy app.Run to prevent 'No application service provider was found' error.
                         .ConfigureServices((context, services) => configureServices(context, services)));
    }
}
