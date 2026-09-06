// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using ReactiveMarbles.Extensions.Hosting.AppServices;
using ReactiveMarbles.Extensions.Hosting.AppServices.Internal;
using ReactiveMarbles.Extensions.Hosting.Plugins.Internals;

namespace Extensions.Hosting.Tests;

/// <summary>Contains focused tests for remaining core branch coverage paths.</summary>
public sealed class CoreBranchCoverageTests
{
    /// <summary>The native library file name used by resolver tests.</summary>
    private const string NativeLibraryFileName = "native.dll";

    /// <summary>The native library name used by resolver tests.</summary>
    private const string NativeLibraryName = "native";

    /// <summary>The prefix for process-local named mutexes.</summary>
    private const string LocalMutexPrefix = @"Local\";

    /// <summary>Verifies that the normal plugin load context attempts to load an existing unmanaged dependency path.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task PluginLoadContext_LoadUnmanagedDll_WithInvalidExistingLibrary_ThrowsPlatformLoaderException()
    {
        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var pluginPath = Path.Combine(tempDirectory, $"{nameof(Plugin)}.dll");
            var dependencyPath = Path.Combine(tempDirectory, NativeLibraryFileName);
            await File.WriteAllTextAsync(pluginPath, string.Empty);
            await File.WriteAllTextAsync(dependencyPath, string.Empty);
            var context = new PluginLoadContext(pluginPath, nameof(Plugin));

            var act = () => context.LoadUnmanagedLibrary(NativeLibraryName);
            Exception? exception = null;

            try
            {
                _ = act();
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            await Assert.That(exception).IsNotNull();
            if (OperatingSystem.IsWindows())
            {
                await Assert.That(exception).IsTypeOf<BadImageFormatException>();
                return;
            }

            await Assert.That(exception).IsTypeOf<DllNotFoundException>();
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

    /// <summary>Verifies that a second host instance can stop when no not-first-instance callback is configured.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureSingleInstance_StartAsync_WhenMutexAlreadyLockedWithoutCallback_StopsApplication()
    {
        var mutexId = $"test-mutex-no-callback-{Guid.NewGuid():N}";
        using var primaryMutex = ResourceMutex.Create(NullLogger<ResourceMutex>.Instance, mutexId, "primary", false);
        using var host = Host.CreateDefaultBuilder()
            .ConfigureSingleInstance(builder =>
            {
                builder.MutexId = mutexId;
                builder.IsGlobal = false;
            })
            .Build();

        await host.StartAsync();
        await host.StopAsync();

        await Assert.That(primaryMutex.IsLocked).IsTrue();
        await Assert.That(host.Services.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping.IsCancellationRequested).IsTrue();
    }

    /// <summary>Verifies direct hosted-service stop releases the mutex and remains idempotent.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task MutexLifetimeService_StopAsync_ReleasesResourceMutexAndCanBeCalledAgain()
    {
        using var host = Host.CreateApplicationBuilder().Build();
        var mutexId = $"test-mutex-lifetime-cleanup-{Guid.NewGuid():N}";
        var service = new MutexLifetimeService(
            NullLogger<MutexLifetimeService>.Instance,
            host.Services.GetRequiredService<IHostEnvironment>(),
            host.Services.GetRequiredService<IHostApplicationLifetime>(),
            new TestMutexBuilder { MutexId = mutexId, IsGlobal = false });
        await service.StartAsync(CancellationToken.None);
        await service.StopAsync(CancellationToken.None);

        using var reacquired = ResourceMutex.Create(NullLogger<ResourceMutex>.Instance, mutexId, "verification", false);

        Exception? exception = null;
        try
        {
            await service.StopAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        await Assert.That(reacquired.IsLocked).IsTrue();
        await Assert.That(exception).IsNull();
    }

    /// <summary>Verifies disposing a test-created resource mutex before locking does not require an owner thread.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ResourceMutex_DisposeBeforeLock_DoesNotCreateOrReleaseUnderlyingMutex()
    {
        var createCalled = false;
        var releaseCalled = false;
        var mutex = new ResourceMutex(
            NullLogger.Instance,
            LocalMutexPrefix + $"test-mutex-dispose-before-lock-{Guid.NewGuid():N}",
            resourceName: null,
            mutexId =>
            {
                _ = mutexId;
                createCalled = true;
                return (new Mutex(initiallyOwned: true), true);
            },
            mutex =>
            {
                releaseCalled = true;
                mutex.ReleaseMutex();
            },
            TimeSpan.FromMilliseconds(1));

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
        await Assert.That(createCalled).IsFalse();
        await Assert.That(releaseCalled).IsFalse();
        await Assert.That(mutex.IsLocked).IsFalse();
    }

    /// <summary>Creates a temporary directory for assembly resolver tests.</summary>
    /// <returns>The created temporary directory.</returns>
    private static string CreateTemporaryDirectory()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), "Extensions.Hosting.Tests", Guid.NewGuid().ToString("N"));
        _ = Directory.CreateDirectory(tempDirectory);
        return tempDirectory;
    }
}
