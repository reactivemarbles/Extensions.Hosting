// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using ReactiveMarbles.Extensions.Hosting.WinUI;
using ReactiveMarbles.Extensions.Hosting.WinUI.Internals;

namespace Extensions.Hosting.WinUI.Platform.Tests;

/// <summary>Tests WinUI hosting registration behavior that does not start a WinUI application.</summary>
public class WinUIHostingRegistrationTests
{
    /// <summary>Verifies that WinUI configuration registers core services and configured types.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureWinUI_RegistersCoreAndApplicationServices()
    {
        var builder = Host.CreateApplicationBuilder();

        var result = builder.ConfigureWinUI<TestWinUIApplication, TestWinUIWindow>();

        await Assert.That(ReferenceEquals(result, builder)).IsTrue();
        await Assert.That(CountRegistrations(builder.Services, typeof(IWinUIContext), null)).IsEqualTo(1);
        await Assert.That(HasRegistration(builder.Services, typeof(WinUIThread), null)).IsTrue();
        await Assert.That(HasRegistration(builder.Services, typeof(IHostedService), null)).IsTrue();
        await Assert.That(HasRegistration(builder.Services, typeof(TestWinUIApplication), null)).IsTrue();
        await Assert.That(HasRegistration(builder.Services, typeof(Application), null)).IsTrue();
    }

    /// <summary>Verifies that repeated WinUI configuration reuses its registered context.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureWinUI_WhenCalledTwice_RegistersOneContext()
    {
        var builder = Host.CreateApplicationBuilder();

        _ = builder.ConfigureWinUI<TestWinUIApplication, TestWinUIWindow>();
        _ = builder.ConfigureWinUI<TestWinUIApplication, TestWinUIWindow>();

        await Assert.That(CountRegistrations(builder.Services, typeof(IWinUIContext), null)).IsEqualTo(1);
        await Assert.That(CountRegistrations(builder.Services, typeof(IHostedService), null)).IsEqualTo(1);
    }

    /// <summary>Verifies that the generic host builder applies WinUI registrations when it is built.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureWinUI_WithLegacyHostBuilder_ConfiguresTheBuiltHost()
    {
        var builder = Host.CreateDefaultBuilder();

        var result = builder.ConfigureWinUI<TestWinUIApplication, TestWinUIWindow>();

        using var host = result!.Build();
        var context = host.Services.GetRequiredService<IWinUIContext>();

        await Assert.That(ReferenceEquals(result, builder)).IsTrue();
        await Assert.That(context.AppWindowType).IsEqualTo(typeof(TestWinUIWindow));
        await Assert.That(context.IsLifetimeLinked).IsTrue();
    }

    /// <summary>Verifies that configuring the base WinUI application type does not register a redundant application mapping.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureWinUI_WithBaseApplication_RegistersOneApplicationService()
    {
        var builder = Host.CreateApplicationBuilder();

        _ = builder.ConfigureWinUI<Application, TestWinUIWindow>();

        await Assert.That(CountRegistrations(builder.Services, typeof(Application), null)).IsEqualTo(1);
    }

    /// <summary>Verifies that a null application host builder produces the documented argument exception.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureWinUI_WithNullApplicationHostBuilder_ThrowsArgumentNullException() =>
        await Assert.That(static () => ((IHostApplicationBuilder)null!).ConfigureWinUI<TestWinUIApplication, TestWinUIWindow>())
            .Throws<ArgumentNullException>();

    /// <summary>Verifies that a null legacy host builder preserves the documented null result.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureWinUI_WithNullHostBuilder_ReturnsNull()
    {
        var result = ((IHostBuilder)null!).ConfigureWinUI<TestWinUIApplication, TestWinUIWindow>();

        await Assert.That(result).IsNull();
    }

    /// <summary>Determines whether a collection contains a registration with matching types.</summary>
    /// <param name="services">The service registrations to inspect.</param>
    /// <param name="serviceType">The optional expected service type.</param>
    /// <param name="implementationType">The optional expected implementation type.</param>
    /// <returns>true when the matching registration exists; otherwise, false.</returns>
    private static bool HasRegistration(IEnumerable<ServiceDescriptor> services, Type? serviceType, Type? implementationType) =>
        CountRegistrations(services, serviceType, implementationType) > 0;

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

    /// <summary>Provides a WinUI application type used solely for service registrations.</summary>
    public sealed class TestWinUIApplication : Application;

    /// <summary>Provides a WinUI window type used solely for service registrations.</summary>
    public sealed class TestWinUIWindow : Window;
}
