// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Avalonia;
using ReactiveMarbles.Extensions.Hosting.Avalonia;

namespace Extensions.Hosting.Avalonia.Tests;

/// <summary>Verifies configuration of <see cref="IAvaloniaBuilder"/> instances.</summary>
public sealed class AvaloniaBuilderExtensionsTests
{
    /// <summary>Verifies that all configuration extensions persist their supplied settings.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigurationExtensions_PersistSettingsAndReturnOriginalBuilder()
    {
        var builder = new TestAvaloniaBuilder();
        var application = new TestApplication();
        var contextConfigured = false;
        var appBuilderConfigured = false;

        var windowResult = builder.UseWindow(typeof(TestShellWindow));
        var applicationResult = builder.UseApplication(typeof(TestApplication));
        var currentApplicationResult = builder.UseCurrentApplication(application);
        var contextResult = builder.ConfigureContext(_ => contextConfigured = true);
        var appBuilderResult = builder.ConfigureAppBuilder(_ => appBuilderConfigured = true);

        await Assert.That(windowResult).IsSameReferenceAs(builder);
        await Assert.That(applicationResult).IsSameReferenceAs(builder);
        await Assert.That(currentApplicationResult).IsSameReferenceAs(builder);
        await Assert.That(contextResult).IsSameReferenceAs(builder);
        await Assert.That(appBuilderResult).IsSameReferenceAs(builder);
        await Assert.That(builder.WindowTypes).Contains(typeof(TestShellWindow));
        await Assert.That(builder.ApplicationType).IsEqualTo(typeof(TestApplication));
        await Assert.That(builder.Application).IsSameReferenceAs(application);
        await Assert.That(builder.ConfigureContextAction).IsNotNull();
        await Assert.That(builder.ConfigureAppBuilderAction).IsNotNull();

        builder.ConfigureContextAction!(new TestAvaloniaContext());
        builder.ConfigureAppBuilderAction!(AppBuilder.Configure<TestApplication>());

        await Assert.That(contextConfigured).IsTrue();
        await Assert.That(appBuilderConfigured).IsTrue();
    }

    /// <summary>Verifies that builder configuration extensions guard null receivers.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigurationExtensions_WithNullBuilder_ThrowArgumentNullException()
    {
        IAvaloniaBuilder? builder = null;

        var useWindow = () => builder!.UseWindow(typeof(TestShellWindow));
        var useApplication = () => builder!.UseApplication(typeof(TestApplication));
        var useCurrentApplication = () => builder!.UseCurrentApplication(new TestApplication());
        var configureContext = () => builder!.ConfigureContext(static _ => { });
        var configureAppBuilder = () => builder!.ConfigureAppBuilder(static _ => { });

        await Assert.That(useWindow).Throws<ArgumentNullException>();
        await Assert.That(useApplication).Throws<ArgumentNullException>();
        await Assert.That(useCurrentApplication).Throws<ArgumentNullException>();
        await Assert.That(configureContext).Throws<ArgumentNullException>();
        await Assert.That(configureAppBuilder).Throws<ArgumentNullException>();
    }

    /// <summary>Verifies invalid type-selection arguments are rejected.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task TypeSelectionExtensions_WithInvalidTypes_ThrowArgumentException()
    {
        var builder = new TestAvaloniaBuilder();

        await Assert.That(() => builder.UseWindow(typeof(string))).Throws<ArgumentException>();
        await Assert.That(() => builder.UseApplication(typeof(string))).Throws<ArgumentException>();
        await Assert.That(() => builder.UseWindow(null!)).Throws<ArgumentNullException>();
        await Assert.That(() => builder.UseApplication(null!)).Throws<ArgumentNullException>();
        await Assert.That(() => builder.UseCurrentApplication(null!)).Throws<ArgumentNullException>();
    }

    /// <summary>A test implementation of the Avalonia builder contract.</summary>
    private sealed class TestAvaloniaBuilder : IAvaloniaBuilder
    {
        /// <inheritdoc />
        public Type? ApplicationType { get; set; }

        /// <inheritdoc />
        public Application? Application { get; set; }

        /// <inheritdoc />
        public IList<Type> WindowTypes { get; } = new List<Type>();

        /// <inheritdoc />
        public Action<IAvaloniaContext>? ConfigureContextAction { get; set; }

        /// <inheritdoc />
        public Action<AppBuilder>? ConfigureAppBuilderAction { get; set; }
    }

    /// <summary>A test implementation of the Avalonia context contract.</summary>
    private sealed class TestAvaloniaContext : IAvaloniaContext
    {
        /// <inheritdoc />
        public global::Avalonia.Controls.ShutdownMode ShutdownMode { get; set; }

        /// <inheritdoc />
        public bool IsLifetimeLinked { get; set; }

        /// <inheritdoc />
        public bool IsRunning { get; set; }

        /// <inheritdoc />
        public Application? AvaloniaApplication { get; set; }

        /// <inheritdoc />
        public global::Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime? ApplicationLifetime { get; set; }

        /// <inheritdoc />
        public global::Avalonia.Threading.Dispatcher Dispatcher => global::Avalonia.Threading.Dispatcher.UIThread;
    }
}
