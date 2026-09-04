// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactiveMarbles.Extensions.Hosting.WinForms;

namespace Extensions.Hosting.WinForms.Tests;

/// <summary>Exercises WinForms host-builder validation and nullable extension contracts.</summary>
public sealed class WinFormsHostBuilderCoverageTests
{
    /// <summary>Verifies that application builders reject a null receiver when configuring a shell.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureWinFormsShell_ApplicationBuilderNull_ThrowsArgumentNullException()
    {
        IHostApplicationBuilder? hostBuilder = null;

        await Assert.That(() => hostBuilder!.ConfigureWinFormsShell(typeof(TestShell))).Throws<ArgumentNullException>();
    }

    /// <summary>Verifies that application builders reject null or incompatible WinForms view types.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureWinForms_ApplicationBuilderInvalidViewType_Throws()
    {
        var hostBuilder = Host.CreateApplicationBuilder();
        Type? viewType = null;

        await Assert.That(() => hostBuilder.ConfigureWinForms(viewType!)).Throws<ArgumentNullException>();
        await Assert.That(() => hostBuilder.ConfigureWinForms(typeof(string))).Throws<ArgumentException>();
    }

    /// <summary>Verifies that application builders reject null or incompatible WinForms shell types.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureWinFormsShell_ApplicationBuilderInvalidShellType_Throws()
    {
        var hostBuilder = Host.CreateApplicationBuilder();
        Type? shellType = null;

        await Assert.That(() => hostBuilder.ConfigureWinFormsShell(shellType!)).Throws<ArgumentNullException>();
        await Assert.That(() => hostBuilder.ConfigureWinFormsShell(typeof(TestForm))).Throws<ArgumentException>();
    }

    /// <summary>Verifies that legacy builders can configure WinForms through the no-argument overload.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureWinForms_HostBuilderNoArgumentOverload_RegistersContext()
    {
        var hostBuilder = Host.CreateDefaultBuilder();

        var configuredBuilder = hostBuilder.ConfigureWinForms();

        using var host = configuredBuilder!.Build();
        await Assert.That(host.Services.GetRequiredService<IWinFormsContext>()).IsNotNull();
    }

    /// <summary>Verifies that direct nullable legacy overloads keep returning null.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task HostBuilderExtensions_NullBuilderDirectOverloads_ReturnNull()
    {
        IHostBuilder? hostBuilder = null;

        await Assert.That(hostBuilder!.ConfigureWinForms((Action<IWinFormsContext>?)null)).IsNull();
        await Assert.That(hostBuilder!.ConfigureWinForms(typeof(TestForm), null)).IsNull();
    }

    /// <summary>Verifies that legacy builders reject null or incompatible WinForms view and shell types.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task HostBuilderExtensions_InvalidTypes_Throw()
    {
        var hostBuilder = Host.CreateDefaultBuilder();
        Type? viewType = null;
        Type? shellType = null;

        await Assert.That(() => hostBuilder.ConfigureWinForms(viewType!)).Throws<ArgumentNullException>();
        await Assert.That(() => hostBuilder.ConfigureWinForms(typeof(string))).Throws<ArgumentException>();
        await Assert.That(() => hostBuilder.ConfigureWinFormsShell(shellType!)).Throws<ArgumentNullException>();
        await Assert.That(() => hostBuilder.ConfigureWinFormsShell(typeof(TestForm))).Throws<ArgumentException>();
    }

    /// <summary>Verifies that legacy builder chaining preserves nullable host-builder implementations.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureWinForms_HostBuilderReturningNullFromConfigureServices_ReturnsNull()
    {
        var hostBuilder = new NullReturningHostBuilder();

        var configuredBuilder = hostBuilder.ConfigureWinForms(typeof(TestForm), null);

        await Assert.That(configuredBuilder).IsNull();
    }

    /// <summary>Implements a legacy host builder that returns null from service configuration.</summary>
    private sealed class NullReturningHostBuilder : IHostBuilder
    {
        /// <inheritdoc />
        public IDictionary<object, object> Properties { get; } = new Dictionary<object, object>();

        /// <inheritdoc />
        public IHost Build() => throw new NotSupportedException();

        /// <inheritdoc />
        public IHostBuilder ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate) => this;

        /// <inheritdoc />
        public IHostBuilder ConfigureContainer<TContainerBuilder>(Action<HostBuilderContext, TContainerBuilder> configureDelegate) => this;

        /// <inheritdoc />
        public IHostBuilder ConfigureHostConfiguration(Action<IConfigurationBuilder> configureDelegate) => this;

        /// <inheritdoc />
        public IHostBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate) => null!;

        /// <inheritdoc />
        public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory)
            where TContainerBuilder : notnull =>
            this;

        /// <inheritdoc />
        public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(Func<HostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factory)
            where TContainerBuilder : notnull =>
            this;
    }
}
