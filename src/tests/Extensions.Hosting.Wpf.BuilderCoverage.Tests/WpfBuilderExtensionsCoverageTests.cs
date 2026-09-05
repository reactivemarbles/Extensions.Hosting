// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactiveMarbles.Extensions.Hosting.Wpf;

namespace Extensions.Hosting.Wpf.BuilderCoverage.Tests;

/// <summary>Verifies WPF builder extension validation branches.</summary>
[NotInParallel]
public sealed class WpfBuilderExtensionsCoverageTests
{
    /// <summary>Verifies null receiver and invalid type validation for direct WPF builder extensions.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task WpfBuilderExtensions_ValidateNullReceiversAndInvalidTypes()
    {
        var builder = new TestWpfBuilder();

        var nullWindowResult = ((IWpfBuilder)null!).UseWindow(typeof(Window));

        await Assert.That(nullWindowResult).IsNull();
        await Assert.That(static () => ((IWpfBuilder)null!).UseApplication(typeof(Application))).Throws<ArgumentNullException>();
        await Assert.That(static () => ((IWpfBuilder)null!).UseCurrentApplication(null!)).Throws<ArgumentNullException>();
        await Assert.That(static () => ((IWpfBuilder)null!).ConfigureContext(static _ => { })).Throws<ArgumentNullException>();
        await Assert.That(() => builder.UseWindow(null!)).Throws<ArgumentNullException>();
        await Assert.That(() => builder.UseWindow(typeof(string))).Throws<ArgumentException>();
        await Assert.That(() => builder.UseApplication(null!)).Throws<ArgumentNullException>();
        await Assert.That(() => builder.UseApplication(typeof(string))).Throws<ArgumentException>();
        await Assert.That(() => builder.UseCurrentApplication(null!)).Throws<ArgumentNullException>();

        var windowResult = builder.UseWindow(typeof(Window));
        var applicationResult = builder.UseApplication(typeof(Application));
        var contextResult = builder.ConfigureContext(static context => context.IsLifetimeLinked = true);

        await Assert.That(windowResult).IsSameReferenceAs(builder);
        await Assert.That(applicationResult).IsSameReferenceAs(builder);
        await Assert.That(contextResult).IsSameReferenceAs(builder);
        await Assert.That(builder.WindowTypes).Contains(typeof(Window));
        await Assert.That(builder.ApplicationType).IsEqualTo(typeof(Application));
        await Assert.That(builder.ConfigureContextAction).IsNotNull();
    }

    /// <summary>Verifies default WPF configuration and lifetime validation for host application builders.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task HostApplicationBuilderExtensions_CoverDefaultAndLifetimeBranches()
    {
        IHostApplicationBuilder? nullBuilder = null;
        var unconfiguredBuilder = Host.CreateApplicationBuilder();
        var builder = Host.CreateApplicationBuilder();

        await Assert.That(() => nullBuilder!.ConfigureWpf()).Throws<ArgumentNullException>();
        await Assert.That(() => nullBuilder!.UseWpfLifetime()).Throws<ArgumentNullException>();
        await Assert.That(() => unconfiguredBuilder.UseWpfLifetime()).Throws<NotSupportedException>();

        var configuredBuilder = builder.ConfigureWpf();
        var lifetimeBuilder = builder.UseWpfLifetime(ShutdownMode.OnExplicitShutdown);
        using var host = builder.Build();
        var context = host.Services.GetRequiredService<IWpfContext>();

        await Assert.That(configuredBuilder).IsSameReferenceAs(builder);
        await Assert.That(lifetimeBuilder).IsSameReferenceAs(builder);
        await Assert.That(context.ShutdownMode).IsEqualTo(ShutdownMode.OnExplicitShutdown);
        await Assert.That(context.IsLifetimeLinked).IsTrue();
    }

    /// <summary>Verifies default WPF configuration and lifetime validation for classic host builders.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task HostBuilderExtensions_CoverDefaultAndLifetimeBranches()
    {
        IHostBuilder? nullBuilder = null;
        var unconfiguredBuilder = new HostBuilder();

        await Assert.That(() => nullBuilder!.ConfigureWpf()).Throws<ArgumentNullException>();
        await Assert.That(() => nullBuilder!.UseWpfLifetime()).Throws<ArgumentNullException>();
        await Assert.That(() => unconfiguredBuilder.UseWpfLifetime().Build()).Throws<NotSupportedException>();

        var builder = new HostBuilder();
        var configuredBuilder = builder.ConfigureWpf();
        var lifetimeBuilder = builder.UseWpfLifetime(ShutdownMode.OnExplicitShutdown);
        using var host = builder.Build();
        var context = host.Services.GetRequiredService<IWpfContext>();

        await Assert.That(configuredBuilder).IsSameReferenceAs(builder);
        await Assert.That(lifetimeBuilder).IsSameReferenceAs(builder);
        await Assert.That(context.ShutdownMode).IsEqualTo(ShutdownMode.OnExplicitShutdown);
        await Assert.That(context.IsLifetimeLinked).IsTrue();
    }

    /// <summary>Provides a test WPF builder implementation for direct extension validation.</summary>
    private sealed class TestWpfBuilder : IWpfBuilder
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
