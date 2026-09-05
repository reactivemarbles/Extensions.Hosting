// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.ServiceProcess;
using System.Threading;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.Hosting;
#if REACTIVE_SHIM
using ReactiveMarbles.Extensions.Hosting.Reactive.PluginService;
using ReactiveMarbles.Extensions.Hosting.Reactive.Plugins;
#else
using ReactiveMarbles.Extensions.Hosting.PluginService;
using ReactiveMarbles.Extensions.Hosting.Plugins;
#endif

namespace Extensions.Hosting.PluginService.Platform.Tests;

/// <summary>Tests reachable PluginService branch paths that are not covered by the behavior-focused smoke tests.</summary>
public class PluginServiceBranchCoverageTests
{
    /// <summary>Stores a non-console argument for service mode selection tests.</summary>
    private const string NonConsoleSwitch = "--not-console";

    /// <summary>Stores a differently cased console switch for service mode selection tests.</summary>
    private const string UpperConsoleSwitch = "--CONSOLE";

    /// <summary>Captures the public logger default before any test creates a service host.</summary>
    private static bool _loggerWasNullOnModuleLoad;

    /// <summary>Captures process-start static state before test ordering can assign a logger.</summary>
    [ModuleInitializer]
    public static void CaptureInitialServiceHostLogger() => _loggerWasNullOnModuleLoad = ServiceHost.Logger is null;

    /// <summary>Verifies that the internal lifetime constructor rejects a missing runtime before service startup.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ServiceBaseLifetime_WithNullRuntime_ThrowsArgumentNullException()
    {
        using var applicationLifetime = new TestHostApplicationLifetime();

        await Assert.That(() => new TestServiceBaseLifetime(applicationLifetime, null!)).Throws<ArgumentNullException>();
    }

    /// <summary>Verifies null-builder branches for console and service lifetime extensions.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task LifetimeExtensions_WithNullBuilders_ReturnExpectedResults()
    {
        IHostApplicationBuilder? applicationBuilder = null;
        IHostBuilder? hostBuilder = null;

        await Assert.That(() => applicationBuilder!.UseConsoleLifetime()).Throws<ArgumentNullException>();
        await Assert.That(hostBuilder!.UseServiceBaseLifetime()).IsNull();
    }

    /// <summary>Verifies that replacing the service host runtime requires a concrete runtime instance.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ServiceHost_WithNullRuntime_ThrowsArgumentNullException() =>
        await Assert.That(static () => ServiceHost.UseRuntime(null!)).Throws<ArgumentNullException>();

    /// <summary>Verifies that a service host runner requires a runtime dependency.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ServiceHostRunner_WithNullRuntime_ThrowsArgumentNullException() =>
        await Assert.That(static () => new ServiceHostRunner(null!)).Throws<ArgumentNullException>();

    /// <summary>Verifies that the public logger is initially absent before a service host is created.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ServiceHost_Logger_IsNullBeforeHostCreation() =>
        await Assert.That(_loggerWasNullOnModuleLoad).IsTrue();

    /// <summary>Verifies service-mode selection for empty, non-console, case-insensitive console, and debugger policy inputs.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ServiceHostRunnerConfiguration_IsServiceMode_HandlesConsoleArgumentVariants()
    {
        await Assert.That(ServiceHostRunner.Configuration.IsServiceMode([], isDebuggerAttached: false)).IsTrue();
        await Assert.That(ServiceHostRunner.Configuration.IsServiceMode([NonConsoleSwitch], isDebuggerAttached: false)).IsTrue();
        await Assert.That(ServiceHostRunner.Configuration.IsServiceMode([UpperConsoleSwitch], isDebuggerAttached: false)).IsFalse();
        await Assert.That(ServiceHostRunner.Configuration.IsServiceMode([], isDebuggerAttached: true)).IsFalse();
    }

    /// <summary>Verifies that service mode resets the content root to the managed application base directory.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ServiceHostRunnerConfiguration_SetCurrentDirectoryToProcessRoot_UsesApplicationBaseDirectory()
    {
        var originalDirectory = Directory.GetCurrentDirectory();
        var expectedDirectory = Path.GetFullPath(AppContext.BaseDirectory);

        try
        {
            ServiceHostRunner.Configuration.SetCurrentDirectoryToProcessRoot();

            await Assert.That(AreEquivalentPaths(Directory.GetCurrentDirectory(), expectedDirectory)).IsTrue();
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }

    /// <summary>Verifies that plugin scanning configures the supplied builder with process and runtime-derived paths.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ServiceHostRunnerConfiguration_ConfigurePluginScanning_WithBuilderAddsProcessDirectory()
    {
        var pluginBuilder = new RecordingPluginBuilder();
        var executableLocation = Path.Combine(Directory.GetCurrentDirectory(), "plugins", "test-runtime");

        ServiceHostRunner.Configuration.ConfigurePluginScanning(pluginBuilder, executableLocation, null, "Extensions.Hosting");

        var expectedDirectory = Path.GetFullPath(AppContext.BaseDirectory);
        await Assert.That(ContainsEquivalentPath(pluginBuilder.FrameworkDirectories, expectedDirectory)).IsTrue();
        await Assert.That(ContainsEquivalentPath(pluginBuilder.PluginDirectories, expectedDirectory)).IsTrue();
    }

    /// <summary>Determines whether the paths contain the expected directory after trimming optional trailing separators.</summary>
    /// <param name="paths">The paths to inspect.</param>
    /// <param name="expectedPath">The expected path.</param>
    /// <returns>true when an equivalent path exists; otherwise, false.</returns>
    private static bool ContainsEquivalentPath(IEnumerable<string> paths, string expectedPath)
    {
        var normalizedExpectedPath = Path.TrimEndingDirectorySeparator(Path.GetFullPath(expectedPath));

        foreach (var path in paths)
        {
            if (AreEquivalentPaths(path, normalizedExpectedPath))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Determines whether two paths resolve to the same directory after trimming optional trailing separators.</summary>
    /// <param name="path">The actual path.</param>
    /// <param name="expectedPath">The expected path.</param>
    /// <returns>true when the paths are equivalent; otherwise, false.</returns>
    private static bool AreEquivalentPaths(string path, string expectedPath) =>
        Path.TrimEndingDirectorySeparator(Path.GetFullPath(path)) == Path.TrimEndingDirectorySeparator(Path.GetFullPath(expectedPath));

    /// <summary>Exposes the internal runtime constructor for null-guard verification.</summary>
    private sealed class TestServiceBaseLifetime : ServiceBaseLifetime
    {
        /// <summary>Initializes a new instance of the <see cref="TestServiceBaseLifetime"/> class.</summary>
        /// <param name="applicationLifetime">The application lifetime dependency.</param>
        /// <param name="runtime">The service host runtime dependency.</param>
        public TestServiceBaseLifetime(IHostApplicationLifetime applicationLifetime, IServiceHostRuntime runtime)
            : base(applicationLifetime, runtime)
        {
        }
    }

    /// <summary>Provides cancellable application lifetime tokens for constructor tests.</summary>
    private sealed class TestHostApplicationLifetime : IHostApplicationLifetime, IDisposable
    {
        /// <summary>Stores the application-started token source.</summary>
        private readonly CancellationTokenSource _applicationStarted = new();

        /// <summary>Stores the application-stopping token source.</summary>
        private readonly CancellationTokenSource _applicationStopping = new();

        /// <summary>Stores the application-stopped token source.</summary>
        private readonly CancellationTokenSource _applicationStopped = new();

        /// <inheritdoc />
        public CancellationToken ApplicationStarted => _applicationStarted.Token;

        /// <inheritdoc />
        public CancellationToken ApplicationStopping => _applicationStopping.Token;

        /// <inheritdoc />
        public CancellationToken ApplicationStopped => _applicationStopped.Token;

        /// <inheritdoc />
        public void StopApplication() => _applicationStopping.Cancel();

        /// <inheritdoc />
        public void Dispose()
        {
            _applicationStarted.Dispose();
            _applicationStopping.Dispose();
            _applicationStopped.Dispose();
        }
    }

    /// <summary>Records plugin scanning configuration changes for behavior assertions.</summary>
    private sealed class RecordingPluginBuilder : IPluginBuilder
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
}
