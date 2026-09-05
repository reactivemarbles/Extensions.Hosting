// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Hosting;
using Microsoft.UI.Xaml;
using ReactiveMarbles.Extensions.Hosting.Maui;
using ReactiveMarbles.Extensions.Hosting.Maui.Internals;
using WinRT;
using MauiApplication = Microsoft.Maui.Controls.Application;
using MauiIApplication = Microsoft.Maui.IApplication;
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
        await Assert.That(outcome.Results.ConcreteContextDispatcherAvailable).IsTrue();
        await Assert.That(outcome.Results.HostedServiceStoppedApplication).IsTrue();
    }

    /// <summary>Runs native MAUI verification within a WinUI application loop.</summary>
    /// <param name="completion">The completion source that receives verification results.</param>
    private static void RunNativeApplication(TaskCompletionSource<(Exception? Exception, NativeResults? Results)> completion)
    {
        Exception? capturedException = null;
        NativeResults? results = null;

        try
        {
            ComWrappersSupport.InitializeComWrappers();
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
        var internalNativeResult = internalBuilder.UseMauiApp(static _ => new TestMauiApplication());
        var internalExtensionResult = ((IMauiBuilder)internalBuilder).UseMauiApp(static _ => new TestMauiApplication(), static _ => { });
        var internalInstanceBuilder = new MauiBuilder();
        var internalInstanceResult = internalInstanceBuilder.UseMauiApp(application, static _ => { });
        var externalTypeBuilder = new ExternalMauiBuilder();
        var externalTypeResult = externalTypeBuilder.UseMauiApp(static _ => new TestMauiApplication());
        var externalInstanceBuilder = new ExternalMauiBuilder();
        var externalInstanceResult = externalInstanceBuilder.UseMauiApp(application);

        var hostBuilder = Host.CreateApplicationBuilder();
        _ = hostBuilder.ConfigureMaui(maui =>
        {
            maui.ApplicationType = typeof(TestMauiApplication);
            maui.Application = application;
            _ = maui.AddSingletonPage(typeof(TestMauiShell));
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
        var applicationExitObserved = ObserveApplicationExit(starter, fallbackApplication);
        var concreteContextDispatcherAvailable = new MauiContext().Dispatcher is not null;
        var hostedServiceStoppedApplication = StopHostedService(application);

        return new(
            InternalBuilderConfigured(
                internalBuilder,
                internalNativeResult,
                internalExtensionResult,
                internalInstanceBuilder,
                internalInstanceResult,
                ResolveMauiApplicationFromServices(internalInstanceBuilder, application),
                application),
            ReferenceEquals(externalTypeResult, externalTypeBuilder) && externalTypeBuilder.ApplicationType == typeof(TestMauiApplication),
            ReferenceEquals(externalInstanceResult, externalInstanceBuilder)
                && ReferenceEquals(externalInstanceBuilder.Application, application)
                && ResolveMauiApplicationFromServices(externalInstanceBuilder, application),
            ReferenceEquals(hostServices.GetRequiredService<TestMauiApplication>(), application)
                && ReferenceEquals(hostServices.GetRequiredService<MauiApplication>(), application),
            ReferenceEquals(matchingBuilder.Application, application),
            mismatchedBuilder.Application is null,
            ReferenceEquals(configuredBuilder.Application, application),
            ReferenceEquals(resolvedApplication, application),
            fallbackApplication is not null,
            hostServices.GetRequiredService<IMauiShell>() is TestMauiShell,
            applicationExitObserved,
            concreteContextDispatcherAvailable,
            hostedServiceStoppedApplication);
    }

    /// <summary>Checks whether the internal builder paths recorded and resolved their configured application.</summary>
    /// <param name="internalBuilder">The internal builder configured with an application factory.</param>
    /// <param name="internalNativeResult">The result returned from the concrete internal builder overload.</param>
    /// <param name="internalExtensionResult">The result returned from the interface overload.</param>
    /// <param name="internalInstanceBuilder">The internal builder configured with an application instance.</param>
    /// <param name="internalInstanceResult">The result returned from the internal instance overload.</param>
    /// <param name="internalApplicationResolved">Whether the internal MAUI service collection resolved the application.</param>
    /// <param name="application">The expected application instance.</param>
    /// <returns>true when all internal builder paths preserve the configured application; otherwise, false.</returns>
    private static bool InternalBuilderConfigured(
        MauiBuilder internalBuilder,
        IMauiBuilder internalNativeResult,
        IMauiBuilder internalExtensionResult,
        MauiBuilder internalInstanceBuilder,
        IMauiBuilder internalInstanceResult,
        bool internalApplicationResolved,
        MauiApplication application) =>
        ReferenceEquals(internalNativeResult, internalBuilder)
        && ReferenceEquals(internalExtensionResult, internalBuilder)
        && internalBuilder.ApplicationType == typeof(TestMauiApplication)
        && ReferenceEquals(internalInstanceResult, internalInstanceBuilder)
        && ReferenceEquals(internalInstanceBuilder.Application, application)
        && ReferenceEquals(internalInstanceBuilder.ApplicationFactory!(new ServiceCollection().BuildServiceProvider()), application)
        && internalApplicationResolved;

    /// <summary>Checks whether the MAUI service collection resolves the expected application instance.</summary>
    /// <param name="mauiBuilder">The MAUI builder whose public service collection is inspected.</param>
    /// <param name="application">The expected application instance.</param>
    /// <returns>true when the registered application service resolves the expected instance; otherwise, false.</returns>
    private static bool ResolveMauiApplicationFromServices(IMauiBuilder mauiBuilder, MauiApplication application)
    {
        using var serviceProvider = mauiBuilder.MauiAppBuilder.Services.BuildServiceProvider();
        return ReferenceEquals(serviceProvider.GetRequiredService<MauiIApplication>(), application);
    }

    /// <summary>Observes application exit through production and composed modal-pop subscriptions.</summary>
    /// <param name="starter">The application starter to verify.</param>
    /// <param name="application">The application instance to observe.</param>
    /// <returns>true when application exit was observed; otherwise, false.</returns>
    private static bool ObserveApplicationExit(MauiApplicationStarter starter, MauiApplication application)
    {
        var applicationExitObserved = false;
        starter.RegisterApplicationExit(application, () => applicationExitObserved = true);
        MauiApplicationStarter.RegisterApplicationExit(
            handler => handler(application, new(new ContentPage())),
            () => applicationExitObserved = true);
        return applicationExitObserved;
    }

    /// <summary>Stops the hosted service with a running context and a real application.</summary>
    /// <param name="application">The application to stop.</param>
    /// <returns>true when shutdown completes through the dispatcher.</returns>
    private static bool StopHostedService(MauiApplication application)
    {
        var context = new NativeMauiContext(application);
        var service = new MauiHostedService(
            NullLogger<MauiHostedService>.Instance,
            new MauiThreadStarter(static () => { }),
            context);

        var stopTask = service.StopAsync(CancellationToken.None);
        if (!stopTask.IsCompletedSuccessfully)
        {
            throw (Exception?)stopTask.Exception ?? new InvalidOperationException("The hosted-service shutdown did not complete synchronously.");
        }

        return context.Dispatched;
    }

    /// <summary>Provides a MAUI shell for native registration tests.</summary>
    public sealed class TestMauiShell : ContentPage, IMauiShell;

    /// <summary>Provides a MAUI application for native registration tests.</summary>
    private sealed class TestMauiApplication : MauiApplication;

    /// <summary>Provides a WinUI application that owns the native test loop.</summary>
    private sealed class RuntimeTestWinUIApplication : WinUIApplication;

    /// <summary>Provides a running MAUI context backed by a synchronous dispatcher.</summary>
    private sealed class NativeMauiContext : IMauiContext
    {
        /// <summary>Initializes a new instance of the <see cref="NativeMauiContext"/> class.</summary>
        /// <param name="application">The application owned by the context.</param>
        public NativeMauiContext(MauiApplication application)
        {
            MauiApplication = application;
            Dispatcher = new NativeDispatcher(MarkDispatched);
        }

        /// <inheritdoc />
        public bool IsLifetimeLinked { get; set; }

        /// <inheritdoc />
        public bool IsRunning { get; set; } = true;

        /// <inheritdoc />
        public MauiApplication? MauiApplication { get; set; }

        /// <inheritdoc />
        public IDispatcher? Dispatcher { get; }

        /// <summary>Gets a value indicating whether shutdown was dispatched.</summary>
        public bool Dispatched { get; private set; }

        /// <summary>Marks the shutdown callback as dispatched.</summary>
        private void MarkDispatched() =>
            Dispatched = true;

        /// <summary>Dispatches callbacks synchronously for native hosted-service tests.</summary>
        /// <param name="markDispatched">The callback that records dispatch.</param>
        private sealed class NativeDispatcher(Action markDispatched) : IDispatcher
        {
            /// <inheritdoc />
            public bool IsDispatchRequired => false;

            /// <inheritdoc />
            public bool Dispatch(Action action)
            {
                markDispatched();
                action();
                return true;
            }

            /// <inheritdoc />
            public bool DispatchDelayed(TimeSpan delay, Action action) =>
                Dispatch(action);

            /// <inheritdoc />
            public IDispatcherTimer CreateTimer() =>
                throw new NotSupportedException();
        }
    }

    /// <summary>Represents a public-interface MAUI builder implementation used to verify extension fallback behavior.</summary>
    private sealed class ExternalMauiBuilder : IMauiBuilder
    {
        /// <inheritdoc />
        public Type? ApplicationType { get; set; }

        /// <inheritdoc />
        public MauiApplication? Application { get; set; }

        /// <inheritdoc />
        public Func<IServiceProvider, MauiApplication>? ApplicationFactory { get; set; }

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
    /// <param name="ConcreteContextDispatcherAvailable">Whether the concrete context exposed the current application dispatcher.</param>
    /// <param name="HostedServiceStoppedApplication">Whether hosted-service shutdown dispatched through a real application.</param>
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
        bool ApplicationExitObserved,
        bool ConcreteContextDispatcherAvailable,
        bool HostedServiceStoppedApplication);
}
