// Copyright (c) 2019-2024 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReactiveMarbles.Extensions.Hosting.AppServices;
using ReactiveMarbles.Extensions.Hosting.Plugins;
using ReactiveMarbles.Extensions.Hosting.Wpf;

namespace Extensions.Hosting.Wpf.Example;

/// <summary>
/// Interaction logic for App.xaml.
/// </summary>
public partial class App
{
    /// <summary>
    /// Initializes a new instance of the <see cref="App"/> class.
    /// </summary>
    public App()
    {
        // Replace the namespace with the namespace of your plugins
        const string nameSpace = "ReactiveMarbles.Plugin";
        var executableLocation = Path.GetDirectoryName(GetType().Assembly.Location);
        var host = Host.CreateDefaultBuilder()
            .ConfigureLogging()
            .ConfigureConfiguration(Environment.GetCommandLineArgs())
            .ConfigureSingleInstance(builder =>
            {
                builder.MutexId = "{11D78650-4E8A-4D4E-95DC-B06047271CD3}";

                // This is called when an instance was already started, this is in the second instance
                builder.WhenNotFirstInstance = (hostingEnvironment, logger) =>
                    logger.LogWarning("Application {ApplicationName} already running.", hostingEnvironment.ApplicationName);
            })
            .ConfigurePlugins(pluginBuilder =>
            {
                //// Specify the location from where the Dll's are "globbed"
                var process = Process.GetCurrentProcess();
                var fullPath = process.MainModule?.FileName?.Replace(process.MainModule.ModuleName!, string.Empty);
                pluginBuilder?.AddScanDirectories(fullPath!);

                //// Add the netstandard framework libraries which can be found with the specified globs
                pluginBuilder?.IncludeFrameworks(@"\netstandard2.0\*.FrameworkLib.dll");

                //// Add the plugins which can be found with the specified globs
                var runtime = Path.GetFileName(executableLocation);
                pluginBuilder?.IncludePlugins(@$"\Plugins\{runtime}\{nameSpace}*.dll");
            })
            .ConfigureServices(serviceCollection => serviceCollection.AddTransient<SecondWindow>())
            .ConfigureWpf(wpfBuilder => wpfBuilder.UseCurrentApplication(this).UseWindow<MainWindow>())
            .UseWpfLifetime()
            .UseConsoleLifetime()
            .Build();

        host.RunAsync();
    }
}
