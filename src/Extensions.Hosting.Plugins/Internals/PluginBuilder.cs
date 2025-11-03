// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.FileSystemGlobbing;

namespace ReactiveMarbles.Extensions.Hosting.Plugins.Internals;

/// <summary>
/// This is the implementation of the plugin builder.
/// </summary>
internal class PluginBuilder : IPluginBuilder
{
    /// <inheritdoc />
    public Matcher FrameworkMatcher { get; } = new Matcher();

    /// <inheritdoc />
    public Matcher PluginMatcher { get; } = new Matcher();

    /// <inheritdoc />
    public Func<string, bool> ValidatePlugin { get; set; } = _ => true;

    /// <inheritdoc />
    public Func<Assembly, IEnumerable<IPlugin?>?> AssemblyScanFunc { get; set; } = PluginScanner.ScanForPluginInstances;

    /// <inheritdoc />
    public IList<string> PluginDirectories { get; } = [];

    /// <inheritdoc />
    public IList<string> FrameworkDirectories { get; } = [];

    /// <inheritdoc />
    public bool UseContentRoot { get; set; }

    /// <inheritdoc />
    public bool FailIfNoPlugins { get; set; }
}
