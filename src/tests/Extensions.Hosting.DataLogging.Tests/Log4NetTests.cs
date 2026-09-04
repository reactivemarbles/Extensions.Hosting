// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using log4net;
using CoreLevel = log4net.Core.Level;
using LoggingEvent = log4net.Core.LoggingEvent;

namespace Extensions.Hosting.DataLogging.Tests;

/// <summary>Contains Log4Net adapter coverage tests.</summary>
public class Log4NetTests
{
    /// <summary>Provides the debug level name.</summary>
    private const string DebugLevelName = "DEBUG";

    /// <summary>Provides a log message.</summary>
    private const string MessageText = "message";

    /// <summary>Provides a candidate state value.</summary>
    private const string StateText = "state";

    /// <summary>Provides a generic value.</summary>
    private const string ValueText = "value";

    /// <summary>Provides a logging scope key and value.</summary>
    private const string ScopeText = "scope";

    /// <summary>Provides a test event identifier.</summary>
    private const int EventIdentifier = 3;

    /// <summary>Provides an invalid log-level value.</summary>
    private const int InvalidLogLevelValue = 999;

    /// <summary>Provides a numeric scope value.</summary>
    private const int NumericScopeValue = 7;

    /// <summary>Provides a numeric object scope value.</summary>
    private const int ObjectScopeValue = 5;

    /// <summary>Provides a candidate event identifier.</summary>
    private const int CandidateEventIdentifier = 42;

    /// <summary>Provides the expected provider registration count.</summary>
    private const int ExpectedProviderCount = 3;

    /// <summary>Verifies provider options and message-candidate values.</summary>
    /// <returns>A task representing the test.</returns>
    [Test]
    public async Task ProviderOptionsAndMessageCandidate_PreserveSuppliedValues()
    {
        var defaults = new Log4NetProviderOptions();
        var singleArgument = new Log4NetProviderOptions("custom.config");
        var options = new Log4NetProviderOptions("watched.config", true)
        {
            Name = "test-name",
            LoggerRepository = "test-repository",
            OverrideCriticalLevelWith = "Critical",
            ExternalConfigurationSetup = true,
            UseWebOrAppConfig = true,
            ConfigurationAssembly = typeof(Log4NetTests).Assembly,
        };
        var exception = new InvalidOperationException("failure");
        var candidate = new MessageCandidate<string>(LogLevel.Warning, new EventId(CandidateEventIdentifier, "test"), StateText, exception, static (state, _) => state);

        await Assert.That(defaults.Log4NetConfigFileName).IsEqualTo("log4net.config");
        await Assert.That(singleArgument.Watch).IsFalse();
        await Assert.That(options.Watch).IsTrue();
        options.Watch = false;
        options.Log4NetConfigFileName = "replacement.config";
        await Assert.That(options.Watch).IsFalse();
        await Assert.That(options.Log4NetConfigFileName).IsEqualTo("replacement.config");
        await Assert.That(options.Name).IsEqualTo("test-name");
        await Assert.That(options.PropertyOverrides.Count).IsEqualTo(0);
        await Assert.That(candidate.LogLevel).IsEqualTo(LogLevel.Warning);
        await Assert.That(candidate.EventId.Id).IsEqualTo(CandidateEventIdentifier);
        await Assert.That(candidate.State).IsEqualTo(StateText);
        await Assert.That(candidate.Exception).IsEqualTo(exception);
        await Assert.That(candidate.Formatter(candidate.State, candidate.Exception)).IsEqualTo(StateText);
    }

    /// <summary>Verifies log-level translation.</summary>
    /// <returns>A task representing the test.</returns>
    [Test]
    public async Task LogLevelTranslator_TranslatesEveryLevelAndCriticalOverride()
    {
        var translator = new Log4NetLogLevelTranslator();
        var normalOptions = new Log4NetProviderOptions();
        var criticalOptions = new Log4NetProviderOptions { OverrideCriticalLevelWith = "critical" };

        await Assert.That(translator.TranslateLogLevel(LogLevel.Trace, normalOptions)).IsEqualTo(CoreLevel.Trace);
        await Assert.That(translator.TranslateLogLevel(LogLevel.Debug, normalOptions)).IsEqualTo(CoreLevel.Debug);
        await Assert.That(translator.TranslateLogLevel(LogLevel.Information, normalOptions)).IsEqualTo(CoreLevel.Info);
        await Assert.That(translator.TranslateLogLevel(LogLevel.Warning, normalOptions)).IsEqualTo(CoreLevel.Warn);
        await Assert.That(translator.TranslateLogLevel(LogLevel.Error, normalOptions)).IsEqualTo(CoreLevel.Error);
        await Assert.That(translator.TranslateLogLevel(LogLevel.Critical, normalOptions)).IsEqualTo(CoreLevel.Fatal);
        await Assert.That(translator.TranslateLogLevel(LogLevel.Critical, criticalOptions)).IsEqualTo(CoreLevel.Critical);
        await Assert.That(translator.TranslateLogLevel(LogLevel.None, normalOptions)).IsNull();
        await Assert.That(translator.TranslateLogLevel((LogLevel)InvalidLogLevelValue, normalOptions)).IsNull();

        var nullOptions = () => translator.TranslateLogLevel(LogLevel.Critical, null!);
        await Assert.That(nullOptions).Throws<ArgumentNullException>();
    }

    /// <summary>Verifies logging-event creation and scope enrichment.</summary>
    /// <returns>A task representing the test.</returns>
    [Test]
    public async Task LoggingEventFactory_CreatesEventsAndEnrichesAllPublicScopeShapes()
    {
        using var provider = CreateExternalProvider();
        var logger = provider.CreateLogger("scope-category") as Log4NetLogger;
        var scopes = new LoggerExternalScopeProvider();
        using var stringScope = scopes.Push("one");
        using var enumerableScope = scopes.Push(new List<KeyValuePair<string, object>> { new("object", ObjectScopeValue), new("eventId", "overridden"), });
        using var stringEnumerableScope = scopes.Push(new List<KeyValuePair<string, string>> { new("text", ValueText), });
        using var tupleScope = scopes.Push(("tuple", NumericScopeValue));
        using var arbitraryScope = scopes.Push(new ScopeValue(ValueText));
        var factory = new Log4NetLoggingEventFactory();
        var options = new Log4NetProviderOptions { LogLevelTranslator = new Log4NetLogLevelTranslator(), };
        var candidate = new MessageCandidate<string>(LogLevel.Information, new EventId(EventIdentifier, "event"), MessageText, null, static (state, _) => state);

        var loggingEvent = factory.CreateLoggingEvent(in candidate, GetCoreLogger(logger!), options, scopes);

        await Assert.That(loggingEvent).IsNotNull();
        await Assert.That(loggingEvent!.RenderedMessage).IsEqualTo(MessageText);
        await Assert.That(loggingEvent.Properties[ScopeText]).IsEqualTo("one value");
        await Assert.That(loggingEvent.Properties["object"]).IsEqualTo(ObjectScopeValue.ToString(System.Globalization.CultureInfo.InvariantCulture));
        await Assert.That(loggingEvent.Properties["text"]).IsEqualTo(ValueText);
        await Assert.That(loggingEvent.Properties["tuple"]).IsEqualTo(NumericScopeValue.ToString(System.Globalization.CultureInfo.InvariantCulture));
        await Assert.That(loggingEvent.Properties["eventId"]).IsEqualTo(new EventId(EventIdentifier, "event"));
    }

    /// <summary>Verifies logging-event empty and invalid paths.</summary>
    /// <returns>A task representing the test.</returns>
    [Test]
    public async Task LoggingEventFactory_ReturnsNullForUntranslatableOrEmptyMessagesAndValidatesArguments()
    {
        using var provider = CreateExternalProvider();
        var logger = provider.CreateLogger("factory-category") as Log4NetLogger;
        var factory = new Log4NetLoggingEventFactory();
        var options = new Log4NetProviderOptions { LogLevelTranslator = new Log4NetLogLevelTranslator() };
        var noneCandidate = new MessageCandidate<string>(LogLevel.None, default, MessageText, null, static (state, _) => state);
        var emptyCandidate = new MessageCandidate<string>(LogLevel.Information, default, string.Empty, null, static (state, _) => state);

        await Assert.That(factory.CreateLoggingEvent(in noneCandidate, GetCoreLogger(logger!), options, new LoggerExternalScopeProvider())).IsNull();
        await Assert.That(factory.CreateLoggingEvent(in emptyCandidate, GetCoreLogger(logger!), options, new LoggerExternalScopeProvider())).IsNull();

        var nullOptions = () => factory.CreateLoggingEvent(in noneCandidate, GetCoreLogger(logger!), null!, new LoggerExternalScopeProvider());
        var nullLogger = () => factory.CreateLoggingEvent(in noneCandidate, null!, options, new LoggerExternalScopeProvider());
        await Assert.That(nullOptions).Throws<ArgumentNullException>();
        await Assert.That(nullLogger).Throws<ArgumentNullException>();
    }

    /// <summary>Verifies provider logger and scope behavior.</summary>
    /// <returns>A task representing the test.</returns>
    [Test]
    public async Task Provider_CreatesAndCachesLoggersAndManagesScopes()
    {
        using var provider = CreateExternalProvider();
        var first = provider.CreateLogger("category");
        var second = provider.CreateLogger("category");
        var defaultLogger = provider.CreateLogger();
        var nullScopeLogger = provider.CreateLogger("null-scope") as Log4NetLogger;
        using var nullScope = nullScopeLogger!.BeginScope(ScopeText);
        nullScopeLogger.Log(LogLevel.None, default, MessageText, null, static (state, _) => state);
        var scopeProvider = new LoggerExternalScopeProvider();

        provider.SetScopeProvider(scopeProvider);
        var scopedLogger = provider.CreateLogger("scoped-category") as Log4NetLogger;
        using var scope = scopedLogger!.BeginScope(ScopeText);

        await Assert.That(first).IsEqualTo(second);
        await Assert.That(defaultLogger).IsNotNull();
        await Assert.That(scopedLogger.Name).IsEqualTo("scoped-category");
        await Assert.That(scope).IsNotNull();
    }

    /// <summary>Verifies provider options, files, repositories, and overrides.</summary>
    /// <returns>A task representing the test.</returns>
    [Test]
    public async Task Provider_ValidatesOptionsAndCanConfigureFilesRepositoriesAndOverrides()
    {
        var configPath = CreateConfigurationFile();
        var invalidOptions = new Log4NetProviderOptions(configPath, true);
        invalidOptions.PropertyOverrides.Add(new NodeInfo { XPath = "/log4net/root/level", NodeContent = DebugLevelName });
        var invalid = () => new Log4NetProvider(invalidOptions);
        await Assert.That(invalid).Throws<NotSupportedException>();

        using var configured = new Log4NetProvider(new Log4NetProviderOptions(configPath));
        await Assert.That(configured.CreateLogger(nameof(configured))).IsNotNull();

        var overrideOptions = new Log4NetProviderOptions(configPath) { LoggerRepository = $"override-{Guid.NewGuid():N}", };
        var overridingNode = new NodeInfo { XPath = "/log4net/root/level", NodeContent = DebugLevelName, };
        overridingNode.Attributes[ValueText] = DebugLevelName;
        overridingNode.Attributes["newAttribute"] = "added";
        overrideOptions.PropertyOverrides.Add(overridingNode);
        using var overridden = new Log4NetProvider(overrideOptions);
        await Assert.That(overridden.CreateLogger(nameof(overridden))).IsNotNull();

        var repositoryName = $"existing-{Guid.NewGuid():N}";
        using var firstRepository = new Log4NetProvider(new Log4NetProviderOptions { LoggerRepository = repositoryName, ExternalConfigurationSetup = true, });
        using var existingRepository = new Log4NetProvider(new Log4NetProviderOptions { LoggerRepository = repositoryName, ExternalConfigurationSetup = true, });
        await Assert.That(existingRepository.CreateLogger("existing")).IsNotNull();
    }

    /// <summary>Verifies web configuration, watch, and disposal behavior.</summary>
    /// <returns>A task representing the test.</returns>
    [Test]
    public async Task Provider_HandlesWebConfigurationWatchAndDisposal()
    {
        var configPath = CreateConfigurationFile();
        using var webConfiguration = new Log4NetProvider(new Log4NetProviderOptions { UseWebOrAppConfig = true, });
        await Assert.That(webConfiguration.CreateLogger("web-config")).IsNotNull();

        using var watched = new Log4NetProvider(new Log4NetProviderOptions(configPath, true));
        await Assert.That(watched.CreateLogger(nameof(watched))).IsNotNull();

        var disposable = CreateExternalProvider();
        disposable.Dispose();
        disposable.Dispose();
        await Assert.That(disposable.CreateLogger("disposed")).IsNotNull();
    }

    /// <summary>Verifies logger factories, enabled checks, and formatter validation.</summary>
    /// <returns>A task representing the test.</returns>
    [Test]
    public async Task Logger_UsesFactoriesSupportsAllEnabledChecksAndValidatesFormatter()
    {
        var factory = new CountingLoggingEventFactory();
        using var provider = new Log4NetProvider(new Log4NetProviderOptions(CreateConfigurationFile())
        {
            LoggerRepository = $"logger-{Guid.NewGuid():N}",
            LoggingEventFactory = factory,
            LogLevelTranslator = new Log4NetLogLevelTranslator(),
        });
        var logger = provider.CreateLogger(nameof(Log4NetLogger)) as Log4NetLogger;

        await Assert.That(logger!.IsEnabled(LogLevel.None)).IsFalse();
        var unsupportedLevel = () => logger.IsEnabled((LogLevel)InvalidLogLevelValue);
        await Assert.That(unsupportedLevel).Throws<ArgumentOutOfRangeException>();
        logger.Log(LogLevel.Information, new(EventIdentifier), MessageText, null, static (state, _) => state);
        await Assert.That(factory.CallCount).IsEqualTo(1);

        using var configuredProvider = new Log4NetProvider(new Log4NetProviderOptions(CreateConfigurationFile()) { LoggerRepository = $"event-{Guid.NewGuid():N}", });
        var configuredLogger = configuredProvider.CreateLogger(nameof(Log4NetProvider)) as Log4NetLogger;
        configuredLogger!.Log(LogLevel.Information, new(EventIdentifier), MessageText, null, static (state, _) => state);

        var nullFormatter = () => logger.Log(LogLevel.Information, default, MessageText, null, null!);
        await Assert.That(nullFormatter).Throws<ArgumentNullException>();
    }

    /// <summary>Verifies factory extensions and provider-type validation.</summary>
    /// <returns>A task representing the test.</returns>
    [Test]
    public async Task Extensions_AddProvidersAndValidateProviderTypes()
    {
        var configPath = CreateConfigurationFile();
        using var factory = LoggerFactory.Create(static _ => { });
        await Assert.That(factory.AddLog4Net(new Log4NetProviderOptions(configPath))).IsEqualTo(factory);
        await Assert.That(factory.AddLog4Net(configPath)).IsEqualTo(factory);
        await Assert.That(factory.AddLog4Net(configPath, false)).IsEqualTo(factory);

        using var defaultProvider = new Log4NetProvider();
        using var namedProvider = new Log4NetProvider(configPath);
        await Assert.That(defaultProvider.CreateLogger("default-provider")).IsNotNull();
        await Assert.That(namedProvider.CreateLogger("named-provider")).IsNotNull();

        await Assert.That(factory.AddLog4Net()).IsEqualTo(factory);
        var missingFactory = () => factory.AddLog4Net("missing-log4net.config");
        await Assert.That(missingFactory).Throws<FileNotFoundException>();

        ILoggerFactory? nullFactory = null;
        var nullFactoryAction = () => nullFactory!.AddLog4Net(new Log4NetProviderOptions(configPath));
        await Assert.That(nullFactoryAction).Throws<ArgumentNullException>();

        using var provider = CreateExternalProvider();
        await Assert.That(((ILoggerProvider)provider).CreateLogger(typeof(Log4NetTests))).IsNotNull();
        var nullProvider = static () => ((ILoggerProvider)null!).CreateLogger(typeof(Log4NetTests));
        var nullType = () => ((ILoggerProvider)provider).CreateLogger(null!);
        var wrongProvider = static () => ((ILoggerProvider)new EmptyProvider()).CreateLogger(typeof(Log4NetTests));
        await Assert.That(nullProvider).Throws<ArgumentNullException>();
        await Assert.That(nullType).Throws<ArgumentNullException>();
        await Assert.That(wrongProvider).Throws<ArgumentOutOfRangeException>();
    }

    /// <summary>Verifies logging-builder extensions.</summary>
    /// <returns>A task representing the test.</returns>
    [Test]
    public async Task LoggingBuilderExtensions_RegisterProvidersAndValidateBuilder()
    {
        var configPath = CreateConfigurationFile();
        var services = new ServiceCollection();
        var builder = new TestLoggingBuilder(services);
        await Assert.That(builder.AddLog4Net(new Log4NetProviderOptions(configPath))).IsEqualTo(builder);
        await Assert.That(builder.AddLog4Net(configPath)).IsEqualTo(builder);
        await Assert.That(builder.AddLog4Net(configPath, false)).IsEqualTo(builder);
        await Assert.That(builder.AddLog4Net()).IsEqualTo(builder);
        await Assert.That(CountServices<ILoggerProvider>(services)).IsEqualTo(ExpectedProviderCount + 1);

        ILoggingBuilder? nullBuilder = null;
        var nullBuilderAction = () => nullBuilder!.AddLog4Net(new Log4NetProviderOptions(configPath));
        await Assert.That(nullBuilderAction).Throws<ArgumentNullException>();
    }

    /// <summary>Verifies direct critical and trace log extensions.</summary>
    /// <returns>A task representing the test.</returns>
    [Test]
    public async Task LogExtensions_AcceptCriticalAndTraceMessages()
    {
        var log = LogManager.GetLogger($"extensions-{Guid.NewGuid():N}");
        var exception = new InvalidOperationException("failure");

        log.Critical("critical", exception);
        log.Trace("trace", exception);

        await Assert.That(log).IsNotNull();
    }

    /// <summary>Creates an externally configured provider.</summary>
    /// <param name="loggingEventFactory">An optional event factory.</param>
    /// <returns>The provider.</returns>
    private static Log4NetProvider CreateExternalProvider(ILog4NetLoggingEventFactory? loggingEventFactory = null) =>
        new(new Log4NetProviderOptions
        {
            ExternalConfigurationSetup = true,
            LoggerRepository = $"provider-{Guid.NewGuid():N}",
            LoggingEventFactory = loggingEventFactory,
            LogLevelTranslator = new Log4NetLogLevelTranslator(),
        });

    /// <summary>Gets a core logger for event-factory tests.</summary>
    /// <param name="logger">The adapter logger.</param>
    /// <returns>The core logger.</returns>
    private static log4net.Core.ILogger GetCoreLogger(Log4NetLogger logger) =>
        LogManager.GetLogger(logger.Name).Logger;

    /// <summary>Creates a temporary Log4Net configuration file.</summary>
    /// <returns>The configuration path.</returns>
    private static string CreateConfigurationFile()
    {
        var path = Path.Combine(Path.GetTempPath(), $"log4net-{Guid.NewGuid():N}.config");
        const string configuration = "<log4net><root><level value=\"ALL\" /><appender-ref ref=\"MemoryAppender\" /></root>"
            + "<appender name=\"MemoryAppender\" type=\"log4net.Appender.MemoryAppender\" /></log4net>";
        File.WriteAllText(path, configuration);
        return path;
    }

    /// <summary>Counts services of a specified type.</summary>
    /// <typeparam name="TService">The service type.</typeparam>
    /// <param name="services">The services to inspect.</param>
    /// <returns>The number of matching services.</returns>
    private static int CountServices<TService>(IServiceCollection services)
    {
        var count = 0;
        foreach (var descriptor in services)
        {
            if (descriptor.ServiceType == typeof(TService))
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>Counts calls made by a logger.</summary>
    private sealed class CountingLoggingEventFactory : ILog4NetLoggingEventFactory
    {
        /// <summary>Gets the number of factory calls.</summary>
        public int CallCount { get; private set; }

        /// <summary>Counts an event creation request.</summary>
        /// <typeparam name="TState">The state type.</typeparam>
        /// <param name="messageCandidate">The candidate message.</param>
        /// <param name="logger">The core logger.</param>
        /// <param name="options">The provider options.</param>
        /// <param name="scopeProvider">The scope provider.</param>
        /// <returns>No event.</returns>
        public LoggingEvent? CreateLoggingEvent<TState>(in MessageCandidate<TState> messageCandidate, log4net.Core.ILogger logger, Log4NetProviderOptions options, IExternalScopeProvider scopeProvider)
        {
            CallCount++;
            return null;
        }
    }

    /// <summary>Provides a non-Log4Net provider test double.</summary>
    private sealed class EmptyProvider : ILoggerProvider
    {
        /// <summary>Creates an empty logger.</summary>
        /// <param name="categoryName">The category name.</param>
        /// <returns>An empty logger.</returns>
        public ILogger CreateLogger(string categoryName) => new EmptyLogger();

        /// <summary>Disposes the provider.</summary>
        public void Dispose()
        {
        }
    }

    /// <summary>Provides a no-op logger test double.</summary>
    private sealed class EmptyLogger : ILogger
    {
        /// <summary>Does not create a scope.</summary>
        /// <typeparam name="TState">The scope state type.</typeparam>
        /// <param name="state">The scope state.</param>
        /// <returns>No scope.</returns>
        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        /// <summary>Reports logging as disabled.</summary>
        /// <param name="logLevel">The log level.</param>
        /// <returns>false.</returns>
        public bool IsEnabled(LogLevel logLevel) => false;

        /// <summary>Does not log.</summary>
        /// <typeparam name="TState">The state type.</typeparam>
        /// <param name="logLevel">The log level.</param>
        /// <param name="eventId">The event identifier.</param>
        /// <param name="state">The state.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="formatter">The formatter.</param>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
        }
    }

    /// <summary>Provides a scope string representation.</summary>
    /// <param name="value">The value to render.</param>
    private sealed class ScopeValue(string value)
    {
        public override string ToString() => value;
    }

    /// <summary>Provides an in-memory logging builder.</summary>
    /// <param name="services">The services to expose.</param>
    private sealed class TestLoggingBuilder(IServiceCollection services) : ILoggingBuilder
    {
        /// <summary>Gets registered services.</summary>
        public IServiceCollection Services { get; } = services;
    }
}
