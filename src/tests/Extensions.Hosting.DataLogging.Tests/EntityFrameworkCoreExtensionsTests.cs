// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

extern alias Sqlite;
extern alias SqlServer;
using SqlServerExtensions = SqlServer::ReactiveMarbles.Extensions.Hosting.Identity.EntityFrameworkCore.HostBuilderEntityFrameworkCoreExtensions;
using SqliteExtensions = Sqlite::ReactiveMarbles.Extensions.Hosting.Identity.EntityFrameworkCore.HostBuilderEntityFrameworkCoreExtensions;

namespace Extensions.Hosting.DataLogging.Tests;

/// <summary>Contains Entity Framework Core extension coverage tests.</summary>
public class EntityFrameworkCoreExtensionsTests
{
    /// <summary>Names the configured connection string.</summary>
    private const string DatabaseConnectionName = "Database";

    /// <summary>Names an absent connection string.</summary>
    private const string MissingConnectionName = "Missing";

    /// <summary>Provides an in-memory SQLite connection string.</summary>
    private const string SqliteMemoryConnectionString = "Data Source=:memory:";

    /// <summary>Provides a non-secret SQL Server connection-string value for options registration tests.</summary>
    private const string SqlServerConnectionString = "Server=(localdb)\\MSSQLLocalDB;Database=ExtensionHostingTests;Trusted_Connection=True;";

    /// <summary>Names the SQL Server Entity Framework Core provider.</summary>
    private const string SqlServerProviderName = "Microsoft.EntityFrameworkCore.SqlServer";

    /// <summary>Names the SQLite Entity Framework Core provider.</summary>
    private const string SqliteProviderName = "Microsoft.EntityFrameworkCore.Sqlite";

    /// <summary>Provides an ephemeral loopback URL for web-host tests.</summary>
    private const string EphemeralWebHostUrl = "http://127.0.0.1:0";

    /// <summary>Provides requests to the ephemeral web hosts.</summary>
    private static readonly HttpClient _httpClient = new();

    /// <summary>Verifies SQL Server application-builder registrations.</summary>
    /// <returns>A task representing the test.</returns>
    [Test]
    public async Task SqlServerConfigurationExtensions_ValidateAndRetrieveConnectionStrings()
    {
        var configuration = CreateConfiguration();

        await Assert.That(SqlServerExtensions.HasConnectionString(configuration, DatabaseConnectionName)).IsTrue();
        await Assert.That(SqlServerExtensions.HasConnectionString(configuration, MissingConnectionName)).IsFalse();
        await Assert.That(SqlServerExtensions.HasConnectionString(configuration, " ")).IsFalse();
        await Assert.That(SqlServerExtensions.GetRequiredConnectionString(configuration, DatabaseConnectionName)).IsEqualTo(SqlServerConnectionString);

        var missing = () => SqlServerExtensions.GetRequiredConnectionString(configuration, MissingConnectionName);
        var whitespace = () => SqlServerExtensions.GetRequiredConnectionString(configuration, " ");
        await Assert.That(missing).Throws<InvalidOperationException>();
        await Assert.That(whitespace).Throws<ArgumentException>();
    }

    /// <summary>Verifies SQL Server application-builder validation.</summary>
    /// <returns>A task representing the test.</returns>
    [Test]
    public async Task SqlServerApplicationBuilderRegistrations_RegisterAllOverloads()
    {
        var databaseBuilder = CreateApplicationBuilder();
        var databaseResult = SqlServerExtensions.AddSqlServerDbContext<SqlServerContext>(databaseBuilder, DatabaseConnectionName);
        await Assert.That(databaseResult).IsEqualTo(databaseBuilder);
        await Assert.That(ContainsService<SqlServerContext>(databaseBuilder.Services)).IsTrue();
        await Assert.That(GetProviderName<SqlServerContext>(databaseBuilder.Services)).IsEqualTo(SqlServerProviderName);

        var databaseLifetimeBuilder = CreateApplicationBuilder();
        var databaseLifetimeResult = SqlServerExtensions.AddSqlServerDbContext<SqlServerContext>(databaseLifetimeBuilder, DatabaseConnectionName, ServiceLifetime.Singleton);
        await Assert.That(databaseLifetimeResult).IsEqualTo(databaseLifetimeBuilder);

        var userIdentityBuilder = CreateApplicationBuilder();
        var userIdentityResult = SqlServerExtensions.AddSqlServerWithIdentity<SqlServerIdentityContext, TestUser>(userIdentityBuilder, DatabaseConnectionName);
        await Assert.That(userIdentityResult).IsEqualTo(userIdentityBuilder);
        await Assert.That(ContainsService<SqlServerIdentityContext>(userIdentityBuilder.Services)).IsTrue();
        await Assert.That(GetProviderName<SqlServerIdentityContext>(userIdentityBuilder.Services)).IsEqualTo(SqlServerProviderName);

        var userIdentityLifetimeBuilder = CreateApplicationBuilder();
        var userIdentityLifetimeResult = SqlServerExtensions.AddSqlServerWithIdentity<SqlServerIdentityContext, TestUser>(
            userIdentityLifetimeBuilder,
            DatabaseConnectionName,
            ServiceLifetime.Singleton);
        await Assert.That(userIdentityLifetimeResult).IsEqualTo(userIdentityLifetimeBuilder);

        var roleIdentityBuilder = CreateApplicationBuilder();
        var roleIdentityResult = SqlServerExtensions.AddSqlServerWithIdentity<SqlServerIdentityContext, TestUser, TestRole>(roleIdentityBuilder, DatabaseConnectionName);
        await Assert.That(roleIdentityResult).IsEqualTo(roleIdentityBuilder);
        await Assert.That(ContainsService<SqlServerIdentityContext>(roleIdentityBuilder.Services)).IsTrue();
        await Assert.That(GetProviderName<SqlServerIdentityContext>(roleIdentityBuilder.Services)).IsEqualTo(SqlServerProviderName);

        var roleIdentityLifetimeBuilder = CreateApplicationBuilder();
        var roleIdentityLifetimeResult = SqlServerExtensions.AddSqlServerWithIdentity<SqlServerIdentityContext, TestUser, TestRole>(
            roleIdentityLifetimeBuilder,
            DatabaseConnectionName,
            ServiceLifetime.Singleton);
        await Assert.That(roleIdentityLifetimeResult).IsEqualTo(roleIdentityLifetimeBuilder);
    }

    /// <summary>Verifies SQL Server service-collection registrations.</summary>
    /// <returns>A task representing the test.</returns>
    [Test]
    public async Task SqlServerApplicationBuilderRegistrations_ValidateArgumentsAndMissingConnections()
    {
        IHostApplicationBuilder? nullBuilder = null;
        var nullBuilderAction = () => SqlServerExtensions.AddSqlServerDbContext<SqlServerContext>(nullBuilder!, DatabaseConnectionName, ServiceLifetime.Scoped);
        await Assert.That(nullBuilderAction).Throws<ArgumentNullException>();

        var builder = CreateApplicationBuilder();
        var whitespace = () => SqlServerExtensions.AddSqlServerDbContext<SqlServerContext>(builder, " ", ServiceLifetime.Scoped);
        await Assert.That(whitespace).Throws<ArgumentException>();

        var missingBuilder = Host.CreateApplicationBuilder();
        _ = SqlServerExtensions.AddSqlServerDbContext<SqlServerContext>(missingBuilder, MissingConnectionName, ServiceLifetime.Scoped);
        await using var provider = missingBuilder.Services.BuildServiceProvider();
        var missing = () => provider.GetRequiredService<SqlServerContext>();
        await Assert.That(missing).Throws<InvalidOperationException>();
    }

    /// <summary>Verifies SQL Server service-collection validation.</summary>
    /// <returns>A task representing the test.</returns>
    [Test]
    public async Task SqlServerServiceCollectionRegistrations_RegisterAllOverloadsAndProviders()
    {
        var configuration = CreateConfiguration();
        var context = new WebHostBuilderContext { Configuration = configuration };

        var databaseServices = new ServiceCollection();
        var databaseResult = SqlServerExtensions.AddSqlServerDbContext<SqlServerContext>(databaseServices, configuration, DatabaseConnectionName);
        await Assert.That(databaseResult).IsEqualTo(databaseServices);
        await Assert.That(GetProviderName<SqlServerContext>(databaseServices)).IsEqualTo(SqlServerProviderName);

        var databaseLifetimeServices = new ServiceCollection();
        var databaseLifetimeResult = SqlServerExtensions.AddSqlServerDbContext<SqlServerContext>(databaseLifetimeServices, configuration, DatabaseConnectionName, ServiceLifetime.Singleton);
        await Assert.That(databaseLifetimeResult).IsEqualTo(databaseLifetimeServices);

        var connectionStringServices = new ServiceCollection();
        var connectionStringResult = SqlServerExtensions.AddSqlServerDbContextWithConnectionString<SqlServerContext>(connectionStringServices, SqlServerConnectionString);
        await Assert.That(connectionStringResult).IsEqualTo(connectionStringServices);
        await Assert.That(GetProviderName<SqlServerContext>(connectionStringServices)).IsEqualTo(SqlServerProviderName);

        var connectionStringLifetimeServices = new ServiceCollection();
        var connectionStringLifetimeResult = SqlServerExtensions.AddSqlServerDbContextWithConnectionString<SqlServerContext>(
            connectionStringLifetimeServices,
            SqlServerConnectionString,
            ServiceLifetime.Singleton);
        await Assert.That(connectionStringLifetimeResult).IsEqualTo(connectionStringLifetimeServices);

        var userServices = new ServiceCollection();
        var userResult = SqlServerExtensions.UseEntityFrameworkCoreSqlServer<SqlServerIdentityContext, TestUser>(userServices, context, DatabaseConnectionName);
        await Assert.That(userResult).IsEqualTo(userServices);
        await Assert.That(ContainsService<SqlServerIdentityContext>(userServices)).IsTrue();

        var userLifetimeServices = new ServiceCollection();
        var userLifetimeResult = SqlServerExtensions.UseEntityFrameworkCoreSqlServer<SqlServerIdentityContext, TestUser>(
            userLifetimeServices,
            context,
            DatabaseConnectionName,
            ServiceLifetime.Singleton);
        await Assert.That(userLifetimeResult).IsEqualTo(userLifetimeServices);

        var roleServices = new ServiceCollection();
        var roleResult = SqlServerExtensions.UseEntityFrameworkCoreSqlServer<SqlServerIdentityContext, TestUser, TestRole>(roleServices, context, DatabaseConnectionName);
        await Assert.That(roleResult).IsEqualTo(roleServices);
        await Assert.That(ContainsService<SqlServerIdentityContext>(roleServices)).IsTrue();

        var roleLifetimeServices = new ServiceCollection();
        var roleLifetimeResult = SqlServerExtensions.UseEntityFrameworkCoreSqlServer<SqlServerIdentityContext, TestUser, TestRole>(
            roleLifetimeServices,
            context,
            DatabaseConnectionName,
            ServiceLifetime.Singleton);
        await Assert.That(roleLifetimeResult).IsEqualTo(roleLifetimeServices);
        await Assert.That(GetProviderName<SqlServerIdentityContext>(userServices)).IsEqualTo(SqlServerProviderName);
        await Assert.That(GetProviderName<SqlServerIdentityContext>(roleServices)).IsEqualTo(SqlServerProviderName);
    }

    /// <summary>Verifies SQLite application-builder and service registrations.</summary>
    /// <returns>A task representing the test.</returns>
    [Test]
    public async Task SqlServerServiceCollectionRegistrations_ValidateArguments()
    {
        var configuration = CreateConfiguration();
        var context = new WebHostBuilderContext { Configuration = configuration };
        IServiceCollection? nullServices = null;
        var nullServicesAction = () => SqlServerExtensions.AddSqlServerDbContext<SqlServerContext>(nullServices!, configuration, DatabaseConnectionName, ServiceLifetime.Scoped);
        await Assert.That(nullServicesAction).Throws<ArgumentNullException>();

        var services = new ServiceCollection();
        IConfiguration? nullConfiguration = null;
        var nullConfigurationAction = () => SqlServerExtensions.AddSqlServerDbContext<SqlServerContext>(services, nullConfiguration!, DatabaseConnectionName, ServiceLifetime.Scoped);
        var nullContextAction = () => SqlServerExtensions.UseEntityFrameworkCoreSqlServer<SqlServerIdentityContext, TestUser>(services, null!, DatabaseConnectionName, ServiceLifetime.Scoped);
        var whitespace = () => SqlServerExtensions.AddSqlServerDbContextWithConnectionString<SqlServerContext>(services, " ", ServiceLifetime.Scoped);
        await Assert.That(nullConfigurationAction).Throws<ArgumentNullException>();
        await Assert.That(nullContextAction).Throws<ArgumentNullException>();
        await Assert.That(whitespace).Throws<ArgumentException>();

        var missingServices = new ServiceCollection();
        _ = SqlServerExtensions.AddSqlServerDbContext<SqlServerContext>(missingServices, configuration, MissingConnectionName, ServiceLifetime.Scoped);
        await using var provider = missingServices.BuildServiceProvider();
        var missing = () => provider.GetRequiredService<SqlServerContext>();
        await Assert.That(missing).Throws<InvalidOperationException>();

        await Assert.That(context.Configuration).IsEqualTo(configuration);
    }

    /// <summary>Verifies SQLite helpers and validation behavior.</summary>
    /// <returns>A task representing the test.</returns>
    [Test]
    public async Task SqliteApplicationBuilderAndServiceCollectionRegistrations_RegisterAllOverloads()
    {
        var builder = CreateApplicationBuilder();
        await Assert.That(SqliteExtensions.AddSqliteDbContext<SqliteContext>(builder, DatabaseConnectionName)).IsEqualTo(builder);
        await Assert.That(GetProviderName<SqliteContext>(builder.Services)).IsEqualTo(SqliteProviderName);
        await Assert.That(SqliteExtensions.AddSqliteDbContext<SqliteContext>(builder, DatabaseConnectionName, ServiceLifetime.Singleton)).IsEqualTo(builder);

        var userBuilder = CreateApplicationBuilder();
        await Assert.That(SqliteExtensions.AddSqliteWithIdentity<SqliteIdentityContext, TestUser>(userBuilder, DatabaseConnectionName)).IsEqualTo(userBuilder);
        await Assert.That(GetProviderName<SqliteIdentityContext>(userBuilder.Services)).IsEqualTo(SqliteProviderName);
        await Assert.That(SqliteExtensions.AddSqliteWithIdentity<SqliteIdentityContext, TestUser>(userBuilder, DatabaseConnectionName, ServiceLifetime.Singleton)).IsEqualTo(userBuilder);

        var roleBuilder = CreateApplicationBuilder();
        await Assert.That(SqliteExtensions.AddSqliteWithIdentity<SqliteIdentityContext, TestUser, TestRole>(roleBuilder, DatabaseConnectionName)).IsEqualTo(roleBuilder);
        await Assert.That(GetProviderName<SqliteIdentityContext>(roleBuilder.Services)).IsEqualTo(SqliteProviderName);
        await Assert.That(SqliteExtensions.AddSqliteWithIdentity<SqliteIdentityContext, TestUser, TestRole>(roleBuilder, DatabaseConnectionName, ServiceLifetime.Singleton)).IsEqualTo(roleBuilder);

        var configuration = CreateConfiguration();
        var context = new WebHostBuilderContext { Configuration = configuration };
        var services = new ServiceCollection();
        await Assert.That(SqliteExtensions.AddSqliteDbContext<SqliteContext>(services, configuration, DatabaseConnectionName)).IsEqualTo(services);
        await Assert.That(GetProviderName<SqliteContext>(services)).IsEqualTo(SqliteProviderName);
        await Assert.That(SqliteExtensions.AddSqliteDbContext<SqliteContext>(services, configuration, DatabaseConnectionName, ServiceLifetime.Singleton)).IsEqualTo(services);
        await Assert.That(SqliteExtensions.AddSqliteDbContextWithConnectionString<SqliteContext>(services, SqliteMemoryConnectionString)).IsEqualTo(services);
        await Assert.That(SqliteExtensions.AddSqliteDbContextWithConnectionString<SqliteContext>(services, SqliteMemoryConnectionString, ServiceLifetime.Singleton)).IsEqualTo(services);
        await Assert.That(SqliteExtensions.UseEntityFrameworkCoreSqlite<SqliteIdentityContext, TestUser>(services, context, DatabaseConnectionName)).IsEqualTo(services);
        await Assert.That(SqliteExtensions.UseEntityFrameworkCoreSqlite<SqliteIdentityContext, TestUser>(services, context, DatabaseConnectionName, ServiceLifetime.Singleton)).IsEqualTo(services);
        await Assert.That(SqliteExtensions.UseEntityFrameworkCoreSqlite<SqliteIdentityContext, TestUser, TestRole>(services, context, DatabaseConnectionName)).IsEqualTo(services);
        await Assert.That(SqliteExtensions.UseEntityFrameworkCoreSqlite<SqliteIdentityContext, TestUser, TestRole>(
            services,
            context,
            DatabaseConnectionName,
            ServiceLifetime.Singleton)).IsEqualTo(services);
        await Assert.That(GetProviderName<SqliteContext>(services)).IsEqualTo(SqliteProviderName);
        await Assert.That(GetProviderName<SqliteIdentityContext>(services)).IsEqualTo(SqliteProviderName);
    }

    /// <summary>Verifies SQL Server web-host service overloads.</summary>
    /// <returns>A task representing the test.</returns>
    [Test]
    public async Task SqliteExtensions_CreateConnectionStringsAndValidateArguments()
    {
        var memoryConnection = SqliteExtensions.CreateInMemoryConnectionString();
        var namedMemoryConnection = SqliteExtensions.CreateInMemoryConnectionString("database");
        var nullMemoryConnection = SqliteExtensions.CreateInMemoryConnectionString(null);
        await Assert.That(memoryConnection).IsEqualTo("DataSource=:memory:");
        await Assert.That(namedMemoryConnection).IsEqualTo("DataSource=database;Mode=Memory;Cache=Shared");
        await Assert.That(nullMemoryConnection).IsEqualTo("DataSource=:memory:");
        await Assert.That(SqliteExtensions.CreateFileConnectionString("database.db")).IsEqualTo("DataSource=database.db");

        var builder = CreateApplicationBuilder();
        var whitespace = () => SqliteExtensions.AddSqliteDbContext<SqliteContext>(builder, " ", ServiceLifetime.Scoped);
        await Assert.That(whitespace).Throws<ArgumentException>();

        var services = new ServiceCollection();
        var nullServices = static () => SqliteExtensions.AddSqliteDbContextWithConnectionString<SqliteContext>(null!, SqliteMemoryConnectionString, ServiceLifetime.Scoped);
        var nullConfiguration = () => SqliteExtensions.AddSqliteDbContext<SqliteContext>(services, null!, DatabaseConnectionName, ServiceLifetime.Scoped);
        var emptyConnectionString = () => SqliteExtensions.AddSqliteDbContextWithConnectionString<SqliteContext>(services, " ", ServiceLifetime.Scoped);
        await Assert.That(nullServices).Throws<ArgumentNullException>();
        await Assert.That(nullConfiguration).Throws<ArgumentNullException>();
        await Assert.That(emptyConnectionString).Throws<ArgumentException>();
    }

    /// <summary>Verifies SQLite web-host service overloads.</summary>
    /// <returns>A task representing the test.</returns>
    [Test]
    public async Task WebHostServiceOverloads_ReturnBuildersAndRunConfiguration()
    {
        var serviceConfigured = false;
        var webHostConfigured = false;
        var appConfigured = false;

        var first = CreateWebHostBuilder();
        await Assert.That(SqlServerExtensions.UseWebHostServices(first, (context, services) =>
        {
            _ = context;
            serviceConfigured = true;
            _ = services.AddSingleton(new WebHostMarker());
        })).IsEqualTo(first);
        using var firstHost = first.Build();
        await firstHost.StartAsync();
        await Assert.That(await SendRequestAsync(firstHost)).IsTrue();
        await firstHost.StopAsync();

        var second = CreateWebHostBuilder();
        await Assert.That(SqlServerExtensions.UseWebHostServices(second, static (_, _) => { }, true)).IsEqualTo(second);
        using var secondHost = second.Build();
        await secondHost.StartAsync();
        await secondHost.StopAsync();

        var third = CreateWebHostBuilder();
        await Assert.That(SqlServerExtensions.UseWebHostServices(third, static (_, _) => { }, builder =>
        {
            webHostConfigured = true;
            return builder;
        })).IsEqualTo(third);
        using var thirdHost = third.Build();
        await thirdHost.StartAsync();
        await Assert.That(await SendRequestAsync(thirdHost)).IsTrue();
        await thirdHost.StopAsync();

        var fourth = CreateWebHostBuilder();
        await Assert.That(SqlServerExtensions.UseWebHostServices(fourth, static (_, _) => { }, static builder => builder, true)).IsEqualTo(fourth);
        using var fourthHost = fourth.Build();
        await fourthHost.StartAsync();
        await fourthHost.StopAsync();

        var fifth = CreateWebHostBuilder();
        await Assert.That(SqlServerExtensions.UseWebHostServices(fifth, static (_, _) => { }, static builder => builder.UseUrls(EphemeralWebHostUrl), app =>
        {
            appConfigured = true;
            return app;
        })).IsEqualTo(fifth);
        using var fifthHost = fifth.Build();
        await fifthHost.StartAsync();
        await Assert.That(await SendRequestAsync(fifthHost)).IsTrue();
        await fifthHost.StopAsync();

        var sixth = CreateWebHostBuilder();
        await Assert.That(SqlServerExtensions.UseWebHostServices(sixth, static (_, _) => { }, static builder => builder, static app => app, true)).IsEqualTo(sixth);
        using var sixthHost = sixth.Build();
        await sixthHost.StartAsync();
        await sixthHost.StopAsync();

        IHostBuilder? nullBuilder = null;
        var nullBuilderAction = () => SqlServerExtensions.UseWebHostServices(nullBuilder!, static (_, _) => { }, true);
        await Assert.That(nullBuilderAction).Throws<ArgumentNullException>();
        await Assert.That(serviceConfigured).IsTrue();
        await Assert.That(webHostConfigured).IsTrue();
        await Assert.That(appConfigured).IsTrue();
    }

    /// <summary>Verifies SQLite web-host service overloads.</summary>
    /// <returns>A task representing the test.</returns>
    [Test]
    public async Task SqliteWebHostServiceOverloads_ReturnBuildersAndRunConfiguration()
    {
        var serviceConfigured = false;
        var webHostConfigured = false;
        var appConfigured = false;

        var first = CreateWebHostBuilder();
        await Assert.That(SqliteExtensions.UseWebHostServices(first, (_, _) => serviceConfigured = true)).IsEqualTo(first);
        using var firstHost = first.Build();
        await firstHost.StartAsync();
        await Assert.That(await SendRequestAsync(firstHost)).IsTrue();
        await firstHost.StopAsync();

        var second = CreateWebHostBuilder();
        await Assert.That(SqliteExtensions.UseWebHostServices(second, static (_, _) => { }, true)).IsEqualTo(second);
        using var secondHost = second.Build();

        var third = CreateWebHostBuilder();
        await Assert.That(SqliteExtensions.UseWebHostServices(third, static (_, _) => { }, builder =>
        {
            webHostConfigured = true;
            return builder;
        })).IsEqualTo(third);
        using var thirdHost = third.Build();
        await thirdHost.StartAsync();
        await Assert.That(await SendRequestAsync(thirdHost)).IsTrue();
        await thirdHost.StopAsync();

        var fourth = CreateWebHostBuilder();
        await Assert.That(SqliteExtensions.UseWebHostServices(fourth, static (_, _) => { }, static builder => builder, true)).IsEqualTo(fourth);
        using var fourthHost = fourth.Build();

        var fifth = CreateWebHostBuilder();
        await Assert.That(SqliteExtensions.UseWebHostServices(fifth, static (_, _) => { }, static builder => builder.UseUrls(EphemeralWebHostUrl), app =>
        {
            appConfigured = true;
            return app;
        })).IsEqualTo(fifth);
        using var fifthHost = fifth.Build();
        await fifthHost.StartAsync();
        await Assert.That(await SendRequestAsync(fifthHost)).IsTrue();
        await fifthHost.StopAsync();

        var sixth = CreateWebHostBuilder();
        await Assert.That(SqliteExtensions.UseWebHostServices(sixth, static (_, _) => { }, static builder => builder, static app => app, true)).IsEqualTo(sixth);
        using var sixthHost = sixth.Build();

        IHostBuilder? nullBuilder = null;
        var nullBuilderAction = () => SqliteExtensions.UseWebHostServices(nullBuilder!, static (_, _) => { }, true);
        await Assert.That(nullBuilderAction).Throws<ArgumentNullException>();
        await Assert.That(serviceConfigured).IsTrue();
        await Assert.That(webHostConfigured).IsTrue();
        await Assert.That(appConfigured).IsTrue();
    }

    /// <summary>Creates configuration containing the test connection string.</summary>
    /// <returns>The configuration.</returns>
    private static IConfiguration CreateConfiguration() => new ConfigurationBuilder()
        .AddInMemoryCollection(
        [
            new KeyValuePair<string, string?>("ConnectionStrings:Database", SqlServerConnectionString),
        ])
        .Build();

    /// <summary>Creates an application builder containing the test connection string.</summary>
    /// <returns>The application builder.</returns>
    private static HostApplicationBuilder CreateApplicationBuilder()
    {
        var builder = Host.CreateApplicationBuilder();
        _ = builder.Configuration.AddInMemoryCollection(
        [
            new KeyValuePair<string, string?>("ConnectionStrings:Database", SqlServerConnectionString),
        ]);
        return builder;
    }

    /// <summary>Creates a web-host builder that selects an ephemeral local port.</summary>
    /// <returns>The configured host builder.</returns>
    private static IHostBuilder CreateWebHostBuilder() => Host.CreateDefaultBuilder()
        .ConfigureWebHostDefaults(static builder => builder.UseUrls(EphemeralWebHostUrl));

    /// <summary>Sends a request to the active web host.</summary>
    /// <param name="host">The started host.</param>
    /// <returns>true when the host successfully responds.</returns>
    private static async Task<bool> SendRequestAsync(IHost host)
    {
        ArgumentNullException.ThrowIfNull(host);

        var server = host.Services.GetRequiredService<IServer>();
        var addresses = server.Features.Get<IServerAddressesFeature>()?.Addresses;
        var address = addresses?.Single() ?? throw new InvalidOperationException("The web host did not expose a listening address.");
        using var response = await _httpClient.GetAsync(new Uri(address, UriKind.Absolute));
        return response.IsSuccessStatusCode;
    }

    /// <summary>Resolves a context and returns its configured provider name.</summary>
    /// <typeparam name="TContext">The context type.</typeparam>
    /// <param name="services">The registered services.</param>
    /// <returns>The provider name.</returns>
    private static string? GetProviderName<TContext>(IServiceCollection services)
        where TContext : DbContext
    {
        using var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<TContext>().Database.ProviderName;
    }

    /// <summary>Determines whether a service is registered.</summary>
    /// <typeparam name="TService">The service type.</typeparam>
    /// <param name="services">The services to inspect.</param>
    /// <returns>true when the service is registered.</returns>
    private static bool ContainsService<TService>(IServiceCollection services)
    {
        foreach (var descriptor in services)
        {
            if (descriptor.ServiceType == typeof(TService))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Provides a SQL Server context test double.</summary>
    /// <param name="options">The context options.</param>
    public sealed class SqlServerContext(DbContextOptions<SqlServerContext> options) : DbContext(options);

    /// <summary>Provides a SQLite context test double.</summary>
    /// <param name="options">The context options.</param>
    public sealed class SqliteContext(DbContextOptions<SqliteContext> options) : DbContext(options);

    /// <summary>Provides a SQL Server identity context test double.</summary>
    /// <param name="options">The context options.</param>
    public sealed class SqlServerIdentityContext(DbContextOptions<SqlServerIdentityContext> options) : IdentityDbContext<TestUser, TestRole, string>(options);

    /// <summary>Provides a SQLite identity context test double.</summary>
    /// <param name="options">The context options.</param>
    public sealed class SqliteIdentityContext(DbContextOptions<SqliteIdentityContext> options) : IdentityDbContext<TestUser, TestRole, string>(options);

    /// <summary>Provides an identity user test double.</summary>
    public sealed class TestUser : IdentityUser;

    /// <summary>Provides an identity role test double.</summary>
    public sealed class TestRole : IdentityRole;

    /// <summary>Marks web-host service registration.</summary>
    public sealed class WebHostMarker
    {
        /// <summary>Gets a marker value.</summary>
        public bool IsRegistered => true;
    }
}
