// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReactiveMarbles.Extensions.Hosting.AppServices;
using ReactiveMarbles.Extensions.Hosting.ReactiveUI;
using ReactiveMarbles.Extensions.Hosting.Wpf;
using ReactiveUI;

namespace Extensions.Hosting.Reactive.Example;

/// <summary>Interaction logic for App.xaml.</summary>
public partial class App
{
    /// <summary>Logs that another instance is already running.</summary>
    private static readonly Action<ILogger, string, Exception?> ApplicationAlreadyRunning =
        LoggerMessage.Define<string>(LogLevel.Warning, new EventId(1, nameof(ApplicationAlreadyRunning)), "Application {ApplicationName} already running.");

    /// <summary>Initializes a new instance of the <see cref="App"/> class.</summary>
    public App()
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureSplatForMicrosoftDependencyResolver()
            .ConfigureLogging()
            .ConfigureConfiguration(Environment.GetCommandLineArgs())
            .ConfigureSingleInstance(builder =>
            {
                builder.MutexId = "{691A4D6D-2CE0-4D47-B7F4-D99D8C02161E}";
                builder.WhenNotFirstInstance = (hostingEnvironment, logger) =>
                    ApplicationAlreadyRunning(logger, hostingEnvironment.ApplicationName, null);
            })
            .ConfigureServices(services => services.AddTransient<IViewFor<NugetDetailsViewModel>, NugetDetailsView>())
            .ConfigureWpf(wpfBuilder => wpfBuilder.UseCurrentApplication(this).UseWindow<MainWindow>())
            .UseWpfLifetime()
            .UseConsoleLifetime()
            .Build();

        _ = host.RunAsync();
    }
}
