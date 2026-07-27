// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
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
    public Matcher FrameworkMatcher { get; } = new();

    /// <inheritdoc />
    public Matcher PluginMatcher { get; } = new();

    /// <inheritdoc />
    public Func<string, bool> ValidatePlugin { get; set; } = static _ => true;

    /// <inheritdoc />
    public Func<Assembly, IEnumerable<IPlugin?>?> AssemblyScanFunc { get; set; } = PluginScanner.ScanForPluginInstances;
}
