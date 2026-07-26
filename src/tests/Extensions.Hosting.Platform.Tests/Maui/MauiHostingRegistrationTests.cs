// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Maui.Controls;
using ReactiveMarbles.Extensions.Hosting.Maui;
using ReactiveMarbles.Extensions.Hosting.Maui.Internals;

namespace Extensions.Hosting.Maui.Platform.Tests;

/// <summary>Tests MAUI hosting registration behavior that does not require a running UI loop.</summary>
public class MauiHostingRegistrationTests
{
    /// <summary>Verifies that configuring the MAUI lifetime preserves the host builder.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task UseMauiLifetime_ReturnsConfiguredApplicationBuilder()
    {
        var builder = Host.CreateApplicationBuilder();

        var result = builder.UseMauiLifetime();

        await Assert.That(ReferenceEquals(result, builder)).IsTrue();
    }

    /// <summary>Verifies that MAUI configuration registers its lifetime and hosted service once.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureMaui_RegistersCoreServicesOnlyOnce()
    {
        var builder = Host.CreateApplicationBuilder();

        _ = builder.ConfigureMaui();
        _ = builder.ConfigureMaui();

        await Assert.That(CountRegistrations(builder.Services, typeof(IMauiContext), null)).IsEqualTo(1);
        await Assert.That(CountRegistrations(builder.Services, typeof(IHostedService), null)).IsEqualTo(1);
    }

    /// <summary>Verifies that null application builders are rejected.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureMaui_WithNullApplicationBuilder_ThrowsArgumentNullException()
    {
        static IHostApplicationBuilder? GetNullBuilder() => null;

        await Assert.That(static () => GetNullBuilder()!.ConfigureMaui()).Throws<ArgumentNullException>();
    }

    /// <summary>Verifies application-builder configuration registers application, page, shell, and context services.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureMaui_WithApplicationAndShell_RegistersConfiguredServices()
    {
        var builder = Host.CreateApplicationBuilder();

        var result = builder.ConfigureMaui(static maui =>
        {
            maui.ApplicationType = typeof(TestMauiApplication);
            _ = maui.AddSingletonPage<TestMauiShell>();
            _ = maui.AddSingletonPage<ContentPage>();
            _ = maui.ConfigureContext(static context => context.IsLifetimeLinked = true);
        });
        await using var serviceProvider = builder.Services.BuildServiceProvider();

        await Assert.That(result).IsSameReferenceAs(builder);
        await Assert.That(serviceProvider.GetRequiredService<IMauiContext>().IsLifetimeLinked).IsTrue();
        await Assert.That(CountRegistrations(builder.Services, typeof(TestMauiApplication), null)).IsEqualTo(1);
        await Assert.That(CountRegistrations(builder.Services, typeof(IMauiShell), null)).IsEqualTo(1);
        await Assert.That(CountRegistrations(builder.Services, typeof(ContentPage), null)).IsEqualTo(1);
    }

    /// <summary>Verifies the application-builder shell and base-application convenience paths register their services.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureMauiShell_WithApplicationBuilder_RegistersShellAndBaseApplication()
    {
        var builder = Host.CreateApplicationBuilder();

        var shellResult = builder.ConfigureMauiShell<TestMauiShell>();
        var applicationResult = builder.ConfigureMaui(static maui => maui.ApplicationType = typeof(Application));
        await using var serviceProvider = builder.Services.BuildServiceProvider();
        using var mauiThread = serviceProvider.GetRequiredService<MauiThread>();

        await Assert.That(shellResult).IsSameReferenceAs(builder);
        await Assert.That(applicationResult).IsSameReferenceAs(builder);
        await Assert.That(CountRegistrations(builder.Services, typeof(Application), null)).IsEqualTo(1);
    }

    /// <summary>Verifies invalid application types are rejected by application-builder configuration.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureMaui_WithInvalidApplicationType_ThrowsArgumentException()
    {
        var builder = Host.CreateApplicationBuilder();

        await Assert.That(() => builder.ConfigureMaui(static maui => maui.ApplicationType = typeof(string))).Throws<ArgumentException>();
    }

    /// <summary>Verifies legacy host-builder configuration is materialized during host construction.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureMaui_WithLegacyHostBuilder_RegistersConfiguredServices()
    {
        var builder = Host.CreateDefaultBuilder();

        var lifetimeResult = builder.UseMauiLifetime();
        var configurationResult = builder.ConfigureMaui(static maui =>
        {
            maui.ApplicationType = typeof(TestMauiApplication);
            _ = maui.AddSingletonPage<TestMauiShell>();
            _ = maui.ConfigureContext(static context => context.IsLifetimeLinked = true);
        });
        using var host = builder.Build();

        await Assert.That(lifetimeResult).IsSameReferenceAs(builder);
        await Assert.That(configurationResult).IsSameReferenceAs(builder);
        await Assert.That(host.Services).IsNotNull();
    }

    /// <summary>Verifies the legacy convenience overload preserves its builder.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureMaui_WithoutCustomization_ReturnsLegacyHostBuilder()
    {
        var builder = Host.CreateDefaultBuilder();

        var result = builder.ConfigureMaui();

        await Assert.That(result).IsSameReferenceAs(builder);
    }

    /// <summary>Verifies legacy shell configuration and null legacy builders preserve their documented behavior.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureMauiShell_WithLegacyHostBuilder_RegistersShellAndNullReturnsNull()
    {
        var builder = Host.CreateDefaultBuilder();
        IHostBuilder? nullBuilder = null;

        var result = builder.ConfigureMauiShell<TestMauiShell>();
        using var host = builder.Build();

        await Assert.That(result).IsSameReferenceAs(builder);
        await Assert.That(host.Services.GetRequiredService<IMauiContext>()).IsNotNull();
        await Assert.That(nullBuilder!.UseMauiLifetime()).IsNull();
        await Assert.That(nullBuilder!.ConfigureMauiShell<TestMauiShell>()).IsNull();
    }

    /// <summary>Counts registrations with matching types.</summary>
    /// <param name="services">The service registrations to inspect.</param>
    /// <param name="serviceType">The optional expected service type.</param>
    /// <param name="implementationType">The optional expected implementation type.</param>
    /// <returns>The matching registration count.</returns>
    private static int CountRegistrations(IEnumerable<ServiceDescriptor> services, Type? serviceType, Type? implementationType)
    {
        var count = 0;
        foreach (var descriptor in services)
        {
            if ((serviceType is null || descriptor.ServiceType == serviceType) && (implementationType is null || descriptor.ImplementationType == implementationType))
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>Represents an application type used exclusively for hosting registration tests.</summary>
    public sealed class TestMauiApplication : Application;

    /// <summary>Represents a shell type used exclusively for hosting registration tests.</summary>
    public sealed class TestMauiShell : ContentPage, IMauiShell;
}
