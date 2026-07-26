// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactiveMarbles.Extensions.Hosting.Wpf;
using ReactiveMarbles.Extensions.Hosting.Wpf.Internals;

namespace Extensions.Hosting.Wpf.SingleShellCoverage.Tests;

/// <summary>Verifies the hosted WPF single-shell lifecycle in an isolated executable process.</summary>
[NotInParallel]
public sealed partial class SingleShellLifecycleCoverageTests
{
    /// <summary>Gets the Win32 message that closes a window.</summary>
    private const uint CloseWindowMessage = 0x0010;

    /// <summary>Gets the Win32 dialog window class name.</summary>
    private const string DialogWindowClass = "#32770";

    /// <summary>Gets the interval between searches for the WPF message box.</summary>
    private static readonly TimeSpan DialogPollInterval = TimeSpan.FromMilliseconds(20);

    /// <summary>Gets the maximum duration allowed for WPF application startup and shutdown.</summary>
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

    /// <summary>Verifies a single shell window is used as the WPF application's main window.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task StartAsync_SingleShellRunsApplicationWithRegisteredWindow()
    {
        var builder = Host.CreateApplicationBuilder();
        _ = builder.ConfigureWpf(static wpfBuilder =>
        {
            _ = wpfBuilder.UseApplication<SingleShellApplication>();
            _ = wpfBuilder.UseWindow<SingleShellWindow>();
        });
        using var host = builder.Build();

        try
        {
            await host.StartAsync().WaitAsync(Timeout);
            var dispatcher = await SingleShellApplication.Started.Task.WaitAsync(Timeout);
            var context = host.Services.GetRequiredService<IWpfContext>();
            var mainWindowIsShell = await context.Dispatcher.InvokeAsync(
                static () => Application.Current?.MainWindow is SingleShellWindow);

            await Assert.That(context.WpfApplication).IsTypeOf<SingleShellApplication>();
            await Assert.That(mainWindowIsShell).IsTrue();
            await Assert.That(context.Dispatcher).IsSameReferenceAs(dispatcher);

            var wpfApplication = context.WpfApplication
                ?? throw new InvalidOperationException("The WPF application was not initialized.");
            using var noApplicationThread = new ExposedWpfThread(host.Services);
            context.WpfApplication = null;
            noApplicationThread.RunUiThreadStart();
            context.WpfApplication = wpfApplication;
            using var runningThread = new ExposedWpfThread(host.Services);
            runningThread.RunUiThreadStart();

            await context.Dispatcher.InvokeAsync(
                () => wpfApplication.StartupUri = new("Unused.xaml", UriKind.Relative));
            var messageBoxCloserStarted = new TaskCompletionSource(
                TaskCreationOptions.RunContinuationsAsynchronously);
            var messageBoxClosed = new TaskCompletionSource(
                TaskCreationOptions.RunContinuationsAsynchronously);
            var messageBoxCloser = new Thread(
                () => CloseMessageBox(messageBoxCloserStarted, messageBoxClosed))
                { IsBackground = true };
            messageBoxCloser.Start();
            await messageBoxCloserStarted.Task.WaitAsync(Timeout);
            runningThread.RunUiThreadStart();
            await messageBoxClosed.Task.WaitAsync(Timeout);
        }
        finally
        {
            await host.StopAsync().WaitAsync(Timeout);
        }
    }

    /// <summary>Closes the WPF warning message box after its native dialog is created.</summary>
    /// <param name="started">The signal completed after native window lookup is available.</param>
    /// <param name="closed">The signal completed after the native dialog is closed.</param>
    private static void CloseMessageBox(
        TaskCompletionSource started,
        TaskCompletionSource closed)
    {
        using var cancellationTokenSource = new CancellationTokenSource(Timeout);
        _ = FindCurrentProcessDialog();
        _ = started.TrySetResult();

        do
        {
            var dialogHandle = FindCurrentProcessDialog();
            if (dialogHandle != nint.Zero
                && NativeMethods.PostMessage(
                    dialogHandle,
                    CloseWindowMessage,
                    nuint.Zero,
                    nint.Zero))
            {
                _ = closed.TrySetResult();
                return;
            }
        }
        while (!cancellationTokenSource.Token.WaitHandle.WaitOne(DialogPollInterval));

        _ = closed.TrySetException(
            new TimeoutException("The WPF warning message box was not created."));
    }

    /// <summary>Finds a native dialog owned by the current test process.</summary>
    /// <returns>The native dialog handle, or zero when no matching dialog exists.</returns>
    private static nint FindCurrentProcessDialog()
    {
        var dialogHandle = nint.Zero;
        while ((dialogHandle = NativeMethods.FindWindowEx(
            nint.Zero,
            dialogHandle,
            DialogWindowClass,
            null)) != nint.Zero)
        {
            _ = NativeMethods.GetWindowThreadProcessId(dialogHandle, out var processId);
            if (processId == Environment.ProcessId)
            {
                return dialogHandle;
            }
        }

        return nint.Zero;
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

    /// <summary>Provides the native window operations required by the message-box harness.</summary>
    private static partial class NativeMethods
    {
        /// <summary>Finds a child or top-level native window.</summary>
        /// <param name="parentWindow">The optional parent window.</param>
        /// <param name="childAfter">The child window after which searching begins.</param>
        /// <param name="className">The native window class name.</param>
        /// <param name="windowName">The optional native window title.</param>
        /// <returns>The native window handle, or zero if no matching window exists.</returns>
        [LibraryImport("user32.dll", EntryPoint = "FindWindowExW", StringMarshalling = StringMarshalling.Utf16)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial nint FindWindowEx(
            nint parentWindow,
            nint childAfter,
            string? className,
            string? windowName);

        /// <summary>Gets the process that owns a native window.</summary>
        /// <param name="windowHandle">The native window handle.</param>
        /// <param name="processId">The owning process identifier.</param>
        /// <returns>The identifier of the thread that created the window.</returns>
        [LibraryImport("user32.dll")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial uint GetWindowThreadProcessId(
            nint windowHandle,
            out uint processId);

        /// <summary>Posts a message to a native window.</summary>
        /// <param name="windowHandle">The target native window handle.</param>
        /// <param name="message">The message identifier.</param>
        /// <param name="wordParameter">The message word parameter.</param>
        /// <param name="longParameter">The message long parameter.</param>
        /// <returns><see langword="true"/> when the message was posted; otherwise, <see langword="false"/>.</returns>
        [LibraryImport("user32.dll", EntryPoint = "PostMessageW")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool PostMessage(
            nint windowHandle,
            uint message,
            nuint wordParameter,
            nint longParameter);
    }
}
