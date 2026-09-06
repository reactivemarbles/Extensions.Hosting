// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Avalonia;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactiveMarbles.Extensions.Hosting.Avalonia;
using ReactiveMarbles.Extensions.Hosting.Avalonia.Internals;

namespace Extensions.Hosting.Avalonia.Tests;

/// <summary>Verifies host configuration for Avalonia.</summary>
public sealed class AvaloniaHostingExtensionsTests
{
    /// <summary>Verifies application-builder customization is applied when the UI thread is resolved.</summary>
    /// <returns>A task representing the test.</returns>
    [Test]
    public async Task ConfigureAvalonia_ResolvedThread_AppliesAppBuilderConfiguration()
    {
        var configured = false;
        var builder = Host.CreateApplicationBuilder();
        _ = builder.ConfigureAvalonia(avalonia => avalonia.ConfigureAppBuilder(_ => configured = true));
        using var host = builder.Build();

        _ = host.Services.GetRequiredService<AvaloniaThread>();

        await Assert.That(configured).IsTrue();
    }

    /// <summary>Verifies service registration and context configuration through an application host builder.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureAvalonia_WithApplicationBuilder_RegistersConfiguredServices()
    {
        var builder = Host.CreateApplicationBuilder();
        var contextConfigured = false;
        _ = builder.ConfigureAvalonia(avaloniaBuilder =>
        {
            _ = avaloniaBuilder.UseApplication(typeof(TestApplication));
            _ = avaloniaBuilder.UseWindow(typeof(TestShellWindow));
            _ = avaloniaBuilder.UseWindow(typeof(Window));
            _ = avaloniaBuilder.ConfigureContext(context =>
            {
                context.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                contextConfigured = true;
            });
        });

        using var host = builder.Build();
        var context = host.Services.GetRequiredService<IAvaloniaContext>();
        var application = host.Services.GetRequiredService<Application>();
        var typedApplication = host.Services.GetRequiredService<TestApplication>();
        var hostedServices = host.Services.GetServices<IHostedService>();

        await Assert.That(contextConfigured).IsTrue();
        await Assert.That(context.ShutdownMode).IsEqualTo(ShutdownMode.OnExplicitShutdown);
        await Assert.That(application).IsSameReferenceAs(typedApplication);
        await Assert.That(CountAvaloniaHostedServices(hostedServices)).IsEqualTo(1);
    }

    /// <summary>Verifies repeated configuration shares the context and avoids duplicate hosting services.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureAvalonia_RepeatedApplicationBuilderCalls_ReusesContext()
    {
        var builder = Host.CreateApplicationBuilder();
        _ = builder.ConfigureAvalonia();
        _ = builder.ConfigureAvalonia(static avaloniaBuilder => avaloniaBuilder.UseApplication(typeof(Application)));

        using var host = builder.Build();
        var hostedServices = host.Services.GetServices<IHostedService>();

        await Assert.That(host.Services.GetRequiredService<IAvaloniaContext>()).IsNotNull();
        await Assert.That(host.Services.GetRequiredService<Application>()).IsNotNull();
        await Assert.That(CountAvaloniaHostedServices(hostedServices)).IsEqualTo(1);
    }

    /// <summary>Verifies an existing Avalonia application is preserved during service registration.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureAvalonia_WithCurrentApplication_RegistersProvidedInstance()
    {
        var builder = Host.CreateApplicationBuilder();
        var application = new TestApplication();
        _ = builder.ConfigureAvalonia(avaloniaBuilder => avaloniaBuilder.UseCurrentApplication(application));

        using var host = builder.Build();

        await Assert.That(host.Services.GetRequiredService<TestApplication>()).IsSameReferenceAs(application);
        await Assert.That(host.Services.GetRequiredService<Application>()).IsSameReferenceAs(application);
    }

    /// <summary>Verifies invalid application types fail when the host is built.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureAvalonia_WithInvalidApplicationType_ThrowsArgumentException()
    {
        var builder = Host.CreateApplicationBuilder();
        var configure = () => builder.ConfigureAvalonia(static avaloniaBuilder => avaloniaBuilder.ApplicationType = typeof(string));

        await Assert.That(configure).Throws<ArgumentException>();
    }

    /// <summary>Verifies lifecycle configuration requires Avalonia configuration first.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task UseAvaloniaLifetime_BeforeConfigureAvalonia_ThrowsNotSupportedException()
    {
        var applicationBuilder = Host.CreateApplicationBuilder();
        var applicationBuilderAction = () => applicationBuilder.UseAvaloniaLifetime();
        var hostBuilder = Host.CreateDefaultBuilder().UseAvaloniaLifetime();
        var hostBuilderAction = () => hostBuilder.Build();

        await Assert.That(applicationBuilderAction).Throws<NotSupportedException>();
        await Assert.That(hostBuilderAction).Throws<NotSupportedException>();
    }

    /// <summary>Verifies default and explicit lifetime configuration update the shared Avalonia context.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task UseAvaloniaLifetime_AfterConfigureAvalonia_UpdatesContext()
    {
        var applicationBuilder = Host.CreateApplicationBuilder();
        _ = applicationBuilder.ConfigureAvalonia();
        _ = applicationBuilder.UseAvaloniaLifetime(ShutdownMode.OnExplicitShutdown);

        using var applicationHost = applicationBuilder.Build();
        var applicationContext = applicationHost.Services.GetRequiredService<IAvaloniaContext>();

        var hostBuilder = Host.CreateDefaultBuilder().ConfigureAvalonia().UseAvaloniaLifetime();
        using var host = hostBuilder.Build();
        var context = host.Services.GetRequiredService<IAvaloniaContext>();

        await Assert.That(applicationContext.IsLifetimeLinked).IsTrue();
        await Assert.That(applicationContext.ShutdownMode).IsEqualTo(ShutdownMode.OnExplicitShutdown);
        await Assert.That(context.IsLifetimeLinked).IsTrue();
        await Assert.That(context.ShutdownMode).IsEqualTo(ShutdownMode.OnLastWindowClose);
    }

    /// <summary>Verifies host-builder configuration registers Avalonia application services.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureAvalonia_WithHostBuilder_RegistersApplication()
    {
        var hostBuilder = Host.CreateDefaultBuilder().ConfigureAvalonia(static avaloniaBuilder => avaloniaBuilder.UseApplication(typeof(TestApplication)));

        using var host = hostBuilder.Build();

        await Assert.That(host.Services.GetRequiredService<Application>()).IsTypeOf<TestApplication>();
    }

    /// <summary>Verifies host builder extensions guard null receivers.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task HostBuilderExtensions_WithNullBuilder_ThrowArgumentNullException()
    {
        IHostBuilder? builder = null;
        var configure = () => builder!.ConfigureAvalonia(static _ => { });
        var lifetime = () => builder!.UseAvaloniaLifetime(ShutdownMode.OnMainWindowClose);

        await Assert.That(configure).Throws<ArgumentNullException>();
        await Assert.That(lifetime).Throws<ArgumentNullException>();
    }

    /// <summary>Verifies application host builder lifetime extensions guard null receivers.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task HostApplicationBuilderLifetimeExtensions_WithNullBuilder_ThrowArgumentNullException()
    {
        IHostApplicationBuilder? builder = null;
        var lifetime = () => builder!.UseAvaloniaLifetime(ShutdownMode.OnMainWindowClose);

        await Assert.That(lifetime).Throws<ArgumentNullException>();
    }

    /// <summary>Counts Avalonia hosted services in a collection.</summary>
    /// <param name="services">The service collection to inspect.</param>
    /// <returns>The number of Avalonia hosted services.</returns>
    private static int CountAvaloniaHostedServices(IEnumerable<IHostedService> services)
    {
        var count = 0;
        foreach (var service in services)
        {
            if (service is AvaloniaHostedService)
            {
                count++;
            }
        }

        return count;
    }
}
