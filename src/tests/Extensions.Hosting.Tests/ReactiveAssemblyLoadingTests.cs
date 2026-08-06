// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.IO;
using System.Reflection;
using ReactiveMarbles.Extensions.Hosting.Reactive.Plugins.Internals;
using ReactiveAssemblyDependencyResolver = ReactiveMarbles.Extensions.Hosting.Reactive.Plugins.Internals.AssemblyDependencyResolver;
using ReactiveAssemblyLoadContext = ReactiveMarbles.Extensions.Hosting.Reactive.Plugins.Internals.AssemblyLoadContext;

namespace Extensions.Hosting.Tests;

/// <summary>Contains tests for reactive shim plugin assembly loading helper types.</summary>
public class ReactiveAssemblyLoadingTests
{
    /// <summary>The native library file name used by resolver tests.</summary>
    private const string NativeLibraryFileName = "native.dll";

    /// <summary>The native library name used by resolver tests.</summary>
    private const string NativeLibraryName = "native";

    /// <summary>Verifies that the dependency resolver resolves existing managed assembly paths.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveAssemblyToPath_WithExistingAssembly_ReturnsPath()
    {
        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var pluginPath = Path.Combine(tempDirectory, $"{nameof(Plugin)}.dll");
            var dependencyPath = Path.Combine(tempDirectory, "Dependency.dll");
            await File.WriteAllTextAsync(pluginPath, string.Empty);
            await File.WriteAllTextAsync(dependencyPath, string.Empty);
            var resolver = new ReactiveAssemblyDependencyResolver(pluginPath);

            var resolvedPath = resolver.ResolveAssemblyToPath(new("Dependency"));

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
            var pluginPath = Path.Combine(tempDirectory, $"{nameof(Plugin)}.dll");
            await File.WriteAllTextAsync(pluginPath, string.Empty);
            var resolver = new ReactiveAssemblyDependencyResolver(pluginPath);

            var resolvedPath = resolver.ResolveAssemblyToPath(new("Missing.Dependency"));

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
        var resolver = new ReactiveAssemblyDependencyResolver($"{nameof(Plugin)}.dll");
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
            var pluginPath = Path.Combine(tempDirectory, $"{nameof(Plugin)}.dll");
            var dependencyPath = Path.Combine(tempDirectory, NativeLibraryFileName);
            await File.WriteAllTextAsync(pluginPath, string.Empty);
            await File.WriteAllTextAsync(dependencyPath, string.Empty);
            var resolver = new ReactiveAssemblyDependencyResolver(pluginPath);

            var resolvedPath = resolver.ResolveUnmanagedDllToPath(NativeLibraryName);

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
            var pluginPath = Path.Combine(tempDirectory, $"{nameof(Plugin)}.dll");
            var dependencyPath = Path.Combine(tempDirectory, "native.custom");
            await File.WriteAllTextAsync(pluginPath, string.Empty);
            await File.WriteAllTextAsync(dependencyPath, string.Empty);
            var resolver = new ReactiveAssemblyDependencyResolver(pluginPath);

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
        var resolver = new ReactiveAssemblyDependencyResolver($"{nameof(Plugin)}.dll");

        var act = () => resolver.ResolveUnmanagedDllToPath(string.Empty);

        await Assert.That(act).Throws<ArgumentException>();
    }

    /// <summary>Verifies that the load context exposes its name and loaded assemblies.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task AssemblyLoadContext_Default_ExposesNameAndAssemblies()
    {
        var context = ReactiveAssemblyLoadContext.Default;

        await Assert.That(context.Name).IsEqualTo("default");
        await Assert.That(TestEnumerable.ContainsAny(ReactiveAssemblyLoadContext.Assemblies)).IsTrue();
    }

    /// <summary>Verifies that loading from a null assembly name throws.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task LoadFromAssemblyName_WithNullAssemblyName_ThrowsArgumentNullException()
    {
        var context = new ReactiveAssemblyLoadContext("test");
        AssemblyName? assemblyName = null;

        var act = () => context.LoadFromAssemblyName(assemblyName!);

        await Assert.That(act).Throws<ArgumentNullException>();
    }

    /// <summary>Verifies that the base load context returns null for unresolved assembly names.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task LoadFromAssemblyName_WithUnresolvedAssemblyName_ReturnsNull()
    {
        var context = new ReactiveAssemblyLoadContext("test");

        Assembly? assembly = context.LoadFromAssemblyName(new("Missing.Assembly"));

        await Assert.That(assembly).IsNull();
    }

    /// <summary>Verifies that a derived load context can supply an assembly from Load.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task LoadFromAssemblyName_WithDerivedLoad_ReturnsAssembly()
    {
        var expectedAssembly = typeof(ReactiveAssemblyLoadingTests).Assembly;
        var context = new TestAssemblyLoadContext("test", expectedAssembly);

        var assembly = context.LoadFromAssemblyName(expectedAssembly.GetName());

        await Assert.That(assembly).IsEqualTo(expectedAssembly);
    }

    /// <summary>Verifies that an assembly can be loaded from an existing assembly path.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task LoadFromAssemblyPath_WithExistingAssemblyPath_ReturnsAssembly()
    {
        var expectedAssembly = typeof(ReactiveAssemblyLoadingTests).Assembly;

        var assembly = ReactiveAssemblyLoadContext.LoadFromAssemblyPath(expectedAssembly.Location);

        await Assert.That(assembly.GetName().Name).IsEqualTo(expectedAssembly.GetName().Name);
    }

    /// <summary>Verifies that native library load helpers return zero by default.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task LoadUnmanagedDllHelpers_ByDefault_ReturnZero()
    {
        var context = new TestAssemblyLoadContext("test", typeof(ReactiveAssemblyLoadingTests).Assembly);

        await Assert.That(TestAssemblyLoadContext.LoadNativeFromPath(NativeLibraryFileName)).IsEqualTo(IntPtr.Zero);
        await Assert.That(context.LoadNativeByName(NativeLibraryName)).IsEqualTo(IntPtr.Zero);
    }

    /// <summary>Verifies that TryGetAssembly returns false when the context is null.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task TryGetAssembly_WithNullContext_ReturnsFalse()
    {
        ReactiveAssemblyLoadContext? context = null;

        var result = context!.TryGetAssembly(typeof(ReactiveAssemblyLoadingTests).Assembly.GetName(), out var assembly);

        await Assert.That(result).IsFalse();
        await Assert.That(assembly).IsNull();
    }

    /// <summary>Verifies that TryGetAssembly returns true when an assembly is loaded.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task TryGetAssembly_WithLoadedAssembly_ReturnsTrue()
    {
        var context = ReactiveAssemblyLoadContext.Default;

        var result = context.TryGetAssembly(typeof(ReactiveAssemblyLoadingTests).Assembly.GetName(), out var assembly);

        await Assert.That(result).IsTrue();
        await Assert.That(assembly).IsEqualTo(typeof(ReactiveAssemblyLoadingTests).Assembly);
    }

    /// <summary>Verifies that TryGetAssembly returns false when an assembly is not loaded.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task TryGetAssembly_WithMissingAssembly_ReturnsFalse()
    {
        var context = ReactiveAssemblyLoadContext.Default;

        var result = context.TryGetAssembly(new("Missing.Plugin.Assembly"), out var assembly);

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
            var pluginPath = Path.Combine(tempDirectory, $"{nameof(Plugin)}.dll");
            var dependencyPath = Path.Combine(tempDirectory, "Dependency.dll");
            await File.WriteAllTextAsync(pluginPath, string.Empty);
            await File.WriteAllTextAsync(dependencyPath, string.Empty);
            var context = new PluginLoadContext(pluginPath, nameof(Plugin));

            var resolvedPath = context.ResolveAssemblyPath(new("Dependency"));

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
        var expectedAssembly = typeof(ReactiveAssemblyLoadingTests).Assembly;
        var context = new PluginLoadContext(expectedAssembly.Location, nameof(Plugin));

        var assembly = context.LoadFromAssemblyName(expectedAssembly.GetName());

        await Assert.That(assembly).IsEqualTo(expectedAssembly);
    }

    /// <summary>Verifies that PluginLoadContext loads an assembly from the plugin directory when it is not already loaded.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task PluginLoadContext_LoadFromAssemblyName_WithPluginLocalAssembly_ReturnsAssembly()
    {
        var pluginPath = Path.Combine(
            AppContext.BaseDirectory,
            "ExternalPluginFixtures",
            "reactive",
            "Extensions.Hosting.PluginLoading.Reactive.Fixture.dll");
        var assemblyName = AssemblyName.GetAssemblyName(pluginPath);
        var context = new PluginLoadContext(pluginPath, assemblyName.Name!);

        var assembly = context.TryLoadFromAssemblyName(assemblyName);

        await Assert.That(assembly!.GetName().Name).IsEqualTo(assemblyName.Name);
        await Assert.That(Path.GetFullPath(assembly.Location)).IsEqualTo(Path.GetFullPath(pluginPath));
        var pluginCount = 0;
        foreach (var plugin in ReactiveMarbles.Extensions.Hosting.Reactive.Plugins.PluginScanner.ScanForPluginInstances(assembly))
        {
            _ = plugin;
            pluginCount++;
        }

        await Assert.That(pluginCount).IsEqualTo(1);
    }

    /// <summary>Verifies that PluginLoadContext returns null when a plugin-local assembly cannot be resolved.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task PluginLoadContext_LoadFromAssemblyName_WithMissingAssembly_ReturnsNull()
    {
        var context = new PluginLoadContext(typeof(ReactiveAssemblyLoadingTests).Assembly.Location, nameof(Plugin));

        var assembly = context.TryLoadFromAssemblyName(new("Missing.Plugin.Assembly"));

        await Assert.That(assembly).IsNull();
    }

    /// <summary>Verifies that PluginLoadContext returns zero when an unmanaged dependency cannot be resolved.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task PluginLoadContext_LoadUnmanagedDll_WithMissingLibrary_ReturnsZero()
    {
        var context = new PluginLoadContext(typeof(ReactiveAssemblyLoadingTests).Assembly.Location, nameof(Plugin));

        var result = context.LoadUnmanagedLibrary("missing");

        await Assert.That(result).IsEqualTo(IntPtr.Zero);
    }

    /// <summary>Verifies that PluginLoadContext attempts to load an existing unmanaged dependency from its resolved path.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task PluginLoadContext_LoadUnmanagedDll_WithInvalidExistingLibrary_ThrowsBadImageFormatException()
    {
        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var pluginPath = Path.Combine(tempDirectory, $"{nameof(Plugin)}.dll");
            var dependencyPath = Path.Combine(tempDirectory, NativeLibraryFileName);
            await File.WriteAllTextAsync(pluginPath, string.Empty);
            await File.WriteAllTextAsync(dependencyPath, string.Empty);
            var context = new PluginLoadContext(pluginPath, nameof(Plugin));

            var act = () => context.LoadUnmanagedLibrary(NativeLibraryName);

            await Assert.That(act).Throws<BadImageFormatException>();
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

    /// <summary>Creates a temporary directory for assembly resolver tests.</summary>
    /// <returns>The created temporary directory.</returns>
    private static string CreateTemporaryDirectory()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), "Extensions.Hosting.Tests", Guid.NewGuid().ToString("N"));
        _ = Directory.CreateDirectory(tempDirectory);
        return tempDirectory;
    }

    /// <summary>Test assembly load context that delegates managed loads to a supplied assembly.</summary>
    /// <param name="name">The load context name.</param>
    /// <param name="assembly">The assembly returned from managed load requests.</param>
    private sealed class TestAssemblyLoadContext(string name, Assembly assembly) : ReactiveAssemblyLoadContext(name)
    {
        /// <summary>Calls the protected native load-from-path helper.</summary>
        /// <param name="path">The native library path.</param>
        /// <returns>The native library handle.</returns>
        public static IntPtr LoadNativeFromPath(string path) => LoadUnmanagedDllFromPath(path);

        /// <summary>Calls the protected native load-by-name helper.</summary>
        /// <param name="name">The native library name.</param>
        /// <returns>The native library handle.</returns>
        public IntPtr LoadNativeByName(string name) => LoadUnmanagedDll(name);

        /// <inheritdoc />
        protected override Assembly Load(AssemblyName assemblyName) => assembly;
    }
}
