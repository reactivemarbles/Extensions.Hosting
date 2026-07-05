// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.IO;
using System.Reflection;
using ReactiveMarbles.Extensions.Hosting.Plugins.Internals;
using PluginAssemblyDependencyResolver = ReactiveMarbles.Extensions.Hosting.Plugins.Internals.AssemblyDependencyResolver;
using PluginAssemblyLoadContext = ReactiveMarbles.Extensions.Hosting.Plugins.Internals.AssemblyLoadContext;

namespace Extensions.Hosting.Tests;

/// <summary>Contains tests for plugin assembly loading helper types.</summary>
public class AssemblyLoadingTests
{
    /// <summary>Verifies that the dependency resolver resolves existing managed assembly paths.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveAssemblyToPath_WithExistingAssembly_ReturnsPath()
    {
        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var pluginPath = Path.Combine(tempDirectory, "Plugin.dll");
            var dependencyPath = Path.Combine(tempDirectory, "Dependency.dll");
            await File.WriteAllTextAsync(pluginPath, string.Empty);
            await File.WriteAllTextAsync(dependencyPath, string.Empty);
            var resolver = new PluginAssemblyDependencyResolver(pluginPath);

            var resolvedPath = resolver.ResolveAssemblyToPath(new AssemblyName("Dependency"));

            await Assert.That(resolvedPath).IsEqualTo(dependencyPath);
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

    /// <summary>Verifies that the dependency resolver returns null when a managed assembly does not exist.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveAssemblyToPath_WithMissingAssembly_ReturnsNull()
    {
        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var pluginPath = Path.Combine(tempDirectory, "Plugin.dll");
            await File.WriteAllTextAsync(pluginPath, string.Empty);
            var resolver = new PluginAssemblyDependencyResolver(pluginPath);

            var resolvedPath = resolver.ResolveAssemblyToPath(new AssemblyName("Missing.Dependency"));

            await Assert.That(resolvedPath).IsNull();
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

    /// <summary>Verifies that the dependency resolver throws when the assembly name is null.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveAssemblyToPath_WithNullAssemblyName_ThrowsArgumentNullException()
    {
        var resolver = new PluginAssemblyDependencyResolver("Plugin.dll");
        AssemblyName? assemblyName = null;

        var act = () => resolver.ResolveAssemblyToPath(assemblyName!);

        await Assert.That(act).Throws<ArgumentNullException>();
    }

    /// <summary>Verifies that the dependency resolver resolves unmanaged libraries with an implicit extension.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveUnmanagedDllToPath_WithImplicitExtension_ReturnsPath()
    {
        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var pluginPath = Path.Combine(tempDirectory, "Plugin.dll");
            var dependencyPath = Path.Combine(tempDirectory, "native.dll");
            await File.WriteAllTextAsync(pluginPath, string.Empty);
            await File.WriteAllTextAsync(dependencyPath, string.Empty);
            var resolver = new PluginAssemblyDependencyResolver(pluginPath);

            var resolvedPath = resolver.ResolveUnmanagedDllToPath("native");

            await Assert.That(resolvedPath).IsEqualTo(dependencyPath);
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

    /// <summary>Verifies that the dependency resolver resolves unmanaged libraries with an explicit extension.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveUnmanagedDllToPath_WithExplicitExtension_ReturnsPath()
    {
        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var pluginPath = Path.Combine(tempDirectory, "Plugin.dll");
            var dependencyPath = Path.Combine(tempDirectory, "native.custom");
            await File.WriteAllTextAsync(pluginPath, string.Empty);
            await File.WriteAllTextAsync(dependencyPath, string.Empty);
            var resolver = new PluginAssemblyDependencyResolver(pluginPath);

            var resolvedPath = resolver.ResolveUnmanagedDllToPath("native.custom");

            await Assert.That(resolvedPath).IsEqualTo(dependencyPath);
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

    /// <summary>Verifies that the dependency resolver throws when the unmanaged library name is empty.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveUnmanagedDllToPath_WithEmptyName_ThrowsArgumentException()
    {
        var resolver = new PluginAssemblyDependencyResolver("Plugin.dll");

        var act = () => resolver.ResolveUnmanagedDllToPath(string.Empty);

        await Assert.That(act).Throws<ArgumentException>();
    }

    /// <summary>Verifies that the load context exposes its name and loaded assemblies.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task AssemblyLoadContext_Default_ExposesNameAndAssemblies()
    {
        var context = PluginAssemblyLoadContext.Default;

        await Assert.That(context.Name).IsEqualTo("default");
        await Assert.That(context.Assemblies.Any()).IsTrue();
    }

    /// <summary>Verifies that loading from a null assembly name throws.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task LoadFromAssemblyName_WithNullAssemblyName_ThrowsArgumentNullException()
    {
        var context = new PluginAssemblyLoadContext("test");
        AssemblyName? assemblyName = null;

        var act = () => context.LoadFromAssemblyName(assemblyName!);

        await Assert.That(act).Throws<ArgumentNullException>();
    }

    /// <summary>Verifies that the base load context returns null for unresolved assembly names.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task LoadFromAssemblyName_WithUnresolvedAssemblyName_ReturnsNull()
    {
        var context = new PluginAssemblyLoadContext("test");

        Assembly? assembly = context.LoadFromAssemblyName(new AssemblyName("Missing.Assembly"));

        await Assert.That(assembly).IsNull();
    }

    /// <summary>Verifies that a derived load context can supply an assembly from Load.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task LoadFromAssemblyName_WithDerivedLoad_ReturnsAssembly()
    {
        var expectedAssembly = typeof(AssemblyLoadingTests).Assembly;
        var context = new TestAssemblyLoadContext("test", expectedAssembly);

        var assembly = context.LoadFromAssemblyName(expectedAssembly.GetName());

        await Assert.That(assembly).IsEqualTo(expectedAssembly);
    }

    /// <summary>Verifies that an assembly can be loaded from an existing assembly path.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task LoadFromAssemblyPath_WithExistingAssemblyPath_ReturnsAssembly()
    {
        var context = new PluginAssemblyLoadContext("test");
        var expectedAssembly = typeof(AssemblyLoadingTests).Assembly;

        var assembly = context.LoadFromAssemblyPath(expectedAssembly.Location);

        await Assert.That(assembly.GetName().Name).IsEqualTo(expectedAssembly.GetName().Name);
    }

    /// <summary>Verifies that native library load helpers return zero by default.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task LoadUnmanagedDllHelpers_ByDefault_ReturnZero()
    {
        var context = new TestAssemblyLoadContext("test", typeof(AssemblyLoadingTests).Assembly);

        await Assert.That(context.LoadNativeFromPath("native.dll")).IsEqualTo(IntPtr.Zero);
        await Assert.That(context.LoadNativeByName("native")).IsEqualTo(IntPtr.Zero);
    }

    /// <summary>Verifies that TryGetAssembly returns false when the context is null.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task TryGetAssembly_WithNullContext_ReturnsFalse()
    {
        PluginAssemblyLoadContext? context = null;

        var result = context!.TryGetAssembly(typeof(AssemblyLoadingTests).Assembly.GetName(), out var assembly);

        await Assert.That(result).IsFalse();
        await Assert.That(assembly).IsNull();
    }

    /// <summary>Verifies that TryGetAssembly returns false when an assembly is not loaded.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task TryGetAssembly_WithMissingAssembly_ReturnsFalse()
    {
        var context = PluginAssemblyLoadContext.Default;

        var result = context.TryGetAssembly(new AssemblyName("Missing.Plugin.Assembly"), out var assembly);

        await Assert.That(result).IsFalse();
        await Assert.That(assembly).IsNull();
    }

    /// <summary>Verifies that PluginLoadContext resolves plugin-local managed assembly paths.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task PluginLoadContext_ResolveAssemblyPath_WithExistingDependency_ReturnsPath()
    {
        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var pluginPath = Path.Combine(tempDirectory, "Plugin.dll");
            var dependencyPath = Path.Combine(tempDirectory, "Dependency.dll");
            await File.WriteAllTextAsync(pluginPath, string.Empty);
            await File.WriteAllTextAsync(dependencyPath, string.Empty);
            var context = new PluginLoadContext(pluginPath, "Plugin");

            var resolvedPath = context.ResolveAssemblyPath(new AssemblyName("Dependency"));

            await Assert.That(resolvedPath).IsEqualTo(dependencyPath);
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

    /// <summary>Verifies that PluginLoadContext returns already-loaded assemblies from the default context.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task PluginLoadContext_LoadFromAssemblyName_WithAlreadyLoadedAssembly_ReturnsAssembly()
    {
        var expectedAssembly = typeof(AssemblyLoadingTests).Assembly;
        var context = new PluginLoadContext(expectedAssembly.Location, "Plugin");

        var assembly = context.LoadFromAssemblyName(expectedAssembly.GetName());

        await Assert.That(assembly).IsEqualTo(expectedAssembly);
    }

    /// <summary>Verifies that PluginLoadContext loads an assembly from the plugin directory when it is not already loaded.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task PluginLoadContext_LoadFromAssemblyName_WithPluginLocalAssembly_ReturnsAssembly()
    {
        var candidatePath = GetUnloadedAssemblyPath();
        var assemblyName = new AssemblyName(Path.GetFileNameWithoutExtension(candidatePath));
        var context = new PluginLoadContext(candidatePath, assemblyName.Name!);

        var assembly = context.LoadFromAssemblyName(assemblyName);

        await Assert.That(assembly.GetName().Name).IsEqualTo(assemblyName.Name);
    }

    /// <summary>Verifies that PluginLoadContext returns null when a plugin-local assembly cannot be resolved.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task PluginLoadContext_LoadFromAssemblyName_WithMissingAssembly_ReturnsNull()
    {
        var context = new PluginLoadContext(typeof(AssemblyLoadingTests).Assembly.Location, "Plugin");

        Assembly? assembly = context.LoadFromAssemblyName(new AssemblyName("Missing.Plugin.Assembly"));

        await Assert.That(assembly).IsNull();
    }

    /// <summary>Verifies that PluginLoadContext returns zero when an unmanaged dependency cannot be resolved.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task PluginLoadContext_LoadUnmanagedDll_WithMissingLibrary_ReturnsZero()
    {
        var context = new PluginLoadContext(typeof(AssemblyLoadingTests).Assembly.Location, "Plugin");
        var method = typeof(PluginLoadContext).GetMethod("LoadUnmanagedDll", BindingFlags.Instance | BindingFlags.NonPublic);

        var result = (IntPtr)method!.Invoke(context, ["missing"])!;

        await Assert.That(result).IsEqualTo(IntPtr.Zero);
    }

    /// <summary>Creates a temporary directory for assembly resolver tests.</summary>
    /// <returns>The created temporary directory.</returns>
    private static string CreateTemporaryDirectory()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), "Extensions.Hosting.Tests", Guid.NewGuid().ToString("N"));
        _ = Directory.CreateDirectory(tempDirectory);
        return tempDirectory;
    }

    /// <summary>Finds a managed assembly in the test output that has not yet been loaded.</summary>
    /// <returns>The path to an unloaded managed assembly.</returns>
    private static string GetUnloadedAssemblyPath()
    {
        var assemblyDirectory = Path.GetDirectoryName(typeof(AssemblyLoadingTests).Assembly.Location)!;
        var loadedAssemblyNames = AppDomain.CurrentDomain.GetAssemblies()
            .Select(assembly => assembly.GetName().Name)
            .Where(name => name is not null)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var assemblyPath in Directory.EnumerateFiles(assemblyDirectory, "*.dll"))
        {
            var assemblyName = Path.GetFileNameWithoutExtension(assemblyPath);
            if (!loadedAssemblyNames.Contains(assemblyName) &&
                !assemblyName.StartsWith("Extensions.Hosting", StringComparison.OrdinalIgnoreCase))
            {
                return assemblyPath;
            }
        }

        throw new InvalidOperationException("No unloaded assembly was available in the test output directory.");
    }

    /// <summary>Test assembly load context that delegates managed loads to a supplied assembly.</summary>
    /// <param name="name">The load context name.</param>
    /// <param name="assembly">The assembly returned from managed load requests.</param>
    private sealed class TestAssemblyLoadContext(string name, Assembly assembly) : PluginAssemblyLoadContext(name)
    {
        /// <summary>Calls the protected native load-from-path helper.</summary>
        /// <param name="path">The native library path.</param>
        /// <returns>The native library handle.</returns>
        public IntPtr LoadNativeFromPath(string path) => LoadUnmanagedDllFromPath(path);

        /// <summary>Calls the protected native load-by-name helper.</summary>
        /// <param name="name">The native library name.</param>
        /// <returns>The native library handle.</returns>
        public IntPtr LoadNativeByName(string name) => LoadUnmanagedDll(name);

        /// <inheritdoc />
        protected override Assembly Load(AssemblyName assemblyName) => assembly;
    }
}
