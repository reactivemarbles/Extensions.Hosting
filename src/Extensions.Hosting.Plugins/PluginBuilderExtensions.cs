// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.IO;

namespace ReactiveMarbles.Extensions.Hosting.Plugins;

/// <summary>
/// Provides extension methods for configuring plug-in and framework assembly scanning behavior on an <see
/// cref="IPluginBuilder"/> instance.
/// </summary>
/// <remarks>These extension methods allow customization of directory scanning, inclusion and exclusion patterns
/// for assemblies, and enforcement of plug-in discovery requirements during application startup. Use these methods to
/// control which assemblies are considered as plug-ins or framework components when building a plug-in
/// system.</remarks>
public static class PluginBuilderExtensions
{
    /// <summary>
    /// Adds one or more directories to the plugin and framework scan paths for the specified plugin builder.
    /// </summary>
    /// <remarks>Each directory specified is added to both the framework and plugin directory collections of
    /// the plugin builder. Duplicate or non-existent directories are not checked by this method.</remarks>
    /// <param name="pluginBuilder">The plugin builder to which the scan directories will be added.</param>
    /// <param name="directories">An array of directory paths to add to the scan paths. Each path is normalized to its full path before being
    /// added.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="directories"/> is null.</exception>
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
    /// Excludes plug-in targets that match the specified framework glob patterns from the plug-in builder
    /// configuration.
    /// </summary>
    /// <param name="pluginBuilder">The plug-in builder to configure with framework exclusions.</param>
    /// <param name="frameworkGlobs">One or more glob patterns representing frameworks to exclude. Each pattern is applied to filter out matching
    /// frameworks from the plug-in targets.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="frameworkGlobs"/> is <see langword="null"/>.</exception>
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
    /// Excludes plugins from discovery by specifying one or more glob patterns.
    /// </summary>
    /// <remarks>Use this method to prevent specific plugins from being loaded by matching their names against
    /// the provided glob patterns. This is useful for filtering out plugins that should not be included in the
    /// application.</remarks>
    /// <param name="pluginBuilder">The plugin builder to configure with the exclusion patterns.</param>
    /// <param name="pluginGlobs">An array of glob patterns that identify plugins to exclude. Each pattern is applied to plugin names during
    /// discovery.</param>
    /// <exception cref="ArgumentNullException">Thrown if pluginGlobs is null.</exception>
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
    /// Includes the specified framework glob patterns in the plugin builder's framework matcher, allowing only matching
    /// frameworks to be considered during plugin resolution.
    /// </summary>
    /// <remarks>Use this method to restrict plugin resolution to specific frameworks by providing one or more
    /// glob patterns. This is useful when targeting a subset of supported frameworks in multi-targeted plugin
    /// scenarios.</remarks>
    /// <param name="pluginBuilder">The plugin builder to configure with the specified framework include patterns.</param>
    /// <param name="frameworkGlobs">An array of glob patterns that specify which frameworks to include. Each pattern is added to the framework
    /// matcher as an inclusion rule.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="frameworkGlobs"/> is <see langword="null"/>.</exception>
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
    /// Adds one or more plugin glob patterns to the plugin builder's include list.
    /// </summary>
    /// <remarks>Each glob pattern in <paramref name="pluginGlobs"/> is added to the plugin builder's include
    /// matcher. This method can be called multiple times to add additional patterns.</remarks>
    /// <param name="pluginBuilder">The plugin builder to which the include glob patterns will be added.</param>
    /// <param name="pluginGlobs">An array of glob patterns specifying which plugins to include. Cannot be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="pluginGlobs"/> is null.</exception>
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
    /// Configures the plugin builder to require at least one plugin, optionally failing if no plugins are registered.
    /// </summary>
    /// <param name="pluginBuilder">The plugin builder to configure. Cannot be null.</param>
    /// <param name="failIfNone">true to throw an exception if no plugins are registered; otherwise, false. The default is true.</param>
    /// <exception cref="ArgumentNullException">Thrown if pluginBuilder is null.</exception>
    public static void RequirePlugins(this IPluginBuilder pluginBuilder, bool failIfNone = true)
    {
        if (pluginBuilder == null)
        {
            throw new ArgumentNullException(nameof(pluginBuilder));
        }

        pluginBuilder.FailIfNoPlugins = failIfNone;
    }
}
