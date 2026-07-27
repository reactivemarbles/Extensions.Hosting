// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReactiveMarbles.Extensions.Hosting.WinForms;

namespace Extensions.Hosting.WinForms.Tests;

/// <summary>Verifies Windows Forms thread startup and hosted-service shutdown paths.</summary>
[NotInParallel]
public sealed class WinFormsLifetimeTests
{
    /// <summary>Stores the event identifier logged when form cleanup fails.</summary>
    private const int FormCleanupFailedEventId = 2;

    /// <summary>Gets the maximum duration allowed for UI startup and shutdown.</summary>
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

    /// <summary>Gets the interval used to poll UI state.</summary>
    private static readonly TimeSpan PollingInterval = TimeSpan.FromMilliseconds(10);

    /// <summary>Gets the time provider used to bound asynchronous test operations.</summary>
    private static readonly TimeProvider SystemTimeProvider = TimeProvider.System;

    /// <summary>Verifies that a cancelled start does not release the dedicated UI thread.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task HostedService_StartAsync_WithCancelledToken_DoesNotStartUiThread()
    {
        var hostBuilder = Host.CreateApplicationBuilder();
        _ = hostBuilder.ConfigureWinForms(static context => context.EnableVisualStyles = false);
        using var host = hostBuilder.Build();
        var context = host.Services.GetRequiredService<IWinFormsContext>();
        var hostedService = host.Services.GetRequiredService<IHostedService>();
        using var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();

        await hostedService.StartAsync(cancellationTokenSource.Token);

        await Assert.That(context.IsRunning).IsFalse();
    }

    /// <summary>Verifies the no-shell message-loop path and service initialization.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task HostedService_WithoutShell_InitializesServicesAndStopsMessageLoop()
    {
        var hostBuilder = Host.CreateApplicationBuilder();
        _ = hostBuilder.ConfigureWinForms(static context => context.EnableVisualStyles = false);
        _ = hostBuilder.Services.AddSingleton<RecordingWinFormsService>();
        _ = hostBuilder.Services.AddSingleton<IWinFormsService>(static serviceProvider => serviceProvider.GetRequiredService<RecordingWinFormsService>());
        using var host = hostBuilder.Build();
        var context = host.Services.GetRequiredService<IWinFormsContext>();
        var service = host.Services.GetRequiredService<RecordingWinFormsService>();

        await host.StartAsync().WaitAsync(Timeout);
        await service.Initialized.Task.WaitAsync(Timeout);
        await WaitForAsync(() => context.IsRunning && context.Dispatcher is not null);

        await host.StopAsync().WaitAsync(Timeout);
        await WaitForAsync(() => !context.IsRunning);

        await Assert.That(context.Dispatcher).IsNotNull();
    }

    /// <summary>Verifies that shutdown closes an open shell without mutating the forms enumeration.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task HostedService_StopAsync_WithOpenShell_ClosesShellAndStopsMessageLoop()
    {
        var hostBuilder = Host.CreateApplicationBuilder();
        _ = hostBuilder.ConfigureWinFormsShell<TestShell>();
        using var host = hostBuilder.Build();
        var context = host.Services.GetRequiredService<IWinFormsContext>();

        await host.StartAsync().WaitAsync(Timeout);
        await WaitForAsync(() => context.IsRunning && context.Dispatcher is not null);

        await host.StopAsync().WaitAsync(Timeout);

        await WaitForAsync(() => !context.IsRunning);
    }

    /// <summary>Verifies that a cleanup exception is logged and does not prevent message-loop shutdown.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task HostedService_StopAsync_WhenShellCleanupThrows_LogsWarningAndStopsMessageLoop()
    {
        var loggerProvider = new RecordingLoggerProvider();
        var hostBuilder = Host.CreateApplicationBuilder();
        _ = hostBuilder.Logging.ClearProviders();
        _ = hostBuilder.Logging.AddProvider(loggerProvider);
        _ = hostBuilder.ConfigureWinFormsShell<ThrowingDisposeTestShell>();
        using var host = hostBuilder.Build();
        var context = host.Services.GetRequiredService<IWinFormsContext>();

        await host.StartAsync().WaitAsync(Timeout);
        await WaitForAsync(() => context.IsRunning && context.Dispatcher is not null);

        await host.StopAsync().WaitAsync(Timeout);
        await WaitForAsync(() => !context.IsRunning);

        var cleanupLogEntry = loggerProvider.FindEntry(FormCleanupFailedEventId);
        await Assert.That(cleanupLogEntry is not null).IsTrue();
        await Assert.That(cleanupLogEntry!.LogLevel).IsEqualTo(LogLevel.Warning);
        await Assert.That(cleanupLogEntry.Exception).IsTypeOf<InvalidOperationException>();
    }

    /// <summary>Verifies the multiple-shell application context path.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task HostedService_WithMultipleShells_StartsAndStopsMessageLoop()
    {
        var hostBuilder = Host.CreateApplicationBuilder();
        _ = hostBuilder.ConfigureWinForms(static context => context.EnableVisualStyles = false);
        _ = hostBuilder.Services.AddSingleton<IWinFormsShell, AutoClosingTestShell>();
        _ = hostBuilder.Services.AddSingleton<IWinFormsShell, AutoClosingTestShell>();
        using var host = hostBuilder.Build();
        var context = host.Services.GetRequiredService<IWinFormsContext>();

        await host.StartAsync().WaitAsync(Timeout);
        await WaitForAsync(() => context.IsRunning && context.Dispatcher is not null);

        await WaitForAsync(() => !context.IsRunning);
        await host.StopAsync().WaitAsync(Timeout);
    }

    /// <summary>Waits for an asynchronous UI-state condition without blocking a test runner thread.</summary>
    /// <param name="condition">The condition to wait for.</param>
    /// <returns>A task that completes when the condition is met.</returns>
    private static async Task WaitForAsync(Func<bool> condition)
    {
        var deadline = SystemTimeProvider.GetUtcNow() + Timeout;
        using var timer = new PeriodicTimer(PollingInterval);
        while (!condition())
        {
            if (SystemTimeProvider.GetUtcNow() >= deadline)
            {
                throw new TimeoutException("The Windows Forms UI operation did not complete within the allotted timeout.");
            }

            _ = await timer.WaitForNextTickAsync();
        }
    }

    /// <summary>Captures logger entries emitted by the hosted service.</summary>
    private sealed class RecordingLoggerProvider : ILoggerProvider
    {
        /// <summary>Stores the recorded logging events.</summary>
        private readonly List<LogEntry> _entries = [];

        /// <inheritdoc />
        public ILogger CreateLogger(string categoryName) => new RecordingLogger(_entries);

        /// <inheritdoc />
        public void Dispose()
        {
        }

        /// <summary>Finds the first recorded logging event with the specified identifier.</summary>
        /// <param name="eventId">The event identifier to find.</param>
        /// <returns>The matching logging event, or <see langword="null"/> when none was recorded.</returns>
        public LogEntry? FindEntry(int eventId)
        {
            lock (_entries)
            {
                foreach (var entry in _entries)
                {
                    if (entry.EventId.Id == eventId)
                    {
                        return entry;
                    }
                }

                return null;
            }
        }

        /// <summary>Records log entries for one logger category.</summary>
        /// <param name="entries">The shared collection of recorded entries.</param>
        private sealed class RecordingLogger(List<LogEntry> entries) : ILogger
        {
            /// <inheritdoc />
            public IDisposable? BeginScope<TState>(TState state)
                where TState : notnull => null;

            /// <inheritdoc />
            public bool IsEnabled(LogLevel logLevel) => true;

            /// <inheritdoc />
            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
                lock (entries)
                {
                    entries.Add(new(logLevel, eventId, exception));
                }
            }
        }
    }

    /// <summary>Represents a captured logging event.</summary>
    /// <param name="LogLevel">The severity of the logging event.</param>
    /// <param name="EventId">The event identifier of the logging event.</param>
    /// <param name="Exception">The exception associated with the logging event.</param>
    private sealed record LogEntry(LogLevel LogLevel, EventId EventId, Exception? Exception);
}
