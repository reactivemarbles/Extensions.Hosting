// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactiveMarbles.Extensions.Hosting.Wpf;
using ReactiveMarbles.Extensions.Hosting.Wpf.Internals;

namespace Extensions.Hosting.Wpf.MultipleShellCoverage.Tests;

/// <summary>Verifies the hosted WPF multiple-shell lifecycle in an isolated executable process.</summary>
[NotInParallel]
public sealed class MultipleShellLifecycleCoverageTests
{
    /// <summary>Gets the maximum duration allowed for WPF application startup and shutdown.</summary>
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

    /// <summary>Verifies two registered shell windows are shown during application startup.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task StartAsync_MultipleShellsRunsApplicationAndShowsShellWindows()
    {
        var builder = Host.CreateApplicationBuilder();
        _ = builder.ConfigureWpf(static wpfBuilder =>
        {
            _ = wpfBuilder.UseApplication(typeof(MultipleShellApplication));
            _ = wpfBuilder.UseWindow(typeof(FirstShellWindow));
            _ = wpfBuilder.UseWindow(typeof(SecondShellWindow));
        });
        using var host = builder.Build();

        try
        {
            await host.StartAsync().WaitAsync(Timeout);
            var dispatcher = await MultipleShellApplication.Started.Task.WaitAsync(Timeout);
            var context = host.Services.GetRequiredService<IWpfContext>();
            var firstShell = host.Services.GetRequiredService<FirstShellWindow>();
            var secondShell = host.Services.GetRequiredService<SecondShellWindow>();
            var shellsAreVisible = await context.Dispatcher.InvokeAsync(
                () => firstShell.IsVisible && secondShell.IsVisible);

            await Assert.That(context.WpfApplication).IsTypeOf<MultipleShellApplication>();
            await Assert.That(context.Dispatcher).IsSameReferenceAs(dispatcher);
            await Assert.That(shellsAreVisible).IsTrue();

            using var runningThread = new ExposedWpfThread(host.Services);
            runningThread.RunUiThreadStart();
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
