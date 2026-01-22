// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using TUnit;

namespace Extensions.Hosting.Tests;

/// <summary>
/// Contains unit tests for the IMutexBuilder interface and its implementation.
/// </summary>
public class MutexBuilderTests
{
    /// <summary>
    /// Verifies that TestMutexBuilder has null MutexId by default.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task MutexBuilder_MutexId_DefaultsToNull()
    {
        var builder = new TestMutexBuilder();
        await Assert.That(builder.MutexId).IsNull();
    }

    /// <summary>
    /// Verifies that MutexId can be set and retrieved.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task MutexBuilder_MutexId_CanBeSetAndRetrieved()
    {
        var builder = new TestMutexBuilder();
        builder.MutexId = "test-mutex-id";
        await Assert.That(builder.MutexId).IsEqualTo("test-mutex-id");
    }

    /// <summary>
    /// Verifies that IsGlobal defaults to false.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task MutexBuilder_IsGlobal_DefaultsToFalse()
    {
        var builder = new TestMutexBuilder();
        await Assert.That(builder.IsGlobal).IsFalse();
    }

    /// <summary>
    /// Verifies that IsGlobal can be set and retrieved.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task MutexBuilder_IsGlobal_CanBeSetAndRetrieved()
    {
        var builder = new TestMutexBuilder();
        builder.IsGlobal = true;
        await Assert.That(builder.IsGlobal).IsTrue();
    }

    /// <summary>
    /// Verifies that WhenNotFirstInstance defaults to null.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task MutexBuilder_WhenNotFirstInstance_DefaultsToNull()
    {
        var builder = new TestMutexBuilder();
        await Assert.That(builder.WhenNotFirstInstance).IsNull();
    }

    /// <summary>
    /// Verifies that WhenNotFirstInstance can be set and invoked.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task MutexBuilder_WhenNotFirstInstance_CanBeSetAndInvoked()
    {
        var builder = new TestMutexBuilder();
        var wasInvoked = false;

        builder.WhenNotFirstInstance = (_, _) => wasInvoked = true;
        builder.WhenNotFirstInstance?.Invoke(null!, null!);

        await Assert.That(wasInvoked).IsTrue();
    }

    /// <summary>
    /// Verifies that all properties can be configured together.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task MutexBuilder_AllProperties_CanBeConfiguredTogether()
    {
        var builder = new TestMutexBuilder
        {
            MutexId = "my-app-mutex",
            IsGlobal = true,
            WhenNotFirstInstance = (env, logger) => { }
        };

        await Assert.That(builder.MutexId).IsEqualTo("my-app-mutex");
        await Assert.That(builder.IsGlobal).IsTrue();
        await Assert.That(builder.WhenNotFirstInstance).IsNotNull();
    }
}
