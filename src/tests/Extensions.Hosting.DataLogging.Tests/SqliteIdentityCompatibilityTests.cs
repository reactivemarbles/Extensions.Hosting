// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

extern alias Sqlite;
using SqliteExtensions = Sqlite::ReactiveMarbles.Extensions.Hosting.Identity.EntityFrameworkCore.HostBuilderEntityFrameworkCoreExtensions;

namespace Extensions.Hosting.DataLogging.Tests;

/// <summary>Verifies the SQLite Identity overloads shipped in version 4.1.0.</summary>
public sealed class SqliteIdentityCompatibilityTests
{
    /// <summary>The configured connection string name.</summary>
    private const string ConnectionStringName = "Identity";

    /// <summary>The provider connection string used for registration.</summary>
    private const string ConnectionString = "Data Source=:memory:";

    /// <summary>Verifies application-builder overloads register custom Identity types and context lifetimes.</summary>
    /// <param name="roles">Whether to configure custom roles.</param>
    /// <param name="defaultLifetime">Whether to use the overload with the default lifetime.</param>
    /// <param name="lifetime">The expected context lifetime.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    [Arguments(false, true, ServiceLifetime.Scoped)]
    [Arguments(true, true, ServiceLifetime.Scoped)]
    [Arguments(false, false, ServiceLifetime.Scoped)]
    [Arguments(true, false, ServiceLifetime.Scoped)]
    [Arguments(false, false, ServiceLifetime.Singleton)]
    [Arguments(true, false, ServiceLifetime.Singleton)]
    [Arguments(false, false, ServiceLifetime.Transient)]
    [Arguments(true, false, ServiceLifetime.Transient)]
    public async Task AddSqliteWithIdentity_CustomTypes_RegisterManagersAndLifetime(bool roles, bool defaultLifetime, ServiceLifetime lifetime)
    {
        var builder = Host.CreateApplicationBuilder();
        _ = builder.Configuration.AddInMemoryCollection(
        [
            new KeyValuePair<string, string?>("ConnectionStrings:Identity", ConnectionString),
        ]);

        var result = (roles, defaultLifetime) switch
        {
            (true, true) => SqliteExtensions.AddSqliteWithIdentity<RoleContext, CustomUser, CustomRole>(builder, ConnectionStringName),
            (true, false) => SqliteExtensions.AddSqliteWithIdentity<RoleContext, CustomUser, CustomRole>(builder, ConnectionStringName, lifetime),
            (false, true) => SqliteExtensions.AddSqliteWithIdentity<UserContext, CustomUser>(builder, ConnectionStringName),
            (false, false) => SqliteExtensions.AddSqliteWithIdentity<UserContext, CustomUser>(builder, ConnectionStringName, lifetime),
        };

        await Assert.That(ReferenceEquals(result, builder)).IsTrue();
        await VerifyRegistrationAsync(builder.Services, roles, lifetime);
    }

    /// <summary>Verifies service-collection overloads register custom Identity types and context lifetimes.</summary>
    /// <param name="roles">Whether to configure custom roles.</param>
    /// <param name="defaultLifetime">Whether to use the overload with the default lifetime.</param>
    /// <param name="lifetime">The expected context lifetime.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    [Arguments(false, true, ServiceLifetime.Scoped)]
    [Arguments(true, true, ServiceLifetime.Scoped)]
    [Arguments(false, false, ServiceLifetime.Scoped)]
    [Arguments(true, false, ServiceLifetime.Scoped)]
    [Arguments(false, false, ServiceLifetime.Singleton)]
    [Arguments(true, false, ServiceLifetime.Singleton)]
    [Arguments(false, false, ServiceLifetime.Transient)]
    [Arguments(true, false, ServiceLifetime.Transient)]
    public async Task UseEntityFrameworkCoreSqlite_CustomTypes_RegisterManagersAndLifetime(bool roles, bool defaultLifetime, ServiceLifetime lifetime)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(
        [
            new KeyValuePair<string, string?>("ConnectionStrings:Identity", ConnectionString),
        ]).Build();
        var context = new WebHostBuilderContext { Configuration = configuration };
        var services = new ServiceCollection();
        _ = services.AddLogging();
        var result = (roles, defaultLifetime) switch
        {
            (true, true) => SqliteExtensions.UseEntityFrameworkCoreSqlite<RoleContext, CustomUser, CustomRole>(services, context, ConnectionStringName),
            (true, false) => SqliteExtensions.UseEntityFrameworkCoreSqlite<RoleContext, CustomUser, CustomRole>(services, context, ConnectionStringName, lifetime),
            (false, true) => SqliteExtensions.UseEntityFrameworkCoreSqlite<UserContext, CustomUser>(services, context, ConnectionStringName),
            (false, false) => SqliteExtensions.UseEntityFrameworkCoreSqlite<UserContext, CustomUser>(services, context, ConnectionStringName, lifetime),
        };

        await Assert.That(ReferenceEquals(result, services)).IsTrue();
        await VerifyRegistrationAsync(services, roles, lifetime);
    }

    /// <summary>Verifies actual context, store, and manager resolution for the selected Identity types.</summary>
    /// <param name="services">The configured services.</param>
    /// <param name="roles">Whether custom role services are expected.</param>
    /// <param name="lifetime">The expected context lifetime.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    private static async Task VerifyRegistrationAsync(IServiceCollection services, bool roles, ServiceLifetime lifetime)
    {
        var contextType = roles ? typeof(RoleContext) : typeof(UserContext);
        ServiceDescriptor? contextDescriptor = null;
        foreach (var descriptor in services)
        {
            if (descriptor.ServiceType == contextType)
            {
                contextDescriptor = descriptor;
                break;
            }
        }

        await Assert.That(contextDescriptor).IsNotNull();
        await Assert.That(contextDescriptor!.Lifetime).IsEqualTo(lifetime);
        await using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        using var firstScope = provider.CreateScope();
        using var secondScope = provider.CreateScope();
        var first = (DbContext)firstScope.ServiceProvider.GetRequiredService(contextType);
        var repeated = (DbContext)firstScope.ServiceProvider.GetRequiredService(contextType);
        var second = (DbContext)secondScope.ServiceProvider.GetRequiredService(contextType);
        await Assert.That(first.Database.ProviderName).IsEqualTo("Microsoft.EntityFrameworkCore.Sqlite");
        await Assert.That(first.Database.GetConnectionString()).IsEqualTo(ConnectionString);
        await Assert.That(ReferenceEquals(first, repeated)).IsEqualTo(lifetime != ServiceLifetime.Transient);
        await Assert.That(ReferenceEquals(first, second)).IsEqualTo(lifetime == ServiceLifetime.Singleton);
        await Assert.That(firstScope.ServiceProvider.GetRequiredService<UserManager<CustomUser>>()).IsNotNull();
        await Assert.That(firstScope.ServiceProvider.GetRequiredService<IUserStore<CustomUser>>()).IsNotNull();
        if (roles)
        {
            await Assert.That(firstScope.ServiceProvider.GetRequiredService<RoleManager<CustomRole>>()).IsNotNull();
            await Assert.That(firstScope.ServiceProvider.GetRequiredService<IRoleStore<CustomRole>>()).IsNotNull();
        }
        else
        {
            await Assert.That(firstScope.ServiceProvider.GetService<RoleManager<CustomRole>>()).IsNull();
        }
    }

    /// <summary>Provides a custom application user.</summary>
    public sealed class CustomUser : IdentityUser;

    /// <summary>Provides a custom application role.</summary>
    public sealed class CustomRole : IdentityRole;

    /// <summary>Provides a context storing custom users without roles.</summary>
    /// <param name="options">The configured context options.</param>
    public sealed class UserContext(DbContextOptions<UserContext> options) : IdentityUserContext<CustomUser>(options);

    /// <summary>Provides a context storing custom users and roles.</summary>
    /// <param name="options">The configured context options.</param>
    public sealed class RoleContext(DbContextOptions<RoleContext> options) : IdentityDbContext<CustomUser, CustomRole, string>(options);
}
