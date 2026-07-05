// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using Microsoft.Extensions.FileSystemGlobbing;
using ReactiveMarbles.Extensions.Hosting.Reactive.Plugins;

namespace Extensions.Hosting.Tests;

/// <summary>Test implementation of the reactive shim IPluginBuilder for unit testing.</summary>
public class ReactiveTestPluginBuilder : IPluginBuilder
{
    /// <inheritdoc />
    public IList<string> PluginDirectories { get; } = new List<string>();

    /// <inheritdoc />
    public IList<string> FrameworkDirectories { get; } = new List<string>();

    /// <inheritdoc />
    public bool UseContentRoot { get; set; }

    /// <inheritdoc />
    public bool FailIfNoPlugins { get; set; }

    /// <inheritdoc />
    public Matcher FrameworkMatcher { get; } = new Matcher();

    /// <inheritdoc />
    public Matcher PluginMatcher { get; } = new Matcher();

    /// <inheritdoc />
    public Func<string, bool> ValidatePlugin { get; set; } = _ => true;

    /// <inheritdoc />
    public Func<Assembly, IEnumerable<IPlugin?>?> AssemblyScanFunc { get; set; } = PluginScanner.ScanForPluginInstances;
}
