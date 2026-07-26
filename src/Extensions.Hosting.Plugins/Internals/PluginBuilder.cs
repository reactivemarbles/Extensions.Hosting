// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.FileSystemGlobbing;

#if REACTIVE_SHIM
namespace ReactiveMarbles.Extensions.Hosting.Reactive.Plugins.Internals;
#else
namespace ReactiveMarbles.Extensions.Hosting.Plugins.Internals;
#endif

/// <summary>Provides configuration and matching logic for plugin discovery and loading within the application.</summary>
/// <remarks>The PluginBuilder class exposes properties and delegates that allow customization of plugin scanning,
/// validation, and directory management. It is typically used to configure how plugins are identified and loaded at
/// runtime. This class is intended for internal use and is not designed for direct consumption by external
/// code.</remarks>
internal sealed class PluginBuilder : IPluginBuilder
{
    /// <inheritdoc />
    public Matcher FrameworkMatcher { get; } = new();

    /// <inheritdoc />
    public Matcher PluginMatcher { get; } = new();

    /// <inheritdoc />
    public Func<string, bool> ValidatePlugin { get; set; } = static _ => true;

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
