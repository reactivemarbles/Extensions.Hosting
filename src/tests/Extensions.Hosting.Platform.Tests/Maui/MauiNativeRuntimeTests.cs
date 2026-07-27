// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;
using Microsoft.UI.Xaml;
using ReactiveMarbles.Extensions.Hosting.Maui;
using ReactiveMarbles.Extensions.Hosting.Maui.Internals;
using MauiApplication = Microsoft.Maui.Controls.Application;
using WinUIApplication = Microsoft.UI.Xaml.Application;

namespace Extensions.Hosting.Maui.Platform.Tests;

/// <summary>Tests native Windows MAUI registration paths inside an initialized WinUI application loop.</summary>
public class MauiNativeRuntimeTests
{
    /// <summary>Defines the maximum time to await native application-loop completion.</summary>
    private static readonly TimeSpan NativeLoopTimeout = TimeSpan.FromSeconds(15);

    /// <summary>Verifies native MAUI builder, application-instance, capture, and starter paths.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    [NotInParallel]
    public async Task NativeRegistrationPaths_RunInsideWinUIApplication()
    {
        var completion = new TaskCompletionSource<(Exception? Exception, NativeResults? Results)>(TaskCreationOptions.RunContinuationsAsynchronously);
        var thread = new Thread(() => RunNativeApplication(completion));
        thread.IsBackground = true;
        thread.SetApartmentState(ApartmentState.STA);

        thread.Start();
        var outcome = await completion.Task.WaitAsync(NativeLoopTimeout);

        await Assert.That(outcome.Exception).IsNull();
        await Assert.That(outcome.Results).IsNotNull();
        await Assert.That(outcome.Results!.InternalBuilderConfigured).IsTrue();
        await Assert.That(outcome.Results.ExternalTypeBuilderConfigured).IsTrue();
        await Assert.That(outcome.Results.ExternalInstanceBuilderConfigured).IsTrue();
        await Assert.That(outcome.Results.RegisteredApplicationPreserved).IsTrue();
        await Assert.That(outcome.Results.CurrentApplicationCaptured).IsTrue();
        await Assert.That(outcome.Results.MismatchedApplicationIgnored).IsTrue();
        await Assert.That(outcome.Results.ExplicitApplicationPreserved).IsTrue();
        await Assert.That(outcome.Results.StarterResolvedApplication).IsTrue();
        await Assert.That(outcome.Results.StarterCreatedFallback).IsTrue();
        await Assert.That(outcome.Results.ShellMappingResolved).IsTrue();
        await Assert.That(outcome.Results.ApplicationExitObserved).IsTrue();
    }

    /// <summary>Runs native MAUI verification within a WinUI application loop.</summary>
    /// <param name="completion">The completion source that receives verification results.</param>
    private static void RunNativeApplication(TaskCompletionSource<(Exception? Exception, NativeResults? Results)> completion)
    {
        Exception? capturedException = null;
        NativeResults? results = null;

        try
        {
            WinUIApplication.Start(_ =>
            {
                var winUIApplication = new RuntimeTestWinUIApplication();
                try
                {
                    results = ExecuteNativePaths();
                }
                catch (Exception exception)
                {
                    capturedException = exception;
                }
                finally
                {
                    winUIApplication.Exit();
                }
            });
        }
        catch (Exception exception)
        {
            capturedException = exception;
        }

        completion.SetResult((capturedException, results));
    }

    /// <summary>Executes the native MAUI paths after the Windows XAML runtime has initialized.</summary>
    /// <returns>The observed native-path results.</returns>
    private static NativeResults ExecuteNativePaths()
    {
        var application = new TestMauiApplication();
        var internalBuilder = new MauiBuilder();
        var internalNativeResult = internalBuilder.UseMauiApp<TestMauiApplication>();
        var internalExtensionResult = ((IMauiBuilder)internalBuilder).UseMauiApp<TestMauiApplication>(static _ => { });
        var externalTypeBuilder = new ExternalMauiBuilder();
        var externalTypeResult = externalTypeBuilder.UseMauiApp<TestMauiApplication>();
        var externalInstanceBuilder = new ExternalMauiBuilder();
        var externalInstanceResult = externalInstanceBuilder.UseMauiApp(application);

        var hostBuilder = Host.CreateApplicationBuilder();
        _ = hostBuilder.ConfigureMaui(maui =>
        {
            maui.ApplicationType = typeof(TestMauiApplication);
            maui.Application = application;
            _ = maui.AddSingletonPage<TestMauiShell>();
        });
        using var hostServices = hostBuilder.Services.BuildServiceProvider();

        var matchingBuilder = new MauiBuilder(static _ => { }) { ApplicationType = typeof(TestMauiApplication) };
        var mismatchedBuilder = new MauiBuilder(static _ => { }) { ApplicationType = typeof(MauiApplication) };
        var configuredBuilder = new MauiBuilder(static _ => { }) { ApplicationType = typeof(TestMauiApplication), Application = application };
        MauiApplicationCapture.Capture(matchingBuilder, application);
        MauiApplicationCapture.Capture(mismatchedBuilder, application);
        MauiApplicationCapture.Capture(configuredBuilder, new TestMauiApplication());

        var registeredServices = new ServiceCollection().AddSingleton<MauiApplication>(application);
        using var registeredProvider = registeredServices.BuildServiceProvider();
        using var emptyProvider = new ServiceCollection().BuildServiceProvider();
        var starter = new MauiApplicationStarter();
        var resolvedApplication = starter.Create(registeredProvider);
        var fallbackApplication = starter.Create(emptyProvider);
        var applicationExitObserved = false;
        starter.RegisterApplicationExit(fallbackApplication, () => applicationExitObserved = true);
        MauiApplicationStarter.RegisterApplicationExit(
            handler => handler(fallbackApplication, new(new ContentPage())),
            () => applicationExitObserved = true);

        return new(
            ReferenceEquals(internalNativeResult, internalBuilder.MauiAppBuilder)
                && ReferenceEquals(internalExtensionResult, internalBuilder)
                && internalBuilder.ApplicationType == typeof(TestMauiApplication),
            ReferenceEquals(externalTypeResult, externalTypeBuilder) && externalTypeBuilder.ApplicationType == typeof(TestMauiApplication),
            ReferenceEquals(externalInstanceResult, externalInstanceBuilder) && ReferenceEquals(externalInstanceBuilder.Application, application),
            ReferenceEquals(hostServices.GetRequiredService<TestMauiApplication>(), application)
                && ReferenceEquals(hostServices.GetRequiredService<MauiApplication>(), application),
            ReferenceEquals(matchingBuilder.Application, application),
            mismatchedBuilder.Application is null,
            ReferenceEquals(configuredBuilder.Application, application),
            ReferenceEquals(resolvedApplication, application),
            fallbackApplication is not null,
            hostServices.GetRequiredService<IMauiShell>() is TestMauiShell,
            applicationExitObserved);
    }

    /// <summary>Provides a MAUI shell for native registration tests.</summary>
    public sealed class TestMauiShell : ContentPage, IMauiShell;

    /// <summary>Provides a MAUI application for native registration tests.</summary>
    private sealed class TestMauiApplication : MauiApplication;

    /// <summary>Provides a WinUI application that owns the native test loop.</summary>
    private sealed class RuntimeTestWinUIApplication : WinUIApplication;

    /// <summary>Represents a public-interface MAUI builder implementation used to verify extension fallback behavior.</summary>
    private sealed class ExternalMauiBuilder : IMauiBuilder
    {
        /// <inheritdoc />
        public Type? ApplicationType { get; set; }

        /// <inheritdoc />
        public MauiApplication? Application { get; set; }

        /// <inheritdoc />
        public IList<Type> PageTypes { get; } = [];

        /// <inheritdoc />
        public Action<IMauiContext>? ConfigureContextAction { get; set; }

        /// <inheritdoc />
        public MauiAppBuilder MauiAppBuilder { get; } = MauiApp.CreateBuilder();
    }

    /// <summary>Stores results observed inside the native application loop.</summary>
    /// <param name="InternalBuilderConfigured">Whether the internal builder used MAUI defaults.</param>
    /// <param name="ExternalTypeBuilderConfigured">Whether an external builder configured an application type.</param>
    /// <param name="ExternalInstanceBuilderConfigured">Whether an external builder configured an application instance.</param>
    /// <param name="RegisteredApplicationPreserved">Whether host registration preserved the configured application.</param>
    /// <param name="CurrentApplicationCaptured">Whether compatible current-application capture succeeded.</param>
    /// <param name="MismatchedApplicationIgnored">Whether incompatible current-application capture was ignored.</param>
    /// <param name="ExplicitApplicationPreserved">Whether capture preserved explicit application configuration.</param>
    /// <param name="StarterResolvedApplication">Whether the application starter resolved the registered application.</param>
    /// <param name="StarterCreatedFallback">Whether the application starter created its fallback application.</param>
    /// <param name="ShellMappingResolved">Whether the configured shell mapping resolved the registered shell page.</param>
    /// <param name="ApplicationExitObserved">Whether the registered application-exit callback observed a modal-pop event.</param>
    private sealed record NativeResults(
        bool InternalBuilderConfigured,
        bool ExternalTypeBuilderConfigured,
        bool ExternalInstanceBuilderConfigured,
        bool RegisteredApplicationPreserved,
        bool CurrentApplicationCaptured,
        bool MismatchedApplicationIgnored,
        bool ExplicitApplicationPreserved,
        bool StarterResolvedApplication,
        bool StarterCreatedFallback,
        bool ShellMappingResolved,
        bool ApplicationExitObserved);
}
