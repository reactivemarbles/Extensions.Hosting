// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactiveMarbles.Extensions.Hosting.Wpf;
using ReactiveMarbles.Extensions.Hosting.Wpf.Internals;

namespace Extensions.Hosting.Wpf.ZeroShellCoverage.Tests;

/// <summary>Verifies zero-shell behavior against an already-running WPF application.</summary>
[NotInParallel]
public sealed class ZeroShellCoverageTests
{
    /// <summary>Gets the maximum duration allowed for WPF application startup and shutdown.</summary>
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

    /// <summary>Verifies that an existing main window is shown and a missing main window is rejected.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task RunningApplication_ZeroShellShowsMainWindowAndRejectsMissingMainWindow()
    {
        var builder = Host.CreateApplicationBuilder();
        _ = builder.ConfigureWpf(static wpfBuilder => wpfBuilder.UseApplication<ZeroShellApplication>());
        using var host = builder.Build();

        try
        {
            await host.StartAsync().WaitAsync(Timeout);
            var context = host.Services.GetRequiredService<IWpfContext>();
            await ZeroShellApplication.Started.Task.WaitAsync(Timeout);
            var mainWindow = await context.Dispatcher.InvokeAsync(static () => new Window());
            await context.Dispatcher.InvokeAsync(() => Application.Current!.MainWindow = mainWindow);
            using var runningThread = new ExposedWpfThread(host.Services);
            runningThread.RunUiThreadStart();
            var isVisible = await context.Dispatcher.InvokeAsync(() => mainWindow.IsVisible);
            await Assert.That(isVisible).IsTrue();
            await context.Dispatcher.InvokeAsync(static () => Application.Current!.MainWindow = null);

            void Act() => runningThread.RunUiThreadStart();

            await Assert.That(Act).Throws<InvalidOperationException>();
        }
        finally
        {
            await host.StopAsync().WaitAsync(Timeout);
        }
    }

    /// <summary>Exposes the protected WPF thread start implementation for a running-dispatcher harness.</summary>
    public sealed class ExposedWpfThread : WpfThread
    {
        /// <summary>Initializes a new instance of the <see cref="ExposedWpfThread"/> class.</summary>
        /// <param name="serviceProvider">The service provider used by the WPF thread.</param>
        public ExposedWpfThread(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        /// <summary>Runs the protected WPF UI-thread start implementation.</summary>
        public void RunUiThreadStart() => UiThreadStart();
    }
}
