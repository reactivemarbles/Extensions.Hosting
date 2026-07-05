// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Extensions.Logging;
using ReactiveMarbles.Extensions.Logging.Log4Net.Entities;
using ReactiveMarbles.Extensions.Logging.Log4Net.Extensions;
using ReactiveMarbles.Extensions.Logging.Log4Net.Scope;
using Hierarchy = log4net.Repository.Hierarchy.Hierarchy;
using ILoggerRepository = log4net.Repository.ILoggerRepository;
using LogException = log4net.Core.LogException;
using LogManager = log4net.LogManager;
using XmlConfigurator = log4net.Config.XmlConfigurator;

namespace ReactiveMarbles.Extensions.Logging;

/// <summary>The log4net provider class.</summary>
/// <seealso cref="ILoggerProvider" />
public class Log4NetProvider : ILoggerProvider, ISupportExternalScope
{
    /// <summary>The loggers collection.</summary>
    private readonly ConcurrentDictionary<string, Log4NetLogger> _loggers = new();

    /// <summary>Prevents to dispose the object more than single time.</summary>
    private bool _disposedValue;

    /// <summary>The log4net repository.</summary>
    private ILoggerRepository? _loggerRepository;

    /// <summary>The provider options.</summary>
    private Log4NetProviderOptions? _options;

    /// <summary>Initializes a new instance of the <see cref="Log4NetProvider"/> class.</summary>
    public Log4NetProvider()
        : this(new Log4NetProviderOptions())
    {
    }

    /// <summary>Initializes a new instance of the <see cref="Log4NetProvider"/> class.</summary>
    /// <param name="log4NetConfigFileName">The log4NetConfigFile.</param>
    public Log4NetProvider(string log4NetConfigFileName)
        : this(new Log4NetProviderOptions(log4NetConfigFileName))
    {
    }

    /// <summary>Initializes a new instance of the <see cref="Log4NetProvider"/> class.</summary>
    /// <param name="options">The options.</param>
    /// <exception cref="ArgumentNullException">options.</exception>
    /// <exception cref="NotSupportedException">Wach cannot be true when you are overwriting config file values with values from configuration section.</exception>
    public Log4NetProvider(Log4NetProviderOptions options)
    {
        SetOptionsIfValid(options);

        var loggingAssembly = GetLoggingReferenceAssembly();

        CreateLoggerRepository(loggingAssembly);
        ConfigureLog4NetLibrary();
    }

    /// <summary>Finalizes an instance of the <see cref="Log4NetProvider"/> class.</summary>
    ~Log4NetProvider()
    {
        Dispose(false);
    }

    /// <summary>Gets the external logging scope provider.</summary>
    /// <remarks>
    /// Reading the offical logging implementations, it seems like we need to handle the case that this might never be set.
    /// We handle it with a NullScopeProvider instead of null checks, to make the process of implementing interfaces like
    /// <see cref="ILog4NetLoggingEventFactory"/> less error prone for consumers.
    /// </remarks>
    public IExternalScopeProvider ExternalScopeProvider { get; private set; } = NullScopeProvider.Instance;

    /// <summary>Creates the logger.</summary>
    /// <returns>An instance of the <see cref="ILogger"/>.</returns>
    public ILogger CreateLogger()
        => CreateLogger(_options?.Name ?? string.Empty);

    /// <summary>Creates the logger.</summary>
    /// <param name="categoryName">The category name.</param>
    /// <returns>An instance of the <see cref="ILogger"/>.</returns>
    public ILogger CreateLogger(string categoryName)
        => _loggers.GetOrAdd(categoryName, CreateLoggerImplementation);

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>Sets the external scope provider to be used for logging operations.</summary>
    /// <remarks>Use this method to configure how scope information is managed and included in log messages.
    /// Setting the scope provider to null disables external scoping support.</remarks>
    /// <param name="scopeProvider">The external scope provider that supplies scope information for log entries. Can be null to disable external
    /// scoping.</param>
    public void SetScopeProvider(IExternalScopeProvider scopeProvider) => ExternalScopeProvider = scopeProvider;

    /// <summary>Releases unmanaged and - optionally - managed resources.</summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposedValue)
        {
            return;
        }

        if (disposing)
        {
            _loggerRepository?.Shutdown();
            _loggers.Clear();
        }

        _disposedValue = true;
    }

    /// <summary>Updates configuration nodes overriding values if required.</summary>
    /// <param name="configXmlDocument">The configuration file XML document.</param>
    /// <param name="overridingNodes">The overriding values available.</param>
    /// <returns>An <see cref="XmlDocument"/> within the overriding values replaced.</returns>
    private static XmlDocument UpdateNodesWithOverridingValues(XmlDocument configXmlDocument, IEnumerable<NodeInfo> overridingNodes)
    {
        if (overridingNodes is null)
        {
            return configXmlDocument;
        }

        var configDocument = configXmlDocument.ToXDocument();
        foreach (var nodeInfo in overridingNodes)
        {
            var node = configDocument.XPathSelectElement(nodeInfo.XPath!);
            if (node is null)
            {
                continue;
            }

            if (nodeInfo.NodeContent is not null)
            {
                node.Value = nodeInfo.NodeContent;
            }

            AddOrUpdateAttributes(node, nodeInfo);
        }

        return configDocument.ToXmlDocument();
    }

    /// <summary>Adds or updates the attributes specified in the node information.</summary>
    /// <param name="node">The node.</param>
    /// <param name="nodeInfo">The node information.</param>
    private static void AddOrUpdateAttributes(XElement node, NodeInfo nodeInfo)
    {
        if (nodeInfo.Attributes is null)
        {
            return;
        }

        foreach (var attribute in nodeInfo.Attributes)
        {
            var nodeAttribute = node.Attributes()
                .FirstOrDefault(a => a.Name.LocalName.Equals(attribute.Key, StringComparison.OrdinalIgnoreCase));
            if (nodeAttribute is not null)
            {
                nodeAttribute.Value = attribute.Value;
                continue;
            }

            node.SetAttributeValue(attribute.Key, attribute.Value);
        }
    }

    /// <summary>Parses log4net config file.</summary>
    /// <param name="filename">The filename.</param>
    /// <returns>The <see cref="XmlElement"/> with the log4net XML element.</returns>
    private static XmlDocument ParseLog4NetConfigFile(string filename)
    {
        using var stream = File.OpenRead(filename);
        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
        };

        var log4netConfig = new XmlDocument
        {
            XmlResolver = null,
        };
        using var reader = XmlReader.Create(stream, settings);
        log4netConfig.Load(reader);
        return log4netConfig;
    }

    /// <summary>Creates the logger implementation.</summary>
    /// <param name="name">The name.</param>
    /// <returns>The <see cref="Log4NetLogger"/> instance.</returns>
    private Log4NetLogger CreateLoggerImplementation(string name)
    {
        var loggerOptions = new Log4NetProviderOptions
        {
            Name = name,
            LoggerRepository = _loggerRepository?.Name,
            OverrideCriticalLevelWith = _options?.OverrideCriticalLevelWith ?? string.Empty,
            LoggingEventFactory = _options?.LoggingEventFactory ?? new Log4NetLoggingEventFactory(),
            LogLevelTranslator = _options?.LogLevelTranslator ?? new Log4NetLogLevelTranslator(),
        };

        return new Log4NetLogger(loggerOptions, ExternalScopeProvider);
    }

    /// <summary>Gets the current executing assembly considering the target framework.</summary>
    /// <returns>The assembly to be used as the reference logging assembly.</returns>
    private Assembly GetLoggingReferenceAssembly() => _options?.ConfigurationAssembly ?? Assembly.GetExecutingAssembly();

    /// <summary>Ensures that provided options combinations are valid, and sets the class field if everything is ok.</summary>
    /// <param name="options">The options to validate.</param>
    /// <exception cref="NotSupportedException">
    /// Throws when the Watch option is set and there are properties to override.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Throws when the options parameter is null.
    /// </exception>
    private void SetOptionsIfValid(Log4NetProviderOptions options)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        if (options.Watch
            && options.PropertyOverrides.Count > 0)
        {
            throw new NotSupportedException("Wach cannot be true when you are overwriting config file values with values from configuration section.");
        }

        _options = options;
    }

    /// <summary>Configures the log4net library using the available configuration data.</summary>
    private void ConfigureLog4NetLibrary()
    {
        if (_options?.UseWebOrAppConfig == true)
        {
            _ = XmlConfigurator.Configure(_loggerRepository!);
            return;
        }

        if (_options?.ExternalConfigurationSetup != false)
        {
            return;
        }

        var fileNamePath = CreateLog4NetFilePath();
        if (_options.Watch)
        {
            _ = XmlConfigurator.ConfigureAndWatch(_loggerRepository!, new FileInfo(fileNamePath));
            return;
        }

        var configXml = ParseLog4NetConfigFile(fileNamePath);
        if (_options.PropertyOverrides.Count > 0)
        {
            configXml = UpdateNodesWithOverridingValues(configXml, _options.PropertyOverrides);
        }

        _ = XmlConfigurator.Configure(_loggerRepository!, configXml.DocumentElement!);
    }

    /// <summary>Creates the log4net.config file path.</summary>
    /// <returns>The full path to the log4net.config file.</returns>
    private string CreateLog4NetFilePath()
    {
        var fileNamePath = _options?.Log4NetConfigFileName ?? string.Empty;
        if (!Path.IsPathRooted(fileNamePath))
        {
            fileNamePath = Path.Combine(AppContext.BaseDirectory, fileNamePath);
        }

        return Path.GetFullPath(fileNamePath);
    }

    /// <summary>Gets or creates the logger repository using the given assembly.</summary>
    /// <param name="assembly">The assembly to be used to create the repository.</param>
    private void CreateLoggerRepository(Assembly assembly)
    {
        var repositoryType = typeof(Hierarchy);
        var repositoryName = _options!.LoggerRepository;

        if (string.IsNullOrEmpty(repositoryName))
        {
            _loggerRepository = LogManager.CreateRepository(assembly, repositoryType);
            return;
        }

        if (TryUseExistingRepository(repositoryName!))
        {
            return;
        }

        _loggerRepository ??= LogManager.CreateRepository(repositoryName!, repositoryType);
    }

    /// <summary>Attempts to use an existing log4net repository for the configured repository name.</summary>
    /// <param name="repositoryName">The repository name to resolve.</param>
    /// <returns>true when the repository was found and external configuration means no further setup is required; otherwise, false.</returns>
    private bool TryUseExistingRepository(string repositoryName)
    {
        try
        {
            _loggerRepository = LogManager.GetRepository(repositoryName);
            return _options!.ExternalConfigurationSetup;
        }
        catch (LogException)
        {
            _loggerRepository = null;
            return false;
        }
    }
}
