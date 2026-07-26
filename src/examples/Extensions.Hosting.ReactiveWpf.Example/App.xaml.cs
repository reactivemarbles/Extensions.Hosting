// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows;
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
        LoggerMessage.Define<string>(LogLevel.Warning, new(1, nameof(ApplicationAlreadyRunning)), "Application {ApplicationName} already running.");

    /// <inheritdoc />
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var host = Host.CreateDefaultBuilder()
            .ConfigureSplatForMicrosoftDependencyResolver()
            .ConfigureLogging()
            .ConfigureConfiguration(Environment.GetCommandLineArgs())
            .ConfigureSingleInstance(static builder =>
            {
                builder.MutexId = "{691A4D6D-2CE0-4D47-B7F4-D99D8C02161E}";
                builder.WhenNotFirstInstance = static (hostingEnvironment, logger) =>
                    ApplicationAlreadyRunning(logger, hostingEnvironment.ApplicationName, null);
            })
            .ConfigureServices(static services => services.AddTransient<IViewFor<NugetDetailsViewModel>, NugetDetailsView>())
            .ConfigureWpf(wpfBuilder => wpfBuilder.UseCurrentApplication(this).UseWindow<MainWindow>())
            .UseWpfLifetime()
            .UseConsoleLifetime()
            .Build();

        _ = host.RunAsync();
    }
}
