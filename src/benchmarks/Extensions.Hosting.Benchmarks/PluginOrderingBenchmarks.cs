// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileSystemGlobbing;
using ReactiveMarbles.Extensions.Hosting.Plugins;

namespace Extensions.Hosting.Benchmarks;

/// <summary>Measures the plugin filtering and ordering path used during host startup.</summary>
[MemoryDiagnoser]
public class PluginOrderingBenchmarks
{
    /// <summary>The number of plugin order variants in the benchmark fixture.</summary>
    private const int PluginOrderVariants = 2;

    /// <summary>Stores the assemblies passed to the production ordering path.</summary>
    private readonly HashSet<Assembly> _assemblies = [typeof(PluginOrderingBenchmarks).Assembly];

    /// <summary>Stores the configured plugin builder.</summary>
    private BenchmarkPluginBuilder _pluginBuilder = null!;

    /// <summary>Gets or sets the number of discovered plugins to order.</summary>
    [Params(1, 32, 256)]
    public int PluginCount { get; set; }

    /// <summary>Creates the plugin sequence used by each benchmark invocation.</summary>
    [GlobalSetup]
    public void Setup()
    {
        var plugins = new IPlugin?[PluginCount];
        for (var index = 0; index < plugins.Length; index++)
        {
            plugins[index] = index % PluginOrderVariants == 0 ? new EarlyPlugin() : new LatePlugin();
        }

        _pluginBuilder = new() { AssemblyScanFunc = _ => plugins };
    }

    /// <summary>Filters nullable results and orders all discovered plugins.</summary>
    /// <returns>The ordered plugin list.</returns>
    [Benchmark]
    public List<IPlugin> OrderPlugins() =>
        HostBuilderPluginExtensions.GetOrderedPlugins(_pluginBuilder, _assemblies);

    /// <summary>Provides the minimal builder needed by the production ordering path.</summary>
    private sealed class BenchmarkPluginBuilder : IPluginBuilder
    {
        /// <inheritdoc />
        public IList<string> PluginDirectories { get; } = [];

        /// <inheritdoc />
        public IList<string> FrameworkDirectories { get; } = [];

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
        public Func<Assembly, IEnumerable<IPlugin?>?> AssemblyScanFunc { get; set; } = static _ => [];
    }

    /// <summary>Represents a plugin configured to run before the default order.</summary>
    [PluginOrder(-1)]
    private sealed class EarlyPlugin : IPlugin
    {
        /// <inheritdoc />
        public void ConfigureHost(object hostBuilderContext, IServiceCollection serviceCollection)
        {
        }
    }

    /// <summary>Represents a plugin configured to run after the default order.</summary>
    [PluginOrder(1)]
    private sealed class LatePlugin : IPlugin
    {
        /// <inheritdoc />
        public void ConfigureHost(object hostBuilderContext, IServiceCollection serviceCollection)
        {
        }
    }
}
