// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.IO;

namespace ReactiveMarbles.Extensions.Hosting.Plugins;

/// <summary>
/// Plugin Builder Extensions.
/// </summary>
public static class PluginBuilderExtensions
{
    /// <summary>
    /// Add a directory to scan both framework and plug-in assemblies.
    /// </summary>
    /// <param name="pluginBuilder">IPluginBuilder.</param>
    /// <param name="directories">string array.</param>
    public static void AddScanDirectories(this IPluginBuilder pluginBuilder, params string[] directories)
    {
        if (directories == null)
        {
            throw new ArgumentNullException(nameof(directories));
        }

        foreach (var directory in directories)
        {
            var normalizedDirectory = Path.GetFullPath(directory);
            pluginBuilder?.FrameworkDirectories.Add(normalizedDirectory);
            pluginBuilder?.PluginDirectories.Add(normalizedDirectory);
        }
    }

    /// <summary>
    /// Exclude globs to look for framework assemblies.
    /// </summary>
    /// <param name="pluginBuilder">IPluginBuilder.</param>
    /// <param name="frameworkGlobs">string array.</param>
    public static void ExcludeFrameworks(this IPluginBuilder pluginBuilder, params string[] frameworkGlobs)
    {
        if (frameworkGlobs == null)
        {
            throw new ArgumentNullException(nameof(frameworkGlobs));
        }

        foreach (var glob in frameworkGlobs)
        {
            pluginBuilder?.FrameworkMatcher.AddExclude(glob);
        }
    }

    /// <summary>
    /// Exclude globs to look for plug-in assemblies.
    /// </summary>
    /// <param name="pluginBuilder">IPluginBuilder.</param>
    /// <param name="pluginGlobs">string array.</param>
    public static void ExcludePlugins(this IPluginBuilder pluginBuilder, params string[] pluginGlobs)
    {
        if (pluginGlobs == null)
        {
            throw new ArgumentNullException(nameof(pluginGlobs));
        }

        foreach (var glob in pluginGlobs)
        {
            pluginBuilder?.PluginMatcher.AddExclude(glob);
        }
    }

    /// <summary>
    /// Include globs to look for framework assemblies.
    /// </summary>
    /// <param name="pluginBuilder">IPluginBuilder.</param>
    /// <param name="frameworkGlobs">string array.</param>
    public static void IncludeFrameworks(this IPluginBuilder pluginBuilder, params string[] frameworkGlobs)
    {
        if (frameworkGlobs == null)
        {
            throw new ArgumentNullException(nameof(frameworkGlobs));
        }

        foreach (var glob in frameworkGlobs)
        {
            pluginBuilder?.FrameworkMatcher.AddInclude(glob);
        }
    }

    /// <summary>
    /// Include globs to look for plugin assemblies.
    /// </summary>
    /// <param name="pluginBuilder">IPluginBuilder.</param>
    /// <param name="pluginGlobs">string array.</param>
    public static void IncludePlugins(this IPluginBuilder pluginBuilder, params string[] pluginGlobs)
    {
        if (pluginGlobs == null)
        {
            throw new ArgumentNullException(nameof(pluginGlobs));
        }

        foreach (var glob in pluginGlobs)
        {
            pluginBuilder?.PluginMatcher.AddInclude(glob);
        }
    }

    /// <summary>
    /// Require at least one plugin to be discovered; throw during startup if none are found.
    /// </summary>
    /// <param name="pluginBuilder">IPluginBuilder instance.</param>
    /// <param name="failIfNone">If true, startup throws when no plugins are found. Defaults to true.</param>
    public static void RequirePlugins(this IPluginBuilder pluginBuilder, bool failIfNone = true)
    {
        if (pluginBuilder == null)
        {
            throw new ArgumentNullException(nameof(pluginBuilder));
        }

        pluginBuilder.FailIfNoPlugins = failIfNone;
    }
}
