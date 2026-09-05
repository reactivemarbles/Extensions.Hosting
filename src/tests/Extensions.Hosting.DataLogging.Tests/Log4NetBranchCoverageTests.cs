// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.IO;
using log4net;
using log4net.Appender;
using log4net.Config;

namespace Extensions.Hosting.DataLogging.Tests;

/// <summary>Contains focused Log4Net branch coverage tests.</summary>
public class Log4NetBranchCoverageTests
{
    /// <summary>Provides a log message.</summary>
    private const string MessageText = "message";

    /// <summary>Provides the test event identifier.</summary>
    private const int EventIdentifier = 11;

    /// <summary>Verifies direct logger constructor validation and repository selection branches.</summary>
    /// <returns>A task representing the test.</returns>
    [Test]
    public async Task LoggerConstructor_ValidatesArgumentsAndUsesDefaultRepositoryForWhitespaceRepository()
    {
        var scopeProvider = new LoggerExternalScopeProvider();
        var nullOptions = () => new Log4NetLogger(null!, scopeProvider);
        var nullScopeProvider = static () => new Log4NetLogger(new Log4NetProviderOptions(), null!);

        await Assert.That(nullOptions).Throws<ArgumentNullException>();
        await Assert.That(nullScopeProvider).Throws<ArgumentNullException>();

        var logger = new Log4NetLogger(
            new Log4NetProviderOptions { Name = "direct-default-repository", LoggerRepository = " ", LogLevelTranslator = new Log4NetLogLevelTranslator() },
            scopeProvider);

        await Assert.That(logger.Name).IsEqualTo("direct-default-repository");
    }

    /// <summary>Verifies logger behavior when optional factories and translators are absent.</summary>
    /// <returns>A task representing the test.</returns>
    [Test]
    public async Task Logger_HandlesMissingTranslatorAndMissingEventFactory()
    {
        var missingTranslatorLogger = new Log4NetLogger(
            new Log4NetProviderOptions { Name = "missing-translator" },
            new LoggerExternalScopeProvider());

        await Assert.That(missingTranslatorLogger.IsEnabled(LogLevel.None)).IsFalse();
        var unsupportedLevel = () => missingTranslatorLogger.IsEnabled(LogLevel.Information);
        await Assert.That(unsupportedLevel).Throws<ArgumentOutOfRangeException>();

        var missingFactoryLogger = new Log4NetLogger(
            new Log4NetProviderOptions { Name = "missing-factory", LoggerRepository = CreateEnabledRepositoryName(), LogLevelTranslator = new EnabledLevelTranslator() },
            new LoggerExternalScopeProvider());

        missingFactoryLogger.Log(LogLevel.Information, new(EventIdentifier), MessageText, null, static (state, _) => state);

        await Assert.That(missingFactoryLogger.Name).IsEqualTo("missing-factory");
    }

    /// <summary>Verifies logging-event factory behavior for optional collaborators and empty exception messages.</summary>
    /// <returns>A task representing the test.</returns>
    [Test]
    public async Task LoggingEventFactory_HandlesMissingTranslatorNullScopeProviderAndExceptionOnlyMessages()
    {
        var factory = new Log4NetLoggingEventFactory();
        var logger = LogManager.GetLogger($"event-factory-{Guid.NewGuid():N}").Logger;
        var candidate = new MessageCandidate<string>(
            LogLevel.Information,
            new(EventIdentifier),
            string.Empty,
            new InvalidOperationException("failure"),
            static (state, _) => state);
        var missingTranslator = new Log4NetProviderOptions();
        var configured = new Log4NetProviderOptions { LogLevelTranslator = new Log4NetLogLevelTranslator() };

        await Assert.That(factory.CreateLoggingEvent(in candidate, logger, missingTranslator, new LoggerExternalScopeProvider())).IsNull();

        var loggingEvent = factory.CreateLoggingEvent(in candidate, logger, configured, null!);

        await Assert.That(loggingEvent).IsNotNull();
        await Assert.That(loggingEvent!.ExceptionObject).IsTypeOf<InvalidOperationException>();
        await Assert.That(loggingEvent.RenderedMessage).IsEqualTo(string.Empty);
    }

    /// <summary>Verifies provider validation and default option fallback branches.</summary>
    /// <returns>A task representing the test.</returns>
    [Test]
    public async Task Provider_ValidatesNullOptionsAndUsesDefaultFallbacks()
    {
        var nullOptions = static () => new Log4NetProvider((Log4NetProviderOptions)null!);
        await Assert.That(nullOptions).Throws<ArgumentNullException>();

        var options = new Log4NetProviderOptions { Name = "configured-provider-name", ExternalConfigurationSetup = true, ConfigurationAssembly = typeof(Log4NetBranchCoverageTests).Assembly };
        using var namedProvider = new Log4NetProvider(options);

        var namedLogger = namedProvider.CreateLogger();
        await Assert.That(namedLogger).IsNotNull();

        using var defaultFallbacksProvider = new Log4NetProvider(new Log4NetProviderOptions { ExternalConfigurationSetup = true, LoggerRepository = $"fallbacks-{Guid.NewGuid():N}" });

        var defaultFallbacksLogger = defaultFallbacksProvider.CreateLogger("fallbacks") as Log4NetLogger;

        await Assert.That(defaultFallbacksLogger).IsNotNull();
        await Assert.That(defaultFallbacksLogger!.IsEnabled(LogLevel.None)).IsFalse();
    }

    /// <summary>Verifies provider configuration and disposal branches.</summary>
    /// <returns>A task representing the test.</returns>
    [Test]
    public async Task Provider_HandlesRelativeConfigurationPathsAndNonDisposingDispose()
    {
        using var relativePathProvider = new Log4NetProvider(new Log4NetProviderOptions("log4net.config") { LoggerRepository = $"relative-{Guid.NewGuid():N}" });

        await Assert.That(relativePathProvider.CreateLogger("relative")).IsNotNull();

        using var provider = new ExposedDisposeProvider();
        provider.DisposeWithoutManagedCleanup();

        await Assert.That(provider.CreateLogger("after-non-disposing")).IsNotNull();
    }

    /// <summary>Verifies provider extension validation for null category types.</summary>
    /// <returns>A task representing the test.</returns>
    [Test]
    public async Task ProviderExtension_ValidatesNullCategoryType()
    {
        using var provider = new Log4NetProvider(new Log4NetProviderOptions { ExternalConfigurationSetup = true, LoggerRepository = $"provider-extension-{Guid.NewGuid():N}" });

        Type? nameType = null;
        var nullType = () => ((ILoggerProvider)provider).CreateLogger(nameType!);

        await Assert.That(nullType).Throws<ArgumentNullException>();
    }

    /// <summary>Verifies legacy null option values retain their documented fallback behavior.</summary>
    /// <returns>A task representing the test.</returns>
    [Test]
    public async Task Provider_NullOptionValues_UseEmptyFallbacks()
    {
        var options = new Log4NetProviderOptions { ExternalConfigurationSetup = true, Name = null!, OverrideCriticalLevelWith = null!, LoggerRepository = $"null-options-{Guid.NewGuid():N}" };
        using var provider = new Log4NetProvider(options);

        var logger = (Log4NetLogger)provider.CreateLogger();

        await Assert.That(logger.Name).IsEqualTo(string.Empty);
        await Assert.That(logger.IsEnabled(LogLevel.None)).IsFalse();
    }

    /// <summary>Verifies a missing configuration file name cannot be opened as a configuration file.</summary>
    /// <returns>A task representing the test.</returns>
    [Test]
    public async Task Provider_NullConfigurationFileName_RejectsDirectory()
    {
        var options = new Log4NetProviderOptions(null!) { LoggerRepository = $"null-file-{Guid.NewGuid():N}" };

        await Assert.That(() => new Log4NetProvider(options)).Throws<DirectoryNotFoundException>();
    }

    /// <summary>Creates a Log4Net repository that accepts log events for direct logger tests.</summary>
    /// <returns>The configured repository name.</returns>
    private static string CreateEnabledRepositoryName()
    {
        var repositoryName = $"enabled-{Guid.NewGuid():N}";
        var repository = LogManager.CreateRepository(repositoryName);
        _ = BasicConfigurator.Configure(repository, new MemoryAppender());

        return repositoryName;
    }

    /// <summary>Translates every log level to an enabled Log4Net level.</summary>
    private sealed class EnabledLevelTranslator : ILog4NetLogLevelTranslator
    {
        /// <inheritdoc/>
        public log4net.Core.Level? TranslateLogLevel(LogLevel logLevel, Log4NetProviderOptions options)
        {
            _ = logLevel;
            _ = options;

            return log4net.Core.Level.Info;
        }
    }

    /// <summary>Exposes non-disposing cleanup for protected disposal branch coverage.</summary>
    private sealed class ExposedDisposeProvider : Log4NetProvider
    {
        /// <summary>Initializes a new instance of the <see cref="ExposedDisposeProvider"/> class.</summary>
        public ExposedDisposeProvider()
            : base(new Log4NetProviderOptions { ExternalConfigurationSetup = true, LoggerRepository = $"dispose-{Guid.NewGuid():N}" })
        {
        }

        /// <summary>Invokes the non-disposing cleanup path.</summary>
        public void DisposeWithoutManagedCleanup() => Dispose(false);
    }
}
