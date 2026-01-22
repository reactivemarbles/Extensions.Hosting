// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactiveMarbles.Extensions.Hosting.AppServices;
using TUnit;

namespace Extensions.Hosting.Tests;

/// <summary>
/// Contains unit tests for the HostBuilderApplicationExtensions class (SingleInstance functionality).
/// </summary>
public class HostBuilderApplicationExtensionsTests
{
    /// <summary>
    /// Verifies that ConfigureSingleInstance with IHostBuilder throws when hostBuilder is null.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureSingleInstance_IHostBuilder_WithNullHostBuilder_ThrowsArgumentNullException()
    {
        IHostBuilder? hostBuilder = null;
        var act = () => hostBuilder!.ConfigureSingleInstance(_ => { });
        await Assert.That(act).Throws<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that ConfigureSingleInstance with IHostApplicationBuilder throws when hostBuilder is null.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureSingleInstance_IHostApplicationBuilder_WithNullHostBuilder_ThrowsArgumentNullException()
    {
        IHostApplicationBuilder? hostBuilder = null;
        var act = () => hostBuilder!.ConfigureSingleInstance(_ => { });
        await Assert.That(act).Throws<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that ConfigureSingleInstance with mutexId returns the same IHostBuilder for chaining.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureSingleInstance_WithMutexId_ReturnsHostBuilder()
    {
        var hostBuilder = Host.CreateDefaultBuilder();
        var result = hostBuilder.ConfigureSingleInstance("test-mutex-" + Guid.NewGuid().ToString("N"));
        await Assert.That(result).IsNotNull();
        await Assert.That(result).IsEqualTo(hostBuilder);
    }

    /// <summary>
    /// Verifies that ConfigureSingleInstance with configure action returns the same IHostBuilder for chaining.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureSingleInstance_WithConfigureAction_ReturnsHostBuilder()
    {
        var hostBuilder = Host.CreateDefaultBuilder();
        var result = hostBuilder.ConfigureSingleInstance(builder =>
        {
            builder.MutexId = "test-mutex-" + Guid.NewGuid().ToString("N");
            builder.IsGlobal = false;
        });
        await Assert.That(result).IsNotNull();
        await Assert.That(result).IsEqualTo(hostBuilder);
    }

    /// <summary>
    /// Verifies that ConfigureSingleInstance configure action is invoked.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureSingleInstance_ConfigureActionIsInvoked()
    {
        var wasInvoked = false;
        var hostBuilder = Host.CreateDefaultBuilder();
        hostBuilder.ConfigureSingleInstance(builder =>
        {
            wasInvoked = true;
            builder.MutexId = "test-mutex-" + Guid.NewGuid().ToString("N");
        });

        // Build to trigger the service configuration
        using var host = hostBuilder.Build();
        await Assert.That(wasInvoked).IsTrue();
    }

    /// <summary>
    /// Verifies that ConfigureSingleInstance registers the MutexBuilder service.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureSingleInstance_RegistersMutexBuilder()
    {
        var hostBuilder = Host.CreateDefaultBuilder();
        hostBuilder.ConfigureSingleInstance(builder => builder.MutexId = "test-mutex-" + Guid.NewGuid().ToString("N"));

        using var host = hostBuilder.Build();
        var mutexBuilder = host.Services.GetService<IMutexBuilder>();
        await Assert.That(mutexBuilder).IsNotNull();
    }

    /// <summary>
    /// Verifies that multiple calls to ConfigureSingleInstance use the same MutexBuilder instance.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureSingleInstance_MultipleCalls_UsesSameMutexBuilder()
    {
        IMutexBuilder? firstBuilder = null;
        IMutexBuilder? secondBuilder = null;

        var hostBuilder = Host.CreateDefaultBuilder();
        hostBuilder.ConfigureSingleInstance(builder =>
        {
            firstBuilder = builder;
            builder.MutexId = "test-mutex-" + Guid.NewGuid().ToString("N");
        });
        hostBuilder.ConfigureSingleInstance(builder =>
        {
            secondBuilder = builder;
        });

        using var host = hostBuilder.Build();
        await Assert.That(firstBuilder).IsNotNull();
        await Assert.That(secondBuilder).IsNotNull();
        await Assert.That(firstBuilder).IsEqualTo(secondBuilder);
    }

    /// <summary>
    /// Verifies that ConfigureSingleInstance allows setting IsGlobal property.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureSingleInstance_CanSetIsGlobal()
    {
        var hostBuilder = Host.CreateDefaultBuilder();
        hostBuilder.ConfigureSingleInstance(builder =>
        {
            builder.MutexId = "test-mutex-" + Guid.NewGuid().ToString("N");
            builder.IsGlobal = true;
        });

        using var host = hostBuilder.Build();
        var mutexBuilder = host.Services.GetRequiredService<IMutexBuilder>();
        await Assert.That(mutexBuilder.IsGlobal).IsTrue();
    }

    /// <summary>
    /// Verifies that ConfigureSingleInstance allows setting WhenNotFirstInstance callback.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureSingleInstance_CanSetWhenNotFirstInstanceCallback()
    {
        var callbackSet = false;
        var hostBuilder = Host.CreateDefaultBuilder();
        hostBuilder.ConfigureSingleInstance(builder =>
        {
            builder.MutexId = "test-mutex-" + Guid.NewGuid().ToString("N");
            builder.WhenNotFirstInstance = (_, _) => callbackSet = true;
        });

        using var host = hostBuilder.Build();
        var mutexBuilder = host.Services.GetRequiredService<IMutexBuilder>();
        await Assert.That(mutexBuilder.WhenNotFirstInstance).IsNotNull();

        // Invoke the callback to verify it was set correctly
        mutexBuilder.WhenNotFirstInstance?.Invoke(null!, null!);
        await Assert.That(callbackSet).IsTrue();
    }
}
