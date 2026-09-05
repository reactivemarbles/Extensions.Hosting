// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using ReactiveMarbles.Extensions.Hosting.AppServices;
using ReactiveMarbles.Extensions.Hosting.UiThread;
using ReactiveMarbles.Extensions.Logging;
using NormalPluginLoadContext = ReactiveMarbles.Extensions.Hosting.Plugins.Internals.PluginLoadContext;
using NormalPlugins = ReactiveMarbles.Extensions.Hosting.Plugins;
using ReactivePluginLoadContext = ReactiveMarbles.Extensions.Hosting.Reactive.Plugins.Internals.PluginLoadContext;
using ReactivePlugins = ReactiveMarbles.Extensions.Hosting.Reactive.Plugins;

namespace Extensions.Hosting.LegacyNetFramework.Worker;

/// <summary>Runs observable net462 package compatibility probes for coverage collection.</summary>
public static class Program
{
    /// <summary>Stores the expected command-line argument count.</summary>
    private const int ExpectedArgumentCount = 3;

    /// <summary>Stores the exit code used when command-line arguments are invalid.</summary>
    private const int InvalidArgumentsExitCode = 2;

    /// <summary>Stores the result line prefix.</summary>
    private const string ResultPrefix = "RESULT";

    /// <summary>Stores the passing result marker.</summary>
    private const string Pass = "PASS";

    /// <summary>Stores the failing result marker.</summary>
    private const string Fail = "FAIL";

    /// <summary>Stores the UI probe wait timeout in seconds.</summary>
    private const int UiProbeTimeoutSeconds = 10;

    /// <summary>Stores the earlier test plugin order.</summary>
    private const int EarlierPluginOrder = -1;

    /// <summary>Stores the later test plugin order.</summary>
    private const int LaterPluginOrder = 10;

    /// <summary>Stores the normal plugin fixture file name.</summary>
    private const string NormalPluginFixtureFileName = "Extensions.Hosting.PluginLoading.Fixture.dll";

    /// <summary>Stores the reactive plugin fixture file name.</summary>
    private const string ReactivePluginFixtureFileName = "Extensions.Hosting.PluginLoading.Reactive.Fixture.dll";

    /// <summary>Runs the worker process.</summary>
    /// <param name="args">The normal plugin directory, reactive plugin directory, and result file path.</param>
    /// <returns>Zero when all probes pass; otherwise a non-zero exit code.</returns>
    public static int Main(string[] args)
    {
        if (args.Length != ExpectedArgumentCount)
        {
            return InvalidArgumentsExitCode;
        }

        var finalizationProbe = new FinalizationProbeProvider(args[2]);
        var results = new Dictionary<string, bool>
        {
            ["resource_mutex"] = RunResourceMutexProbe(),
            ["base_ui_thread"] = RunBaseUiThreadProbe(),
            ["normal_plugin_load"] = RunNormalPluginLoadProbe(args[0]),
            ["reactive_plugin_load"] = RunReactivePluginLoadProbe(args[1]),
            ["missing_plugin_load"] = RunMissingPluginLoadProbe(args[0], args[1]),
        };

        using (var writer = new StreamWriter(args[2], append: false))
        {
            foreach (var result in results)
            {
                writer.WriteLine($"{ResultPrefix} {result.Key}={(result.Value ? Pass : Fail)}");
            }
        }

        foreach (var result in results.Values)
        {
            if (!result)
            {
                return 1;
            }
        }

        GC.KeepAlive(finalizationProbe);
        return 0;
    }

    /// <summary>Creates and releases a net462 resource mutex through the public package API.</summary>
    /// <returns>true when the mutex is acquired.</returns>
    private static bool RunResourceMutexProbe()
    {
        var mutexId = $"Local\\ExtensionsHostingLegacyWorker-{Guid.NewGuid():N}";
        using var mutex = ResourceMutex.Create(NullLogger.Instance, mutexId, resourceName: "LegacyWorker", global: false);
        return mutex.IsLocked;
    }

    /// <summary>Starts a dedicated UI thread through the net462 operating-system branch.</summary>
    /// <returns>true when the worker UI thread reaches its start callback.</returns>
    private static bool RunBaseUiThreadProbe()
    {
        using var preStarted = new ManualResetEventSlim(initialState: false);
        using var uiStarted = new ManualResetEventSlim(initialState: false);
        using var uiContext = new WorkerUiContext();

        var services = new ServiceCollection();
        _ = services.AddSingleton(uiContext);
        using var serviceProvider = services.BuildServiceProvider();
        using var uiThread = new WorkerUiThread(serviceProvider, preStarted, uiStarted);

        uiThread.Start();

        return preStarted.Wait(TimeSpan.FromSeconds(UiProbeTimeoutSeconds))
            && uiStarted.Wait(TimeSpan.FromSeconds(UiProbeTimeoutSeconds))
            && uiContext.IsRunning;
    }

    /// <summary>Loads the normal plugin fixture through the net462 plugin package.</summary>
    /// <param name="pluginDirectory">The directory that contains the normal plugin fixture.</param>
    /// <returns>true when the normal plugin fixture assembly is loaded and scanned.</returns>
    private static bool RunNormalPluginLoadProbe(string pluginDirectory)
    {
        var pluginPath = Path.Combine(pluginDirectory, NormalPluginFixtureFileName);
        var scannedAssemblies = new List<string>();
        var configuredPlugins = new List<string>();

        var hostBuilder = Host.CreateDefaultBuilder();
        _ = NormalPlugins.HostBuilderPluginExtensions.ConfigurePlugins(hostBuilder, builder =>
        {
            if (builder is null)
            {
                return;
            }

            builder.PluginDirectories.Add(pluginDirectory);
            _ = builder.PluginMatcher.AddInclude(Path.GetFileName(pluginPath));
            builder.AssemblyScanFunc = assembly =>
            {
                scannedAssemblies.Add(Path.GetFileName(assembly.Location));
                return
                [
                    new NormalLaterWorkerPlugin(configuredPlugins),
                    new NormalEarlierWorkerPlugin(configuredPlugins),
                ];
            };
        });

        using var host = hostBuilder.Build();
        return scannedAssemblies.Contains(Path.GetFileName(pluginPath))
            && configuredPlugins.SequenceEqual(["normal-earlier", "normal-later"]);
    }

    /// <summary>Loads the reactive plugin fixture through the net462 reactive plugin package.</summary>
    /// <param name="pluginDirectory">The directory that contains the reactive plugin fixture.</param>
    /// <returns>true when the reactive plugin fixture assembly is loaded and scanned.</returns>
    private static bool RunReactivePluginLoadProbe(string pluginDirectory)
    {
        var pluginPath = Path.Combine(pluginDirectory, ReactivePluginFixtureFileName);
        var scannedAssemblies = new List<string>();
        var configuredPlugins = new List<string>();

        var hostBuilder = Host.CreateDefaultBuilder();
        _ = ReactivePlugins.HostBuilderPluginExtensions.ConfigurePlugins(hostBuilder, builder =>
        {
            if (builder is null)
            {
                return;
            }

            builder.PluginDirectories.Add(pluginDirectory);
            _ = builder.PluginMatcher.AddInclude(Path.GetFileName(pluginPath));
            builder.AssemblyScanFunc = assembly =>
            {
                scannedAssemblies.Add(Path.GetFileName(assembly.Location));
                return
                [
                    new ReactiveLaterWorkerPlugin(configuredPlugins),
                    new ReactiveEarlierWorkerPlugin(configuredPlugins),
                ];
            };
        });

        using var host = hostBuilder.Build();
        return scannedAssemblies.Contains(Path.GetFileName(pluginPath))
            && configuredPlugins.SequenceEqual(["reactive-earlier", "reactive-later"]);
    }

    /// <summary>Verifies that both legacy plugin contexts report missing dependencies without loading an unrelated assembly.</summary>
    /// <param name="normalPluginDirectory">The directory containing the normal plugin fixture.</param>
    /// <param name="reactivePluginDirectory">The directory containing the reactive plugin fixture.</param>
    /// <returns>true when both contexts leave the missing dependency unresolved.</returns>
    private static bool RunMissingPluginLoadProbe(string normalPluginDirectory, string reactivePluginDirectory)
    {
        var missingAssembly = new AssemblyName($"MissingLegacyPlugin{Guid.NewGuid():N}");
        var normalContext = new NormalPluginLoadContext(Path.Combine(normalPluginDirectory, NormalPluginFixtureFileName), "normal-missing");
        var reactiveContext = new ReactivePluginLoadContext(Path.Combine(reactivePluginDirectory, ReactivePluginFixtureFileName), "reactive-missing");

        return normalContext.TryLoadFromAssemblyName(missingAssembly) is null
            && reactiveContext.TryLoadFromAssemblyName(missingAssembly) is null;
    }

    /// <summary>Records cleanup performed by the inherited finalizer at .NET Framework process shutdown.</summary>
    /// <param name="resultPath">The file that receives the finalization result after the worker closes its writer.</param>
    private sealed class FinalizationProbeProvider(string resultPath)
        : Log4NetProvider(new Log4NetProviderOptions { ExternalConfigurationSetup = true, LoggerRepository = $"finalizer-{Guid.NewGuid():N}" })
    {
        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                return;
            }

            File.AppendAllText(resultPath, $"{ResultPrefix} log4net_finalizer={Pass}{Environment.NewLine}");
        }
    }

    /// <summary>Provides an earlier normal plugin for ordering coverage.</summary>
    /// <param name="configuredPlugins">The list that receives configured plugin markers.</param>
    [NormalPlugins.PluginOrder(EarlierPluginOrder)]
    private sealed class NormalEarlierWorkerPlugin(List<string> configuredPlugins) : NormalPlugins.IPlugin
    {
        /// <inheritdoc />
        public void ConfigureHost(object hostBuilderContext, IServiceCollection serviceCollection) =>
            configuredPlugins.Add("normal-earlier");
    }

    /// <summary>Provides a later normal plugin for ordering coverage.</summary>
    /// <param name="configuredPlugins">The list that receives configured plugin markers.</param>
    [NormalPlugins.PluginOrder(LaterPluginOrder)]
    private sealed class NormalLaterWorkerPlugin(List<string> configuredPlugins) : NormalPlugins.IPlugin
    {
        /// <inheritdoc />
        public void ConfigureHost(object hostBuilderContext, IServiceCollection serviceCollection) =>
            configuredPlugins.Add("normal-later");
    }

    /// <summary>Provides an earlier reactive plugin for ordering coverage.</summary>
    /// <param name="configuredPlugins">The list that receives configured plugin markers.</param>
    [ReactivePlugins.PluginOrder(EarlierPluginOrder)]
    private sealed class ReactiveEarlierWorkerPlugin(List<string> configuredPlugins) : ReactivePlugins.IPlugin
    {
        /// <inheritdoc />
        public void ConfigureHost(object hostBuilderContext, IServiceCollection serviceCollection) =>
            configuredPlugins.Add("reactive-earlier");
    }

    /// <summary>Provides a later reactive plugin for ordering coverage.</summary>
    /// <param name="configuredPlugins">The list that receives configured plugin markers.</param>
    [ReactivePlugins.PluginOrder(LaterPluginOrder)]
    private sealed class ReactiveLaterWorkerPlugin(List<string> configuredPlugins) : ReactivePlugins.IPlugin
    {
        /// <inheritdoc />
        public void ConfigureHost(object hostBuilderContext, IServiceCollection serviceCollection) =>
            configuredPlugins.Add("reactive-later");
    }

    /// <summary>Provides a minimal UI context for the worker probe.</summary>
    private sealed class WorkerUiContext : IUiContext, IDisposable
    {
        /// <inheritdoc />
        public bool IsLifetimeLinked { get; set; }

        /// <inheritdoc />
        public bool IsRunning { get; set; }

        /// <inheritdoc />
        public void Dispose()
        {
        }
    }

    /// <summary>Provides a minimal concrete UI thread for the worker probe.</summary>
    private sealed class WorkerUiThread : BaseUiThread<WorkerUiContext>
    {
        /// <summary>Stores the pre-start signal.</summary>
        private readonly ManualResetEventSlim _preStarted;

        /// <summary>Stores the start signal.</summary>
        private readonly ManualResetEventSlim _uiStarted;

        /// <summary>Initializes a new instance of the <see cref="WorkerUiThread"/> class.</summary>
        /// <param name="serviceProvider">The worker service provider.</param>
        /// <param name="preStarted">The pre-start signal.</param>
        /// <param name="uiStarted">The start signal.</param>
        public WorkerUiThread(IServiceProvider serviceProvider, ManualResetEventSlim preStarted, ManualResetEventSlim uiStarted)
            : base(serviceProvider)
        {
            _preStarted = preStarted;
            _uiStarted = uiStarted;
        }

        /// <inheritdoc />
        protected override void PreUiThreadStart() => _preStarted.Set();

        /// <inheritdoc />
        protected override void UiThreadStart() => _uiStarted.Set();
    }
}
