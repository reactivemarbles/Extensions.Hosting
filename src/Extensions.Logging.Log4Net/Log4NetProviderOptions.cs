// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Logging;
using ReactiveMarbles.Extensions.Logging.Log4Net.Entities;

namespace ReactiveMarbles.Extensions.Logging;

/// <summary>
/// The log4Net provider options.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="Log4NetProviderOptions"/> class with the specified configuration file name and.
/// watch setting.
/// </remarks>
/// <param name="log4NetConfigFileName">The path to the log4net configuration file to be used for logger setup. Cannot be null or empty.</param>
/// <param name="watch">true to enable watching the configuration file for changes; otherwise, false.</param>
public sealed class Log4NetProviderOptions(string log4NetConfigFileName, bool watch)
{
    /// <summary>
    /// The default log4 net file name.
    /// </summary>
    private const string DefaultLog4NetFileName = "log4net.config";

    /// <summary>
    /// Initializes a new instance of the <see cref="Log4NetProviderOptions"/> class.
    /// </summary>
    public Log4NetProviderOptions()
        : this(DefaultLog4NetFileName)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Log4NetProviderOptions"/> class.
    /// </summary>
    /// <param name="log4NetConfigFileName">Name of the log4 net configuration file.</param>
    public Log4NetProviderOptions(string log4NetConfigFileName)
        : this(log4NetConfigFileName, false)
    {
    }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the log file.
    /// </summary>
    public string Log4NetConfigFileName { get; set; } = log4NetConfigFileName;

    /// <summary>
    /// Gets or sets the logger repository.
    /// </summary>
    public string? LoggerRepository { get; set; }

    /// <summary>
    /// Gets or sets the level value that should be used to override default's critical level.
    /// </summary>
    public string OverrideCriticalLevelWith { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the property overrides.
    /// </summary>
    public List<NodeInfo> PropertyOverrides { get; set; } = new List<NodeInfo>();

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="Log4NetProviderOptions"/> is watch.
    /// </summary>
    public bool Watch { get; set; } = watch;

    /// <summary>
    /// Gets or sets a value indicating whether let user setup log4net externally.
    /// </summary>
    public bool ExternalConfigurationSetup { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether let user setup log4net from web.config / app.config.
    /// </summary>
    public bool UseWebOrAppConfig { get; set; }

    /// <summary>
    /// Gets or sets the assembly used to create the log4net repository identity when <see cref="LoggerRepository"/> is not provided.
    /// </summary>
    public Assembly? ConfigurationAssembly { get; set; }

    /// <summary>
    /// Gets or sets the factory for the log4net <see cref="log4net.Core.LoggingEvent"/>.
    /// </summary>
    public ILog4NetLoggingEventFactory? LoggingEventFactory { get; set; }

    /// <summary>
    /// Gets or sets the translator between the <see cref="LogLevel"/> and the log4net <see cref="log4net.Core.Level"/>.
    /// </summary>
    public ILog4NetLogLevelTranslator? LogLevelTranslator { get; set; }
}
