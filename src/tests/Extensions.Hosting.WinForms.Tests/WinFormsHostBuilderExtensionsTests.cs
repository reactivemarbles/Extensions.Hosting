// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactiveMarbles.Extensions.Hosting.WinForms;

namespace Extensions.Hosting.WinForms.Tests;

/// <summary>Verifies Windows Forms service registration on generic host builders.</summary>
public sealed class WinFormsHostBuilderExtensionsTests
{
    /// <summary>Stores the expected number of configuration callbacks.</summary>
    private const int ExpectedConfigurationCallbackCount = 2;

    /// <summary>Verifies that application builders reject a null receiver.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ApplicationBuilderExtensions_NullBuilder_ThrowArgumentNullException()
    {
        IHostApplicationBuilder? hostBuilder = null;

        await Assert.That(() => hostBuilder!.UseWinFormsLifetime()).Throws<ArgumentNullException>();
        await Assert.That(() => hostBuilder!.ConfigureWinForms()).Throws<ArgumentNullException>();
        await Assert.That(() => hostBuilder!.ConfigureWinForms(typeof(TestForm))).Throws<ArgumentNullException>();
    }

    /// <summary>Verifies that application builders configure the shared context once and apply each requested configuration.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureWinForms_ApplicationBuilder_RegistersServicesOnceAndAppliesConfiguration()
    {
        var hostBuilder = Host.CreateApplicationBuilder();
        var configuredContexts = new List<IWinFormsContext>();

        void Configure(IWinFormsContext context)
        {
            context.EnableVisualStyles = false;
            configuredContexts.Add(context);
        }

        _ = hostBuilder.ConfigureWinForms(Configure);
        _ = hostBuilder.ConfigureWinForms(configuredContexts.Add);
        _ = hostBuilder.UseWinFormsLifetime();

        using var host = hostBuilder.Build();
        var context = host.Services.GetRequiredService<IWinFormsContext>();
        var hostedServiceCount = 0;
        foreach (var hostedService in host.Services.GetServices<IHostedService>())
        {
            _ = hostedService;
            hostedServiceCount++;
        }

        await Assert.That(context.EnableVisualStyles).IsFalse();
        await Assert.That(context.IsLifetimeLinked).IsTrue();
        await Assert.That(configuredContexts.Count).IsEqualTo(ExpectedConfigurationCallbackCount);
        await Assert.That(configuredContexts[0]).IsSameReferenceAs(context);
        await Assert.That(configuredContexts[1]).IsSameReferenceAs(context);
        await Assert.That(hostedServiceCount).IsEqualTo(1);
    }

    /// <summary>Verifies that configured shells are registered as both their concrete and shell service types.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureWinFormsShell_ApplicationBuilder_RegistersConcreteShellAndShellInterface()
    {
        var hostBuilder = Host.CreateApplicationBuilder();
        _ = hostBuilder.ConfigureWinFormsShell(typeof(TestShell));

        using var host = hostBuilder.Build();
        var shell = host.Services.GetRequiredService<TestShell>();

        await Assert.That(host.Services.GetRequiredService<IWinFormsShell>()).IsSameReferenceAs(shell);
    }

    /// <summary>Verifies that ordinary forms are not exposed as shells.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureWinForms_ApplicationBuilder_RegistersNonShellFormWithoutShellInterface()
    {
        var hostBuilder = Host.CreateApplicationBuilder();
        _ = hostBuilder.ConfigureWinForms(typeof(TestForm));

        using var host = hostBuilder.Build();

        await Assert.That(host.Services.GetRequiredService<TestForm>()).IsNotNull();
        await Assert.That(host.Services.GetService<IWinFormsShell>()).IsNull();
    }

    /// <summary>Verifies that legacy builders preserve null-compatible extension behavior.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task HostBuilderExtensions_NullBuilder_ReturnNull()
    {
        IHostBuilder? hostBuilder = null;

        await Assert.That(hostBuilder!.UseWinFormsLifetime()).IsNull();
        await Assert.That(hostBuilder!.ConfigureWinForms()).IsNull();
        await Assert.That(hostBuilder!.ConfigureWinForms(typeof(TestForm))).IsNull();
        await Assert.That(hostBuilder!.ConfigureWinFormsShell(typeof(TestShell))).IsNull();
    }

    /// <summary>Verifies that legacy builders configure a shared context and shell registrations.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureWinForms_HostBuilder_RegistersContextAndShell()
    {
        var hostBuilder = Host.CreateDefaultBuilder();
        _ = hostBuilder.ConfigureWinForms(static context => context.EnableVisualStyles = false);
        _ = hostBuilder.ConfigureWinFormsShell(typeof(TestShell));
        _ = hostBuilder.UseWinFormsLifetime();

        using var host = hostBuilder.Build();
        var context = host.Services.GetRequiredService<IWinFormsContext>();
        var shell = host.Services.GetRequiredService<TestShell>();

        await Assert.That(context.EnableVisualStyles).IsFalse();
        await Assert.That(context.IsLifetimeLinked).IsTrue();
        await Assert.That(host.Services.GetRequiredService<IWinFormsShell>()).IsSameReferenceAs(shell);
        var hostedServiceCount = 0;
        foreach (var hostedService in host.Services.GetServices<IHostedService>())
        {
            _ = hostedService;
            hostedServiceCount++;
        }

        await Assert.That(hostedServiceCount).IsEqualTo(1);
    }

    /// <summary>Verifies that legacy builders register ordinary forms without exposing them as shells.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureWinForms_HostBuilder_RegistersNonShellFormWithoutShellInterface()
    {
        var hostBuilder = Host.CreateDefaultBuilder();
        _ = hostBuilder.ConfigureWinForms(typeof(TestForm));

        using var host = hostBuilder.Build();

        await Assert.That(host.Services.GetRequiredService<TestForm>()).IsNotNull();
        await Assert.That(host.Services.GetService<IWinFormsShell>()).IsNull();
    }
}
