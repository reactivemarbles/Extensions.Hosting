// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Maui.Controls;
using ReactiveMarbles.Extensions.Hosting.Maui;
using ReactiveMarbles.Extensions.Hosting.Maui.Internals;

namespace Extensions.Hosting.Maui.Platform.Tests;

/// <summary>Tests MAUI builder extension configuration without starting a MAUI UI loop.</summary>
public class MauiBuilderExtensionsTests
{
    /// <summary>Verifies page registration preserves the builder and accepts a null builder.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task AddSingletonPage_RegistersPageAndPreservesNullBuilder()
    {
        var builder = new MauiBuilder(static _ => { });

        var result = builder.AddSingletonPage<ContentPage>();
        var nullResult = ((IMauiBuilder)null!).AddSingletonPage<ContentPage>();

        await Assert.That(result).IsSameReferenceAs(builder);
        await Assert.That(builder.PageTypes).Contains(typeof(ContentPage));
        await Assert.That(nullResult).IsNull();
    }

    /// <summary>Verifies application-type configuration assigns the type and invokes its optional customization.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task UseMauiApp_WithApplicationType_ConfiguresBuilder()
    {
        var builder = new MauiBuilder(static _ => { });
        var result = builder.UseMauiApp<TestMauiApplication>(static _ => { });

        await Assert.That(result).IsSameReferenceAs(builder);
        await Assert.That(builder.ApplicationType).IsEqualTo(typeof(TestMauiApplication));
    }

    /// <summary>Verifies application-parameter configuration assigns the application and supports the convenience overload.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task UseMauiApp_WithApplicationParameter_ConfiguresBuilder()
    {
        var builder = new MauiBuilder(static _ => { });

        var result = builder.UseMauiApp((TestMauiApplication)null!);

        await Assert.That(result).IsSameReferenceAs(builder);
        await Assert.That(builder.Application).IsNull();
        await Assert.That(builder.ApplicationType).IsEqualTo(typeof(TestMauiApplication));
    }

    /// <summary>Verifies context configuration stores the provided action.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureContext_StoresAction()
    {
        var builder = new MauiBuilder();
        var configured = false;

        var result = builder.ConfigureContext(_ => configured = true);
        builder.ConfigureContextAction!(new MauiContext());

        await Assert.That(result).IsSameReferenceAs(builder);
        await Assert.That(configured).IsTrue();
    }

    /// <summary>Verifies null builders are rejected by operations that require builder state.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task RequiredBuilderOperations_WithNullBuilder_ThrowArgumentNullException()
    {
        IMauiBuilder? builder = null;
        await Assert.That(() => builder!.UseMauiApp<TestMauiApplication>()).Throws<ArgumentNullException>();
        await Assert.That(() => builder!.UseMauiApp((TestMauiApplication)null!)).Throws<ArgumentNullException>();
        await Assert.That(() => builder!.ConfigureContext(static _ => { })).Throws<ArgumentNullException>();
    }

    /// <summary>Represents an application type used exclusively for builder registration tests.</summary>
    public sealed class TestMauiApplication : Application;
}
