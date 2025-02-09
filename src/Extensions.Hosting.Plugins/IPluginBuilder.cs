// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.FileSystemGlobbing;

namespace ReactiveMarbles.Extensions.Hosting.Plugins;

/// <summary>
/// The plug-in builder is used to configure the plug-in loading.
/// </summary>
public interface IPluginBuilder
{
    /// <summary>
    /// Gets in these directories we will scan for plug-ins.
    /// </summary>
    IList<string> PluginDirectories { get; }

    /// <summary>
    /// Gets in these directories we will scan for framework assemblies.
    /// </summary>
    IList<string> FrameworkDirectories { get; }

    /// <summary>
    /// Gets or sets a value indicating whether specify to use the content root for scanning.
    /// </summary>
    bool UseContentRoot { get; set; }

    /// <summary>
    /// Gets the matcher used to find all the framework assemblies.
    /// </summary>
    Matcher FrameworkMatcher { get; }

    /// <summary>
    /// Gets the matcher to find all the plugins.
    /// </summary>
    Matcher PluginMatcher { get; }

    /// <summary>
    /// Gets or sets specifies a way to validate the plugin file before it's being loaded.
    /// </summary>
    Func<string, bool> ValidatePlugin { get; set; }

    /// <summary>
    /// Gets or sets specify the Assembly scan function, which takes the Assembly and returns the IPlugin(s) for it.
    /// Available functions are:
    /// PluginScanner.ByNamingConvention which is fast, but finds only one IPlugin by convention
    /// PluginScanner.ScanForPluginInstances which is the default and finds all public classes implementing IPlugin.
    /// </summary>
    Func<Assembly, IEnumerable<IPlugin?>?> AssemblyScanFunc { get; set; }
}
