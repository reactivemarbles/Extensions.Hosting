// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
#if REACTIVE_SHIM
using ReactiveMarbles.Extensions.Hosting.Reactive.ReactiveUI;
#else
using ReactiveMarbles.Extensions.Hosting.ReactiveUI;
#endif
using ReactiveMarbles.Extensions.Hosting.Wpf;

namespace Extensions.Hosting.ReactiveUI.Wpf.Tests;

/// <summary>Verifies WPF host and ReactiveUI builder registrations without starting a WPF message loop.</summary>
[NotInParallel]
public sealed class WpfHostingCoverageTests
{
    /// <summary>Verifies application-builder WPF registration, context configuration, and lifetime configuration.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureWpf_ApplicationBuilderRegistersServicesAndConfiguresLifetime()
    {
        var builder = Host.CreateApplicationBuilder();
        IWpfContext? configuredContext = null;

        var configuredBuilder = builder.ConfigureWpf(wpfBuilder =>
        {
            _ = wpfBuilder.UseApplication(typeof(TestApplication));
            _ = wpfBuilder.UseWindow(typeof(TestShellWindow));
            _ = wpfBuilder.ConfigureContext(context =>
            {
                configuredContext = context;
                context.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            });
        });
        var lifetimeBuilder = builder.UseWpfLifetime();
        var explicitlyConfiguredLifetimeBuilder = builder.UseWpfLifetime(ShutdownMode.OnMainWindowClose);

        await Assert.That(configuredBuilder).IsSameReferenceAs(builder);
        await Assert.That(lifetimeBuilder).IsSameReferenceAs(builder);
        await Assert.That(explicitlyConfiguredLifetimeBuilder).IsSameReferenceAs(builder);
        await Assert.That(configuredContext).IsNotNull();
        await Assert.That(configuredContext!.ShutdownMode).IsEqualTo(ShutdownMode.OnMainWindowClose);
        await Assert.That(configuredContext.IsLifetimeLinked).IsTrue();
        await Assert.That(HasRegistration(builder.Services, typeof(IWpfContext))).IsTrue();
        await Assert.That(HasRegistration(builder.Services, typeof(IHostedService))).IsTrue();
        await Assert.That(HasRegistration(builder.Services, typeof(TestApplication))).IsTrue();
        await Assert.That(HasRegistration(builder.Services, typeof(TestShellWindow))).IsTrue();
        await Assert.That(HasRegistration(builder.Services, typeof(IWpfShell))).IsTrue();

        _ = builder.ConfigureWpf();

        await Assert.That(CountRegistrations(builder.Services, typeof(IWpfContext))).IsEqualTo(1);
        await Assert.That(CountRegistrations(builder.Services, typeof(IHostedService))).IsEqualTo(1);
    }

    /// <summary>Verifies generic host-builder WPF registration and lifetime configuration.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureWpf_HostBuilderRegistersContextWhenBuilt()
    {
        var hostBuilder = new HostBuilder();
        var configuredBuilder = hostBuilder.ConfigureWpf(static wpfBuilder =>
        {
            _ = wpfBuilder.UseApplication(typeof(TestApplication));
            _ = wpfBuilder.UseWindow(typeof(TestShellWindow));
        });
        var lifetimeBuilder = hostBuilder.UseWpfLifetime(ShutdownMode.OnExplicitShutdown);

        using var host = hostBuilder.Build();
        var context = host.Services.GetRequiredService<IWpfContext>();

        await Assert.That(configuredBuilder).IsSameReferenceAs(hostBuilder);
        await Assert.That(lifetimeBuilder).IsSameReferenceAs(hostBuilder);
        await Assert.That(context.ShutdownMode).IsEqualTo(ShutdownMode.OnExplicitShutdown);
        await Assert.That(context.IsLifetimeLinked).IsTrue();
    }

    /// <summary>Verifies WPF lifetime configuration reports a missing WPF configuration.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task UseWpfLifetime_WithoutWpfConfigurationThrowsNotSupportedException()
    {
        var applicationBuilder = Host.CreateApplicationBuilder();
        var hostBuilder = new HostBuilder();

        void ConfigureApplicationBuilderLifetime() => applicationBuilder.UseWpfLifetime();
        void BuildHost() => hostBuilder.UseWpfLifetime().Build();

        await Assert.That(ConfigureApplicationBuilderLifetime).Throws<NotSupportedException>();
        await Assert.That(BuildHost).Throws<NotSupportedException>();
    }

    /// <summary>Verifies builder extensions configure all supported builder properties.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task WpfBuilderExtensions_ConfigureBuilderProperties()
    {
        var builder = new RecordingWpfBuilder();
        _ = builder.UseApplication(typeof(TestApplication));
        _ = builder.UseWindow(typeof(TestShellWindow));
        var configuredBuilder = builder.ConfigureContext(static context => context.IsLifetimeLinked = true);

        await Assert.That(configuredBuilder).IsSameReferenceAs(builder);
        await Assert.That(builder.ApplicationType).IsEqualTo(typeof(TestApplication));
        await Assert.That(builder.Application).IsNull();
        await Assert.That(builder.WindowTypes[0]).IsEqualTo(typeof(TestShellWindow));
        await Assert.That(builder.ConfigureContextAction).IsNotNull();
    }

    /// <summary>Verifies invalid WPF application types are rejected during configuration.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureWpf_InvalidApplicationTypeThrowsArgumentException()
    {
        var builder = Host.CreateApplicationBuilder();

        void Act() => _ = builder.ConfigureWpf(static wpfBuilder => wpfBuilder.ApplicationType = typeof(string));

        await Assert.That(Act).Throws<ArgumentException>();
    }

    /// <summary>Verifies ReactiveUI configuration registers the WPF scheduler service for application builders.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureSplatForMicrosoftDependencyResolver_ApplicationBuilderRegistersWpfService()
    {
        var builder = Host.CreateApplicationBuilder();

        var configuredBuilder = builder.ConfigureSplatForMicrosoftDependencyResolver();

        await Assert.That(configuredBuilder).IsSameReferenceAs(builder);
        await Assert.That(CountRegistrations(builder.Services, typeof(IWpfService))).IsEqualTo(1);
        await Assert.That(GetRegistration(builder.Services, typeof(IWpfService)).Lifetime).IsEqualTo(ServiceLifetime.Singleton);
    }

    /// <summary>Verifies ReactiveUI configuration registers the WPF scheduler service for generic host builders.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureSplatForMicrosoftDependencyResolver_HostBuilderRegistersWpfService()
    {
        var hostBuilder = new HostBuilder();

        var configuredBuilder = hostBuilder.ConfigureSplatForMicrosoftDependencyResolver();
        using var host = hostBuilder.Build();

        await Assert.That(configuredBuilder).IsSameReferenceAs(hostBuilder);
        await Assert.That(CountServices(host.Services.GetServices<IWpfService>())).IsEqualTo(1);
    }

    /// <summary>Verifies mapping a host forwards its service provider to the supplied callback.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task MapSplatLocator_HostInvokesContainerFactoryWithServiceProvider()
    {
        using var host = Host.CreateApplicationBuilder().Build();
        IServiceProvider? receivedProvider = null;

        var mappedHost = host.MapSplatLocator(provider => receivedProvider = provider);

        await Assert.That(mappedHost).IsSameReferenceAs(host);
        await Assert.That(receivedProvider).IsSameReferenceAs(host.Services);
    }

    /// <summary>Determines whether a service collection contains a registration for the specified service type.</summary>
    /// <param name="services">The service collection to inspect.</param>
    /// <param name="serviceType">The service type to locate.</param>
    /// <returns><see langword="true"/> when a registration is present; otherwise, <see langword="false"/>.</returns>
    private static bool HasRegistration(IServiceCollection services, Type serviceType)
    {
        foreach (var descriptor in services)
        {
            if (descriptor.ServiceType == serviceType)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Counts registrations for the specified service type.</summary>
    /// <param name="services">The service collection to inspect.</param>
    /// <param name="serviceType">The service type to count.</param>
    /// <returns>The number of matching service registrations.</returns>
    private static int CountRegistrations(IServiceCollection services, Type serviceType)
    {
        var count = 0;
        foreach (var descriptor in services)
        {
            if (descriptor.ServiceType == serviceType)
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>Gets the service registration for the specified service type.</summary>
    /// <param name="services">The service collection to inspect.</param>
    /// <param name="serviceType">The service type to locate.</param>
    /// <returns>The matching service registration.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no registration exists for the specified service type.</exception>
    private static ServiceDescriptor GetRegistration(IServiceCollection services, Type serviceType)
    {
        foreach (var descriptor in services)
        {
            if (descriptor.ServiceType == serviceType)
            {
                return descriptor;
            }
        }

        throw new InvalidOperationException($"No registration found for {serviceType}.");
    }

    /// <summary>Counts values in the supplied service sequence.</summary>
    /// <param name="services">The services to count.</param>
    /// <returns>The number of supplied services.</returns>
    private static int CountServices(IEnumerable<IWpfService> services)
    {
        var count = 0;
        foreach (var service in services)
        {
            _ = service;
            count++;
        }

        return count;
    }

    /// <summary>Provides a shell window type used only for service-registration coverage.</summary>
    public sealed class TestShellWindow : Window, IWpfShell
    {
        /// <summary>Gets the shell window type registered by the WPF host builder.</summary>
        public static Type RegistrationType => typeof(TestShellWindow);
    }

    /// <summary>Provides a writable test implementation of <see cref="IWpfBuilder"/>.</summary>
    public sealed class RecordingWpfBuilder : IWpfBuilder
    {
        /// <inheritdoc />
        public Type? ApplicationType { get; set; }

        /// <inheritdoc />
        public Application? Application { get; set; }

        /// <inheritdoc />
        public IList<Type> WindowTypes { get; } = [];

        /// <inheritdoc />
        public Action<IWpfContext>? ConfigureContextAction { get; set; }
    }
}
