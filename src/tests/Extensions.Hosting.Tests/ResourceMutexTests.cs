// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using ReactiveMarbles.Extensions.Hosting.AppServices;
using TUnit;

namespace Extensions.Hosting.Tests;

/// <summary>
/// Contains unit tests for the ResourceMutex class to verify its behavior and exception handling.
/// </summary>
/// <remarks>These tests ensure that ResourceMutex correctly throws exceptions for invalid input and that its
/// locking mechanism functions as expected. The tests are intended to validate the public API and usage scenarios of
/// ResourceMutex.</remarks>
public class ResourceMutexTests
{
    /// <summary>
    /// Verifies that calling ResourceMutex.Create with a null mutex ID throws an ArgumentNullException.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task Create_WithNullMutexId_ThrowsArgumentNullException()
    {
        static ResourceMutex Act() => ResourceMutex.Create(null, null);
        await Assert.That(Act).Throws<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that calling ResourceMutex.Create with an empty mutex ID throws an ArgumentNullException.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task Create_WithEmptyMutexId_ThrowsArgumentNullException()
    {
        static ResourceMutex Act() => ResourceMutex.Create(NullLogger.Instance, string.Empty);
        await Assert.That(Act).Throws<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that calling ResourceMutex.Create with a whitespace mutex ID throws an ArgumentNullException.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task Create_WithWhitespaceMutexId_ThrowsArgumentNullException()
    {
        static ResourceMutex Act() => ResourceMutex.Create(NullLogger.Instance, "   ");
        await Assert.That(Act).Throws<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that a ResourceMutex instance is locked upon creation and is properly disposed.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task Create_ValidMutexId_IsLocked()
    {
        var logger = NullLogger.Instance;
        var id = "test-mutex-" + Guid.NewGuid().ToString("N");

        using var mutex = ResourceMutex.Create(logger, id);
        await Assert.That(mutex.IsLocked).IsTrue();
    }

    /// <summary>
    /// Verifies that ResourceMutex can be created without a logger (null logger).
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task Create_WithNullLogger_CreatesDefaultLogger()
    {
        var id = "test-mutex-nulllogger-" + Guid.NewGuid().ToString("N");

        using var mutex = ResourceMutex.Create(null, id);
        await Assert.That(mutex).IsNotNull();
        await Assert.That(mutex.IsLocked).IsTrue();
    }

    /// <summary>
    /// Verifies that ResourceMutex can be created with a custom resource name.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task Create_WithResourceName_Succeeds()
    {
        var logger = NullLogger.Instance;
        var id = "test-mutex-resource-" + Guid.NewGuid().ToString("N");
        const string resourceName = "TestResource";

        using var mutex = ResourceMutex.Create(logger, id, resourceName);
        await Assert.That(mutex.IsLocked).IsTrue();
    }

    /// <summary>
    /// Verifies that ResourceMutex can be created as a local mutex.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task Create_LocalMutex_Succeeds()
    {
        var logger = NullLogger.Instance;
        var id = "test-mutex-local-" + Guid.NewGuid().ToString("N");

        using var mutex = ResourceMutex.Create(logger, id, global: false);
        await Assert.That(mutex.IsLocked).IsTrue();
    }

    /// <summary>
    /// Verifies that ResourceMutex can be created as a global mutex.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task Create_GlobalMutex_Succeeds()
    {
        var logger = NullLogger.Instance;
        var id = "test-mutex-global-" + Guid.NewGuid().ToString("N");

        using var mutex = ResourceMutex.Create(logger, id, global: true);
        await Assert.That(mutex.IsLocked).IsTrue();
    }

    /// <summary>
    /// Verifies that disposing a ResourceMutex does not throw.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task Dispose_DoesNotThrow()
    {
        var logger = NullLogger.Instance;
        var id = "test-mutex-dispose-" + Guid.NewGuid().ToString("N");

        var mutex = ResourceMutex.Create(logger, id);
        Exception? exception = null;
        try
        {
            mutex.Dispose();
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        await Assert.That(exception).IsNull();
    }

    /// <summary>
    /// Verifies that disposing a ResourceMutex multiple times does not throw.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task Dispose_MultipleTimes_DoesNotThrow()
    {
        var logger = NullLogger.Instance;
        var id = "test-mutex-double-dispose-" + Guid.NewGuid().ToString("N");

        var mutex = ResourceMutex.Create(logger, id);
        mutex.Dispose();

        Exception? exception = null;
        try
        {
            mutex.Dispose();
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        await Assert.That(exception).IsNull();
    }
}
