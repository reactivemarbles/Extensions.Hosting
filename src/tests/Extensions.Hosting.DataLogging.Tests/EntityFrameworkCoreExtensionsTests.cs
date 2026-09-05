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

    /// <summary>Creates SQL Server contexts for extension registration tests.</summary>
    private static readonly Func<IServiceProvider, DbContextOptions<SqlServerContext>, SqlServerContext> _createSqlServerContext =
        static (serviceProvider, options) =>
        {
            _ = serviceProvider;
            return new SqlServerContext(options);
        };

    /// <summary>Creates SQL Server identity contexts for extension registration tests.</summary>
    private static readonly Func<IServiceProvider, DbContextOptions<SqlServerIdentityContext>, SqlServerIdentityContext> _createSqlServerIdentityContext =
        static (serviceProvider, options) =>
        {
            _ = serviceProvider;
            return new SqlServerIdentityContext(options);
        };

    /// <summary>Creates SQLite contexts for extension registration tests.</summary>
    private static readonly Func<IServiceProvider, DbContextOptions<SqliteContext>, SqliteContext> _createSqliteContext =
        static (serviceProvider, options) =>
        {
            _ = serviceProvider;
            return new SqliteContext(options);
        };

    /// <summary>Creates SQLite identity contexts for extension registration tests.</summary>
    private static readonly Func<IServiceProvider, DbContextOptions<SqliteIdentityContext>, SqliteIdentityContext> _createSqliteIdentityContext =
        static (serviceProvider, options) =>
        {
            _ = serviceProvider;
            return new SqliteIdentityContext(options);
        };

    /// <summary>Configures test user Identity services.</summary>
    private static readonly Func<IServiceCollection, IdentityBuilder> _addTestUserIdentity =
        static services => services.AddDefaultIdentity<TestUser>();

    /// <summary>Configures test user and role Identity services.</summary>
    private static readonly Func<IServiceCollection, IdentityBuilder> _addTestUserAndRoleIdentity =
        static services => services.AddDefaultIdentity<TestUser>().AddRoles<TestRole>();

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
        IConfiguration? nullConfiguration = null;
        var nullHasConnection = () => SqlServerExtensions.HasConnectionString(nullConfiguration!, DatabaseConnectionName);
        var nullGetRequired = () => SqlServerExtensions.GetRequiredConnectionString(nullConfiguration!, DatabaseConnectionName);
        await Assert.That(missing).Throws<InvalidOperationException>();
        await Assert.That(whitespace).Throws<ArgumentException>();
        await Assert.That(nullHasConnection).Throws<ArgumentNullException>();
        await Assert.That(nullGetRequired).Throws<ArgumentNullException>();
    }

    /// <summary>Verifies SQL Server application-builder validation.</summary>
    /// <returns>A task representing the test.</returns>
    [Test]
    public async Task SqlServerApplicationBuilderRegistrations_RegisterAllOverloads()
    {
        var databaseBuilder = CreateApplicationBuilder();
        var databaseResult = SqlServerExtensions.AddSqlServerDbContext(databaseBuilder, DatabaseConnectionName, _createSqlServerContext);
        await Assert.That(databaseResult).IsEqualTo(databaseBuilder);
        await Assert.That(ContainsService<SqlServerContext>(databaseBuilder.Services)).IsTrue();
        await Assert.That(GetProviderName<SqlServerContext>(databaseBuilder.Services)).IsEqualTo(SqlServerProviderName);

        var databaseLifetimeBuilder = CreateApplicationBuilder();
        var databaseLifetimeResult = SqlServerExtensions.AddSqlServerDbContext(databaseLifetimeBuilder, DatabaseConnectionName, _createSqlServerContext, ServiceLifetime.Singleton);
        await Assert.That(databaseLifetimeResult).IsEqualTo(databaseLifetimeBuilder);

        var userIdentityBuilder = CreateApplicationBuilder();
        var userIdentityResult = SqlServerExtensions.AddSqlServerWithIdentity(
            userIdentityBuilder,
            DatabaseConnectionName,
            _createSqlServerIdentityContext,
            _addTestUserIdentity);
        await Assert.That(userIdentityResult).IsEqualTo(userIdentityBuilder);
        await Assert.That(ContainsService<SqlServerIdentityContext>(userIdentityBuilder.Services)).IsTrue();
        await Assert.That(ContainsService<UserManager<TestUser>>(userIdentityBuilder.Services)).IsTrue();
        await Assert.That(GetProviderName<SqlServerIdentityContext>(userIdentityBuilder.Services)).IsEqualTo(SqlServerProviderName);

        var userIdentityLifetimeBuilder = CreateApplicationBuilder();
        var userIdentityLifetimeResult = SqlServerExtensions.AddSqlServerWithIdentity(
            userIdentityLifetimeBuilder,
            DatabaseConnectionName,
            _createSqlServerIdentityContext,
            _addTestUserIdentity,
            ServiceLifetime.Singleton);
        await Assert.That(userIdentityLifetimeResult).IsEqualTo(userIdentityLifetimeBuilder);

        var roleIdentityBuilder = CreateApplicationBuilder();
        var roleIdentityResult = SqlServerExtensions.AddSqlServerWithIdentity(
            roleIdentityBuilder,
            DatabaseConnectionName,
            _createSqlServerIdentityContext,
            _addTestUserAndRoleIdentity);
        await Assert.That(roleIdentityResult).IsEqualTo(roleIdentityBuilder);
        await Assert.That(ContainsService<SqlServerIdentityContext>(roleIdentityBuilder.Services)).IsTrue();
        await Assert.That(ContainsService<RoleManager<TestRole>>(roleIdentityBuilder.Services)).IsTrue();
        await Assert.That(GetProviderName<SqlServerIdentityContext>(roleIdentityBuilder.Services)).IsEqualTo(SqlServerProviderName);

        var roleIdentityLifetimeBuilder = CreateApplicationBuilder();
        var roleIdentityLifetimeResult = SqlServerExtensions.AddSqlServerWithIdentity(
            roleIdentityLifetimeBuilder,
            DatabaseConnectionName,
            _createSqlServerIdentityContext,
            _addTestUserAndRoleIdentity,
            ServiceLifetime.Singleton);
        await Assert.That(roleIdentityLifetimeResult).IsEqualTo(roleIdentityLifetimeBuilder);
    }

    /// <summary>Verifies SQL Server service-collection registrations.</summary>
    /// <returns>A task representing the test.</returns>
    [Test]
    public async Task SqlServerApplicationBuilderRegistrations_ValidateArgumentsAndMissingConnections()
    {
        IHostApplicationBuilder? nullBuilder = null;
        var nullBuilderAction = () => SqlServerExtensions.AddSqlServerDbContext(nullBuilder!, DatabaseConnectionName, _createSqlServerContext, ServiceLifetime.Scoped);
        await Assert.That(nullBuilderAction).Throws<ArgumentNullException>();

        var builder = CreateApplicationBuilder();
        var whitespace = () => SqlServerExtensions.AddSqlServerDbContext(builder, " ", _createSqlServerContext, ServiceLifetime.Scoped);
        await Assert.That(whitespace).Throws<ArgumentException>();

        var missingBuilder = Host.CreateApplicationBuilder();
        var missing = () => SqlServerExtensions.AddSqlServerDbContext(
            missingBuilder,
            MissingConnectionName,
            _createSqlServerContext,
            ServiceLifetime.Scoped);
        await Assert.That(missing).Throws<InvalidOperationException>();
    }

    /// <summary>Verifies SQL Server service-collection validation.</summary>
    /// <returns>A task representing the test.</returns>
    [Test]
    public async Task SqlServerServiceCollectionRegistrations_RegisterAllOverloadsAndProviders()
    {
        var configuration = CreateConfiguration();

        var databaseServices = new ServiceCollection();
        var databaseResult = SqlServerExtensions.AddSqlServerDbContext(databaseServices, configuration, DatabaseConnectionName, _createSqlServerContext);
        await Assert.That(databaseResult).IsEqualTo(databaseServices);
        await Assert.That(GetProviderName<SqlServerContext>(databaseServices)).IsEqualTo(SqlServerProviderName);

        var databaseLifetimeServices = new ServiceCollection();
        var databaseLifetimeResult = SqlServerExtensions.AddSqlServerDbContext(databaseLifetimeServices, configuration, DatabaseConnectionName, _createSqlServerContext, ServiceLifetime.Singleton);
        await Assert.That(databaseLifetimeResult).IsEqualTo(databaseLifetimeServices);

        var connectionStringServices = new ServiceCollection();
        var connectionStringResult = SqlServerExtensions.AddSqlServerDbContextWithConnectionString(connectionStringServices, SqlServerConnectionString, _createSqlServerContext);
        await Assert.That(connectionStringResult).IsEqualTo(connectionStringServices);
        await Assert.That(GetProviderName<SqlServerContext>(connectionStringServices)).IsEqualTo(SqlServerProviderName);

        var connectionStringLifetimeServices = new ServiceCollection();
        var connectionStringLifetimeResult = SqlServerExtensions.AddSqlServerDbContextWithConnectionString(
            connectionStringLifetimeServices,
            SqlServerConnectionString,
            _createSqlServerContext,
            ServiceLifetime.Singleton);
        await Assert.That(connectionStringLifetimeResult).IsEqualTo(connectionStringLifetimeServices);
    }

    /// <summary>Verifies SQL Server service-collection Identity registrations.</summary>
    /// <returns>A task representing the test.</returns>
    [Test]
    public async Task SqlServerServiceCollectionRegistrations_RegisterIdentityOverloads()
    {
        var configuration = CreateConfiguration();
        var context = new WebHostBuilderContext { Configuration = configuration };

        var userServices = new ServiceCollection();
        var userResult = SqlServerExtensions.UseEntityFrameworkCoreSqlServer(
            userServices,
            context,
            DatabaseConnectionName,
            _createSqlServerIdentityContext,
            _addTestUserIdentity);
        await Assert.That(userResult).IsEqualTo(userServices);
        await Assert.That(ContainsService<SqlServerIdentityContext>(userServices)).IsTrue();
        await Assert.That(ContainsService<UserManager<TestUser>>(userServices)).IsTrue();

        var userLifetimeServices = new ServiceCollection();
        var userLifetimeResult = SqlServerExtensions.UseEntityFrameworkCoreSqlServer(
            userLifetimeServices,
            context,
            DatabaseConnectionName,
            _createSqlServerIdentityContext,
            _addTestUserIdentity,
            ServiceLifetime.Singleton);
        await Assert.That(userLifetimeResult).IsEqualTo(userLifetimeServices);

        var roleServices = new ServiceCollection();
        var roleResult = SqlServerExtensions.UseEntityFrameworkCoreSqlServer(
            roleServices,
            context,
            DatabaseConnectionName,
            _createSqlServerIdentityContext,
            _addTestUserAndRoleIdentity);
        await Assert.That(roleResult).IsEqualTo(roleServices);
        await Assert.That(ContainsService<SqlServerIdentityContext>(roleServices)).IsTrue();
        await Assert.That(ContainsService<RoleManager<TestRole>>(roleServices)).IsTrue();

        var roleLifetimeServices = new ServiceCollection();
        var roleLifetimeResult = SqlServerExtensions.UseEntityFrameworkCoreSqlServer(
            roleLifetimeServices,
            context,
            DatabaseConnectionName,
            _createSqlServerIdentityContext,
            _addTestUserAndRoleIdentity,
            ServiceLifetime.Singleton);
        await Assert.That(roleLifetimeResult).IsEqualTo(roleLifetimeServices);
        await Assert.That(GetProviderName<SqlServerIdentityContext>(userServices)).IsEqualTo(SqlServerProviderName);
        await Assert.That(GetProviderName<SqlServerIdentityContext>(roleServices)).IsEqualTo(SqlServerProviderName);
    }

    /// <summary>Verifies DbContext registration helpers preserve existing caller registrations.</summary>
    /// <returns>A task representing the test.</returns>
    [Test]
    public async Task ServiceCollectionRegistrations_PreserveExistingContextRegistrations()
    {
        var factoryInvoked = false;
        var factoryServices = new ServiceCollection();
        _ = factoryServices.AddSingleton(new WebHostMarker());
        _ = SqlServerExtensions.AddSqlServerDbContextWithConnectionString<SqlServerContext>(
            factoryServices,
            SqlServerConnectionString,
            (serviceProvider, options) =>
            {
                factoryInvoked = serviceProvider.GetRequiredService<WebHostMarker>().IsRegistered;
                return new SqlServerContext(options);
            },
            ServiceLifetime.Singleton);

        await using var factoryProvider = factoryServices.BuildServiceProvider();
        _ = factoryProvider.GetRequiredService<SqlServerContext>();
        await Assert.That(factoryInvoked).IsTrue();

        var sqliteFactoryInvoked = false;
        var sqliteFactoryServices = new ServiceCollection();
        _ = sqliteFactoryServices.AddSingleton(new WebHostMarker());
        _ = SqliteExtensions.AddSqliteDbContextWithConnectionString<SqliteContext>(
            sqliteFactoryServices,
            SqliteMemoryConnectionString,
            (serviceProvider, options) =>
            {
                sqliteFactoryInvoked = serviceProvider.GetRequiredService<WebHostMarker>().IsRegistered;
                return new SqliteContext(options);
            },
            ServiceLifetime.Singleton);

        await using var sqliteFactoryProvider = sqliteFactoryServices.BuildServiceProvider();
        _ = sqliteFactoryProvider.GetRequiredService<SqliteContext>();
        await Assert.That(sqliteFactoryInvoked).IsTrue();

        var sqlServerOptions = new DbContextOptions<SqlServerContext>();
        var sqlServerContext = new SqlServerContext(sqlServerOptions);
        var sqlServerServices = new ServiceCollection();
        _ = sqlServerServices.AddSingleton(sqlServerContext);

        _ = SqlServerExtensions.AddSqlServerDbContextWithConnectionString(
            sqlServerServices,
            SqlServerConnectionString,
            _createSqlServerContext,
            ServiceLifetime.Singleton);

        await using var sqlServerProvider = sqlServerServices.BuildServiceProvider();
        await Assert.That(sqlServerProvider.GetRequiredService<SqlServerContext>()).IsEqualTo(sqlServerContext);

        var sqliteOptions = new DbContextOptions<SqliteContext>();
        var sqliteContext = new SqliteContext(sqliteOptions);
        var sqliteServices = new ServiceCollection();
        _ = sqliteServices.AddSingleton(sqliteContext);

        _ = SqliteExtensions.AddSqliteDbContextWithConnectionString(
            sqliteServices,
            SqliteMemoryConnectionString,
            _createSqliteContext,
            ServiceLifetime.Singleton);

        await using var sqliteProvider = sqliteServices.BuildServiceProvider();
        await Assert.That(sqliteProvider.GetRequiredService<SqliteContext>()).IsEqualTo(sqliteContext);
    }

    /// <summary>Verifies SQL Server registration argument validation for typed factories and Identity callbacks.</summary>
    /// <returns>A task representing the test.</returns>
    [Test]
    public async Task SqlServerRegistrations_ValidateFactoriesAndIdentityCallbacks()
    {
        var builder = CreateApplicationBuilder();
        Func<IServiceProvider, DbContextOptions<SqlServerContext>, SqlServerContext>? nullFactory = null;
        Func<IServiceCollection, IdentityBuilder>? nullIdentity = null;
        Func<IServiceCollection, IdentityBuilder> returnsNullIdentity = ReturnNullIdentityBuilder;
        var context = new WebHostBuilderContext { Configuration = CreateConfiguration() };
        var services = new ServiceCollection();

        var nullBuilderFactory = () => SqlServerExtensions.AddSqlServerDbContext(builder, DatabaseConnectionName, nullFactory!, ServiceLifetime.Scoped);
        var nullBuilderIdentity = () => SqlServerExtensions.AddSqlServerWithIdentity(builder, DatabaseConnectionName, _createSqlServerIdentityContext, nullIdentity!);
        var nullReturnedBuilderIdentity = () => SqlServerExtensions.AddSqlServerWithIdentity(
            builder,
            DatabaseConnectionName,
            _createSqlServerIdentityContext,
            returnsNullIdentity!);
        var missingBuilderIdentity = static () => SqlServerExtensions.AddSqlServerWithIdentity(
            Host.CreateApplicationBuilder(),
            MissingConnectionName,
            _createSqlServerIdentityContext,
            _addTestUserIdentity);
        var nullServicesFactory = () => SqlServerExtensions.AddSqlServerDbContextWithConnectionString(services, SqlServerConnectionString, nullFactory!);
        var nullServicesIdentity = () => SqlServerExtensions.UseEntityFrameworkCoreSqlServer(services, context, DatabaseConnectionName, _createSqlServerIdentityContext, nullIdentity!);
        var nullReturnedServicesIdentity = () => SqlServerExtensions.UseEntityFrameworkCoreSqlServer(
            services,
            context,
            DatabaseConnectionName,
            _createSqlServerIdentityContext,
            returnsNullIdentity!);

        await Assert.That(nullBuilderFactory).Throws<ArgumentNullException>();
        await Assert.That(nullBuilderIdentity).Throws<ArgumentNullException>();
        await Assert.That(nullReturnedBuilderIdentity).Throws<InvalidOperationException>();
        await Assert.That(missingBuilderIdentity).Throws<InvalidOperationException>();
        await Assert.That(nullServicesFactory).Throws<ArgumentNullException>();
        await Assert.That(nullServicesIdentity).Throws<ArgumentNullException>();
        await Assert.That(nullReturnedServicesIdentity).Throws<InvalidOperationException>();
    }

    /// <summary>Verifies SQL Server Identity validation for context factories and missing configuration.</summary>
    /// <returns>A task representing the test.</returns>
    [Test]
    public async Task SqlServerIdentityRegistrations_ValidateContextFactoriesAndMissingConnections()
    {
        IHostApplicationBuilder? nullBuilder = null;
        Func<IServiceProvider, DbContextOptions<SqlServerIdentityContext>, SqlServerIdentityContext>? nullFactory = null;
        var builder = CreateApplicationBuilder();
        var context = new WebHostBuilderContext { Configuration = CreateConfiguration() };
        var services = new ServiceCollection();

        var nullBuilderAction = () => SqlServerExtensions.AddSqlServerWithIdentity(
            nullBuilder!,
            DatabaseConnectionName,
            _createSqlServerIdentityContext,
            _addTestUserIdentity);
        var nullBuilderFactory = () => SqlServerExtensions.AddSqlServerWithIdentity(
            builder,
            DatabaseConnectionName,
            nullFactory!,
            _addTestUserIdentity);
        var nullServicesFactory = () => SqlServerExtensions.UseEntityFrameworkCoreSqlServer(
            services,
            context,
            DatabaseConnectionName,
            nullFactory!,
            _addTestUserIdentity);
        var missingServicesConnection = () => SqlServerExtensions.UseEntityFrameworkCoreSqlServer(
            services,
            context,
            MissingConnectionName,
            _createSqlServerIdentityContext,
            _addTestUserIdentity);

        await Assert.That(nullBuilderAction).Throws<ArgumentNullException>();
        await Assert.That(nullBuilderFactory).Throws<ArgumentNullException>();
        await Assert.That(nullServicesFactory).Throws<ArgumentNullException>();
        await Assert.That(missingServicesConnection).Throws<InvalidOperationException>();
    }

    /// <summary>Verifies SQL Server service-collection validation for DbContext factories.</summary>
    /// <returns>A task representing the test.</returns>
    [Test]
    public async Task SqlServerServiceCollectionRegistrations_ValidateDbContextFactories()
    {
        var configuration = CreateConfiguration();
        var services = new ServiceCollection();
        Func<IServiceProvider, DbContextOptions<SqlServerContext>, SqlServerContext>? nullFactory = null;

        var nullConfigurationFactory = () => SqlServerExtensions.AddSqlServerDbContext(
            services,
            configuration,
            DatabaseConnectionName,
            nullFactory!,
            ServiceLifetime.Scoped);

        await Assert.That(nullConfigurationFactory).Throws<ArgumentNullException>();
    }

    /// <summary>Verifies SQLite application-builder and service registrations.</summary>
    /// <returns>A task representing the test.</returns>
    [Test]
    public async Task SqlServerServiceCollectionRegistrations_ValidateArguments()
    {
        var configuration = CreateConfiguration();
        var context = new WebHostBuilderContext { Configuration = configuration };
        IServiceCollection? nullServices = null;
        var nullServicesAction = () => SqlServerExtensions.AddSqlServerDbContext(
            nullServices!,
            configuration,
            DatabaseConnectionName,
            _createSqlServerContext,
            ServiceLifetime.Scoped);
        await Assert.That(nullServicesAction).Throws<ArgumentNullException>();

        var services = new ServiceCollection();
        IConfiguration? nullConfiguration = null;
        var nullConfigurationAction = () => SqlServerExtensions.AddSqlServerDbContext(
            services,
            nullConfiguration!,
            DatabaseConnectionName,
            _createSqlServerContext,
            ServiceLifetime.Scoped);
        var nullContextAction = () => SqlServerExtensions.UseEntityFrameworkCoreSqlServer(
            services,
            null!,
            DatabaseConnectionName,
            _createSqlServerIdentityContext,
            _addTestUserIdentity,
            ServiceLifetime.Scoped);
        var nullIdentityServicesAction = () => SqlServerExtensions.UseEntityFrameworkCoreSqlServer(
            nullServices!,
            context,
            DatabaseConnectionName,
            _createSqlServerIdentityContext,
            _addTestUserIdentity,
            ServiceLifetime.Scoped);
        var nullConnectionServicesAction = () => SqlServerExtensions.AddSqlServerDbContextWithConnectionString(
            nullServices!,
            SqlServerConnectionString,
            _createSqlServerContext,
            ServiceLifetime.Scoped);
        var whitespace = () => SqlServerExtensions.AddSqlServerDbContextWithConnectionString(
            services,
            " ",
            _createSqlServerContext,
            ServiceLifetime.Scoped);
        await Assert.That(nullConfigurationAction).Throws<ArgumentNullException>();
        await Assert.That(nullContextAction).Throws<ArgumentNullException>();
        await Assert.That(nullIdentityServicesAction).Throws<ArgumentNullException>();
        await Assert.That(nullConnectionServicesAction).Throws<ArgumentNullException>();
        await Assert.That(whitespace).Throws<ArgumentException>();

        var missingServices = new ServiceCollection();
        var missing = () => SqlServerExtensions.AddSqlServerDbContext(
            missingServices,
            configuration,
            MissingConnectionName,
            _createSqlServerContext,
            ServiceLifetime.Scoped);
        await Assert.That(missing).Throws<InvalidOperationException>();

        await Assert.That(context.Configuration).IsEqualTo(configuration);
    }

    /// <summary>Verifies SQLite helpers and validation behavior.</summary>
    /// <returns>A task representing the test.</returns>
    [Test]
    public async Task SqliteApplicationBuilderAndServiceCollectionRegistrations_RegisterAllOverloads()
    {
        var builder = CreateApplicationBuilder();
        await Assert.That(SqliteExtensions.AddSqliteDbContext(builder, DatabaseConnectionName, _createSqliteContext)).IsEqualTo(builder);
        await Assert.That(GetProviderName<SqliteContext>(builder.Services)).IsEqualTo(SqliteProviderName);
        await Assert.That(SqliteExtensions.AddSqliteDbContext(builder, DatabaseConnectionName, _createSqliteContext, ServiceLifetime.Singleton)).IsEqualTo(builder);

        var userBuilder = CreateApplicationBuilder();
        await Assert.That(SqliteExtensions.AddSqliteWithIdentity(
            userBuilder,
            DatabaseConnectionName,
            _createSqliteIdentityContext,
            _addTestUserIdentity)).IsEqualTo(userBuilder);
        await Assert.That(GetProviderName<SqliteIdentityContext>(userBuilder.Services)).IsEqualTo(SqliteProviderName);
        await Assert.That(ContainsService<UserManager<TestUser>>(userBuilder.Services)).IsTrue();
        await Assert.That(SqliteExtensions.AddSqliteWithIdentity(
            userBuilder,
            DatabaseConnectionName,
            _createSqliteIdentityContext,
            _addTestUserIdentity,
            ServiceLifetime.Singleton)).IsEqualTo(userBuilder);

        var roleBuilder = CreateApplicationBuilder();
        await Assert.That(SqliteExtensions.AddSqliteWithIdentity(
            roleBuilder,
            DatabaseConnectionName,
            _createSqliteIdentityContext,
            _addTestUserAndRoleIdentity)).IsEqualTo(roleBuilder);
        await Assert.That(GetProviderName<SqliteIdentityContext>(roleBuilder.Services)).IsEqualTo(SqliteProviderName);
        await Assert.That(ContainsService<RoleManager<TestRole>>(roleBuilder.Services)).IsTrue();
        await Assert.That(SqliteExtensions.AddSqliteWithIdentity(
            roleBuilder,
            DatabaseConnectionName,
            _createSqliteIdentityContext,
            _addTestUserAndRoleIdentity,
            ServiceLifetime.Singleton)).IsEqualTo(roleBuilder);
    }

    /// <summary>Verifies SQLite service-collection registrations.</summary>
    /// <returns>A task representing the test.</returns>
    [Test]
    public async Task SqliteServiceCollectionRegistrations_RegisterAllOverloads()
    {
        var configuration = CreateConfiguration();
        var context = new WebHostBuilderContext { Configuration = configuration };
        var services = new ServiceCollection();

        await Assert.That(SqliteExtensions.AddSqliteDbContext(services, configuration, DatabaseConnectionName, _createSqliteContext)).IsEqualTo(services);
        await Assert.That(GetProviderName<SqliteContext>(services)).IsEqualTo(SqliteProviderName);
        await Assert.That(SqliteExtensions.AddSqliteDbContext(services, configuration, DatabaseConnectionName, _createSqliteContext, ServiceLifetime.Singleton)).IsEqualTo(services);
        await Assert.That(SqliteExtensions.AddSqliteDbContextWithConnectionString(services, SqliteMemoryConnectionString, _createSqliteContext)).IsEqualTo(services);
        await Assert.That(SqliteExtensions.AddSqliteDbContextWithConnectionString(services, SqliteMemoryConnectionString, _createSqliteContext, ServiceLifetime.Singleton)).IsEqualTo(services);
        await Assert.That(SqliteExtensions.UseEntityFrameworkCoreSqlite(services, context, DatabaseConnectionName, _createSqliteIdentityContext, _addTestUserIdentity)).IsEqualTo(services);
        await Assert.That(SqliteExtensions.UseEntityFrameworkCoreSqlite(
            services,
            context,
            DatabaseConnectionName,
            _createSqliteIdentityContext,
            _addTestUserIdentity,
            ServiceLifetime.Singleton)).IsEqualTo(services);
        await Assert.That(SqliteExtensions.UseEntityFrameworkCoreSqlite(services, context, DatabaseConnectionName, _createSqliteIdentityContext, _addTestUserAndRoleIdentity)).IsEqualTo(services);
        await Assert.That(SqliteExtensions.UseEntityFrameworkCoreSqlite(
            services,
            context,
            DatabaseConnectionName,
            _createSqliteIdentityContext,
            _addTestUserAndRoleIdentity,
            ServiceLifetime.Singleton)).IsEqualTo(services);
        await Assert.That(ContainsService<UserManager<TestUser>>(services)).IsTrue();
        await Assert.That(ContainsService<RoleManager<TestRole>>(services)).IsTrue();
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
        var whitespace = () => SqliteExtensions.AddSqliteDbContext(builder, " ", _createSqliteContext, ServiceLifetime.Scoped);
        await Assert.That(whitespace).Throws<ArgumentException>();

        var services = new ServiceCollection();
        var nullServices = static () => SqliteExtensions.AddSqliteDbContextWithConnectionString(null!, SqliteMemoryConnectionString, _createSqliteContext, ServiceLifetime.Scoped);
        var nullConfiguration = () => SqliteExtensions.AddSqliteDbContext(services, null!, DatabaseConnectionName, _createSqliteContext, ServiceLifetime.Scoped);
        var emptyConnectionString = () => SqliteExtensions.AddSqliteDbContextWithConnectionString(services, " ", _createSqliteContext, ServiceLifetime.Scoped);
        await Assert.That(nullServices).Throws<ArgumentNullException>();
        await Assert.That(nullConfiguration).Throws<ArgumentNullException>();
        await Assert.That(emptyConnectionString).Throws<ArgumentException>();
    }

    /// <summary>Verifies SQLite application-builder validation for typed factories and Identity callbacks.</summary>
    /// <returns>A task representing the test.</returns>
    [Test]
    public async Task SqliteApplicationBuilderRegistrations_ValidateFactoriesAndIdentityCallbacks()
    {
        IHostApplicationBuilder? nullBuilder = null;
        var builder = CreateApplicationBuilder();
        Func<IServiceProvider, DbContextOptions<SqliteContext>, SqliteContext>? nullFactory = null;
        Func<IServiceCollection, IdentityBuilder>? nullIdentity = null;
        Func<IServiceCollection, IdentityBuilder> returnsNullIdentity = ReturnNullIdentityBuilder;

        var nullBuilderAction = () => SqliteExtensions.AddSqliteDbContext(
            nullBuilder!,
            DatabaseConnectionName,
            _createSqliteContext,
            ServiceLifetime.Scoped);
        var nullFactoryAction = () => SqliteExtensions.AddSqliteDbContext(
            builder,
            DatabaseConnectionName,
            nullFactory!,
            ServiceLifetime.Scoped);
        var missingAction = static () => SqliteExtensions.AddSqliteDbContext(
            Host.CreateApplicationBuilder(),
            MissingConnectionName,
            _createSqliteContext,
            ServiceLifetime.Scoped);
        var nullIdentityAction = () => SqliteExtensions.AddSqliteWithIdentity(
            builder,
            DatabaseConnectionName,
            _createSqliteIdentityContext,
            nullIdentity!);
        var nullIdentityBuilderAction = () => SqliteExtensions.AddSqliteWithIdentity(
            nullBuilder!,
            DatabaseConnectionName,
            _createSqliteIdentityContext,
            _addTestUserIdentity);
        var nullIdentityFactoryAction = () => SqliteExtensions.AddSqliteWithIdentity<SqliteIdentityContext>(
            builder,
            DatabaseConnectionName,
            null!,
            _addTestUserIdentity);
        var nullReturnedIdentity = () => SqliteExtensions.AddSqliteWithIdentity(
            builder,
            DatabaseConnectionName,
            _createSqliteIdentityContext,
            returnsNullIdentity);
        var missingIdentity = static () => SqliteExtensions.AddSqliteWithIdentity(
            Host.CreateApplicationBuilder(),
            MissingConnectionName,
            _createSqliteIdentityContext,
            _addTestUserIdentity);

        await Assert.That(nullBuilderAction).Throws<ArgumentNullException>();
        await Assert.That(nullFactoryAction).Throws<ArgumentNullException>();
        await Assert.That(missingAction).Throws<InvalidOperationException>();
        await Assert.That(nullIdentityAction).Throws<ArgumentNullException>();
        await Assert.That(nullIdentityBuilderAction).Throws<ArgumentNullException>();
        await Assert.That(nullIdentityFactoryAction).Throws<ArgumentNullException>();
        await Assert.That(nullReturnedIdentity).Throws<InvalidOperationException>();
        await Assert.That(missingIdentity).Throws<InvalidOperationException>();
    }

    /// <summary>Verifies SQLite service-collection validation for typed factories and Identity callbacks.</summary>
    /// <returns>A task representing the test.</returns>
    [Test]
    public async Task SqliteServiceCollectionRegistrations_ValidateFactoriesAndIdentityCallbacks()
    {
        var configuration = CreateConfiguration();
        var context = new WebHostBuilderContext { Configuration = configuration };
        var services = new ServiceCollection();
        IServiceCollection? nullServices = null;
        Func<IServiceProvider, DbContextOptions<SqliteIdentityContext>, SqliteIdentityContext>? nullIdentityFactory = null;
        Func<IServiceCollection, IdentityBuilder>? nullIdentity = null;
        Func<IServiceCollection, IdentityBuilder> returnsNullIdentity = ReturnNullIdentityBuilder;

        var nullIdentityServicesAction = () => SqliteExtensions.UseEntityFrameworkCoreSqlite(
            nullServices!,
            context,
            DatabaseConnectionName,
            _createSqliteIdentityContext,
            _addTestUserIdentity);
        var nullIdentityFactoryAction = () => SqliteExtensions.UseEntityFrameworkCoreSqlite(
            services,
            context,
            DatabaseConnectionName,
            nullIdentityFactory!,
            _addTestUserIdentity);
        var nullIdentityAction = () => SqliteExtensions.UseEntityFrameworkCoreSqlite(
            services,
            context,
            DatabaseConnectionName,
            _createSqliteIdentityContext,
            nullIdentity!);
        var nullReturnedIdentity = () => SqliteExtensions.UseEntityFrameworkCoreSqlite(
            services,
            context,
            DatabaseConnectionName,
            _createSqliteIdentityContext,
            returnsNullIdentity);
        var missingIdentityConnection = () => SqliteExtensions.UseEntityFrameworkCoreSqlite(
            services,
            context,
            MissingConnectionName,
            _createSqliteIdentityContext,
            _addTestUserIdentity);
        await Assert.That(nullIdentityServicesAction).Throws<ArgumentNullException>();
        await Assert.That(nullIdentityFactoryAction).Throws<ArgumentNullException>();
        await Assert.That(nullIdentityAction).Throws<ArgumentNullException>();
        await Assert.That(nullReturnedIdentity).Throws<InvalidOperationException>();
        await Assert.That(missingIdentityConnection).Throws<InvalidOperationException>();
    }

    /// <summary>Verifies SQLite service-collection validation for DbContext callbacks.</summary>
    /// <returns>A task representing the test.</returns>
    [Test]
    public async Task SqliteServiceCollectionRegistrations_ValidateDbContextFactories()
    {
        var configuration = CreateConfiguration();
        var context = new WebHostBuilderContext { Configuration = configuration };
        var services = new ServiceCollection();
        IServiceCollection? nullServices = null;
        Func<IServiceProvider, DbContextOptions<SqliteContext>, SqliteContext>? nullFactory = null;

        var nullIdentityContext = () => SqliteExtensions.UseEntityFrameworkCoreSqlite(
            services,
            null!,
            DatabaseConnectionName,
            _createSqliteIdentityContext,
            _addTestUserIdentity);
        var nullConfigurationFactory = () => SqliteExtensions.AddSqliteDbContext(
            services,
            configuration,
            DatabaseConnectionName,
            nullFactory!,
            ServiceLifetime.Scoped);
        var nullConfigurationServices = () => SqliteExtensions.AddSqliteDbContext(
            nullServices!,
            configuration,
            DatabaseConnectionName,
            _createSqliteContext,
            ServiceLifetime.Scoped);
        var nullConnectionStringServices = () => SqliteExtensions.AddSqliteDbContextWithConnectionString(
            nullServices!,
            SqliteMemoryConnectionString,
            _createSqliteContext,
            ServiceLifetime.Scoped);
        var nullConnectionStringFactory = () => SqliteExtensions.AddSqliteDbContextWithConnectionString(
            services,
            SqliteMemoryConnectionString,
            nullFactory!,
            ServiceLifetime.Scoped);
        var missingConfigurationConnection = () => SqliteExtensions.AddSqliteDbContext(
            services,
            configuration,
            MissingConnectionName,
            _createSqliteContext,
            ServiceLifetime.Scoped);

        await Assert.That(nullIdentityContext).Throws<ArgumentNullException>();
        await Assert.That(nullConfigurationFactory).Throws<ArgumentNullException>();
        await Assert.That(nullConfigurationServices).Throws<ArgumentNullException>();
        await Assert.That(nullConnectionStringServices).Throws<ArgumentNullException>();
        await Assert.That(nullConnectionStringFactory).Throws<ArgumentNullException>();
        await Assert.That(missingConfigurationConnection).Throws<InvalidOperationException>();
        await Assert.That(context.Configuration).IsEqualTo(configuration);
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

        await Assert.That(serviceConfigured).IsTrue();
        await Assert.That(webHostConfigured).IsTrue();
        await Assert.That(appConfigured).IsTrue();
    }

    /// <summary>Verifies SQL Server web-host overload null receiver validation.</summary>
    /// <returns>A task representing the test.</returns>
    [Test]
    public async Task WebHostServiceOverloads_ValidateNullBuilders()
    {
        IHostBuilder? nullBuilder = null;
        var nullBuilderAction = () => SqlServerExtensions.UseWebHostServices(nullBuilder!, static (_, _) => { }, true);
        var nullWebHostBuilderAction = () => SqlServerExtensions.UseWebHostServices(
            nullBuilder!,
            static (_, _) => { },
            static builder => builder,
            true);
        var nullAppBuilderAction = () => SqlServerExtensions.UseWebHostServices(
            nullBuilder!,
            static (_, _) => { },
            static builder => builder,
            static app => app,
            true);

        await Assert.That(nullBuilderAction).Throws<ArgumentNullException>();
        await Assert.That(nullWebHostBuilderAction).Throws<ArgumentNullException>();
        await Assert.That(nullAppBuilderAction).Throws<ArgumentNullException>();
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

        await Assert.That(serviceConfigured).IsTrue();
        await Assert.That(webHostConfigured).IsTrue();
        await Assert.That(appConfigured).IsTrue();
    }

    /// <summary>Verifies SQLite web-host overload null receiver validation.</summary>
    /// <returns>A task representing the test.</returns>
    [Test]
    public async Task SqliteWebHostServiceOverloads_ValidateNullBuilders()
    {
        IHostBuilder? nullBuilder = null;
        var nullBuilderAction = () => SqliteExtensions.UseWebHostServices(nullBuilder!, static (_, _) => { }, true);
        var nullWebHostBuilderAction = () => SqliteExtensions.UseWebHostServices(
            nullBuilder!,
            static (_, _) => { },
            static builder => builder,
            true);
        var nullAppBuilderAction = () => SqliteExtensions.UseWebHostServices(
            nullBuilder!,
            static (_, _) => { },
            static builder => builder,
            static app => app,
            true);

        await Assert.That(nullBuilderAction).Throws<ArgumentNullException>();
        await Assert.That(nullWebHostBuilderAction).Throws<ArgumentNullException>();
        await Assert.That(nullAppBuilderAction).Throws<ArgumentNullException>();
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

    /// <summary>Returns a null Identity builder for validation tests.</summary>
    /// <param name="services">The service collection passed by the production callback.</param>
    /// <returns>A null Identity builder.</returns>
    private static IdentityBuilder ReturnNullIdentityBuilder(IServiceCollection services)
    {
        _ = services;
        return null!;
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
