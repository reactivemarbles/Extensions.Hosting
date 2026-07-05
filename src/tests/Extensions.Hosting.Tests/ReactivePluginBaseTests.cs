// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactiveMarbles.Extensions.Hosting.Reactive.Plugins;

namespace Extensions.Hosting.Tests;

/// <summary>Contains tests for reactive shim plugin base hosted-service registration helpers.</summary>
public class ReactivePluginBaseTests
{
    /// <summary>Verifies that PluginBase with one hosted service registers that service.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureHost_WithOneHostedService_RegistersService()
    {
        var services = new ServiceCollection();
        var plugin = new PluginBase<FirstReactiveHostedService>();

        plugin.ConfigureHost(new object(), services);

        await Assert.That(services.Any(IsFirstHostedServiceRegistration)).IsTrue();
    }

    /// <summary>Verifies that PluginBase with two hosted services registers both services.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureHost_WithTwoHostedServices_RegistersServices()
    {
        var services = new ServiceCollection();
        var plugin = new PluginBase<FirstReactiveHostedService, SecondReactiveHostedService>();

        plugin.ConfigureHost(new object(), services);

        await Assert.That(services.Any(IsFirstHostedServiceRegistration)).IsTrue();
        await Assert.That(services.Any(IsSecondHostedServiceRegistration)).IsTrue();
    }

    /// <summary>Verifies that PluginBase with three hosted services registers all services.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureHost_WithThreeHostedServices_RegistersServices()
    {
        var services = new ServiceCollection();
        var plugin = new PluginBase<FirstReactiveHostedService, SecondReactiveHostedService, ThirdReactiveHostedService>();

        plugin.ConfigureHost(new object(), services);

        await Assert.That(services.Any(IsFirstHostedServiceRegistration)).IsTrue();
        await Assert.That(services.Any(IsSecondHostedServiceRegistration)).IsTrue();
        await Assert.That(services.Any(IsThirdHostedServiceRegistration)).IsTrue();
    }

    /// <summary>Returns a value indicating whether the descriptor registers the first hosted service.</summary>
    /// <param name="serviceDescriptor">The service descriptor to inspect.</param>
    /// <returns>True when the descriptor registers the first hosted service.</returns>
    private static bool IsFirstHostedServiceRegistration(ServiceDescriptor serviceDescriptor) =>
        IsHostedServiceRegistration<FirstReactiveHostedService>(serviceDescriptor);

    /// <summary>Returns a value indicating whether the descriptor registers the second hosted service.</summary>
    /// <param name="serviceDescriptor">The service descriptor to inspect.</param>
    /// <returns>True when the descriptor registers the second hosted service.</returns>
    private static bool IsSecondHostedServiceRegistration(ServiceDescriptor serviceDescriptor) =>
        IsHostedServiceRegistration<SecondReactiveHostedService>(serviceDescriptor);

    /// <summary>Returns a value indicating whether the descriptor registers the third hosted service.</summary>
    /// <param name="serviceDescriptor">The service descriptor to inspect.</param>
    /// <returns>True when the descriptor registers the third hosted service.</returns>
    private static bool IsThirdHostedServiceRegistration(ServiceDescriptor serviceDescriptor) =>
        IsHostedServiceRegistration<ThirdReactiveHostedService>(serviceDescriptor);

    /// <summary>Returns a value indicating whether the descriptor registers the requested hosted service type.</summary>
    /// <typeparam name="T">The hosted service implementation type.</typeparam>
    /// <param name="serviceDescriptor">The service descriptor to inspect.</param>
    /// <returns>True when the descriptor registers the requested hosted service type.</returns>
    private static bool IsHostedServiceRegistration<T>(ServiceDescriptor serviceDescriptor)
        where T : class, IHostedService =>
        serviceDescriptor.ServiceType == typeof(IHostedService) &&
        serviceDescriptor.ImplementationType == typeof(T);

    /// <summary>First hosted service used for reactive registration tests.</summary>
    public sealed class FirstReactiveHostedService : ReactiveHostedService;

    /// <summary>Second hosted service used for reactive registration tests.</summary>
    public sealed class SecondReactiveHostedService : ReactiveHostedService;

    /// <summary>Third hosted service used for reactive registration tests.</summary>
    public sealed class ThirdReactiveHostedService : ReactiveHostedService;

    /// <summary>Base hosted service used for reactive registration tests.</summary>
    public abstract class ReactiveHostedService : IHostedService
    {
        /// <inheritdoc />
        public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
