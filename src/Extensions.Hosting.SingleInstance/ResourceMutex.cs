// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Threading;
using Microsoft.Extensions.Logging;

#if NET462
using System.Security.AccessControl;
using System.Security.Principal;
#endif

namespace ReactiveMarbles.Extensions.Hosting.AppServices;

/// <summary>
/// Provides a cross-process mutex for synchronizing access to a named resource, ensuring that only one instance can
/// hold the lock at a time across the system or user session.
/// </summary>
/// <remarks>Use this class to prevent multiple instances of an application or process from accessing the same
/// resource simultaneously. The mutex can be created as either global (system-wide) or local (per user session),
/// depending on the use case. The class is disposable; always call Dispose when the mutex is no longer needed to
/// release the lock and associated resources. Thread safety is not guaranteed for multiple threads using the same
/// ResourceMutex instance.</remarks>
public sealed class ResourceMutex : IDisposable
{
    private readonly ILogger _logger;
    private readonly string _mutexId;
    private readonly string _resourceName;
    private Mutex? _applicationMutex;
    private bool _disposedValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceMutex"/> class with the specified logger, mutex identifier, and optional.
    /// resource name.
    /// </summary>
    /// <param name="logger">The logger instance used to record diagnostic and operational messages for the mutex.</param>
    /// <param name="mutexId">The unique identifier for the mutex. Used to distinguish this mutex from others.</param>
    /// <param name="resourceName">The name of the resource associated with the mutex. If null, the mutex identifier is used as the resource name.</param>
    private ResourceMutex(ILogger logger, string mutexId, string? resourceName = null)
    {
        _logger = logger;
        _mutexId = mutexId;
        _resourceName = resourceName ?? mutexId;
    }

    /// <summary>
    /// Gets a value indicating whether the object is locked.
    /// </summary>
    public bool IsLocked { get; private set; }

    /// <summary>
    /// Creates and acquires a new resource mutex for synchronizing access to a named resource across processes.
    /// </summary>
    /// <remarks>The returned ResourceMutex is locked upon creation. Callers are responsible for releasing the
    /// mutex when it is no longer needed. If a logger is not provided, a default logger is created
    /// internally.</remarks>
    /// <param name="logger">The logger to use for diagnostic messages. If null, a default logger is created.</param>
    /// <param name="mutexId">The unique identifier for the mutex. Cannot be null, empty, or consist only of white-space characters.</param>
    /// <param name="resourceName">An optional name for the resource being protected. May be null if not applicable.</param>
    /// <param name="global">true to create a system-wide (global) mutex; false to create a local mutex scoped to the current user session.</param>
    /// <returns>A ResourceMutex instance that is already acquired and can be used to synchronize access to the specified
    /// resource.</returns>
    /// <exception cref="ArgumentNullException">Thrown if mutexId is null, empty, or consists only of white-space characters.</exception>
    public static ResourceMutex Create(ILogger? logger, string? mutexId, string? resourceName = null, bool global = false)
    {
        if (string.IsNullOrWhiteSpace(mutexId))
        {
            throw new ArgumentNullException(nameof(mutexId));
        }

        logger ??= new LoggerFactory().CreateLogger<ResourceMutex>();
        var applicationMutex = new ResourceMutex(logger, (global ? @"Global\" : @"Local\") + mutexId, resourceName);
        applicationMutex.Lock();
        return applicationMutex;
    }

    /// <summary>
    /// Attempts to acquire an exclusive application-wide mutex to prevent multiple instances from running concurrently.
    /// </summary>
    /// <remarks>This method ensures that only one instance of the resource identified by the mutex ID can
    /// hold the lock at a time. If the mutex is already held by another process or user, the method waits for a short
    /// period before failing to acquire the lock. If the mutex cannot be acquired due to permission issues or other
    /// errors, the method returns false. The lock should be released appropriately when no longer needed to avoid
    /// resource contention.</remarks>
    /// <returns>true if the mutex was successfully acquired and the lock is held by the current instance; otherwise, false.</returns>
    public bool Lock()
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("{resourceName} is trying to get Mutex {mutexId}", _resourceName, _mutexId);
        }

        IsLocked = true;

        // check whether there's an local instance running already, but use local so this works in a multi-user environment
        try
        {
#if NET462
            // Added Mutex Security, hopefully this prevents the UnauthorizedAccessException more gracefully
            var sid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            var mutexsecurity = new MutexSecurity();
            mutexsecurity.AddAccessRule(new MutexAccessRule(sid, MutexRights.FullControl, AccessControlType.Allow));
            mutexsecurity.AddAccessRule(new MutexAccessRule(sid, MutexRights.ChangePermissions, AccessControlType.Deny));
            mutexsecurity.AddAccessRule(new MutexAccessRule(sid, MutexRights.Delete, AccessControlType.Deny));

            // 1) Create Mutex
            _applicationMutex = new Mutex(true, _mutexId, out var createdNew, mutexsecurity);
#else
            // 1) Create Mutex
            _applicationMutex = new Mutex(true, _mutexId, out var createdNew);
#endif

            // 2) if the mutex wasn't created new get the right to it, this returns false if it's already locked
            if (!createdNew)
            {
                IsLocked = _applicationMutex.WaitOne(2000, false);
                if (!IsLocked)
                {
                    _logger.LogWarning("Mutex {mutexId} is already in use and couldn't be locked for the caller {resourceName}", _mutexId, _resourceName);

                    // Clean up
                    _applicationMutex.Dispose();
                    _applicationMutex = null;
                }
                else
                {
                    _logger.LogInformation("{resourceName} has claimed the mutex {mutexId}", _resourceName, _mutexId);
                }
            }
            else
            {
                _logger.LogInformation("{resourceName} has created & claimed the mutex {mutexId}", _resourceName, _mutexId);
            }
        }
        catch (AbandonedMutexException e)
        {
            // Another instance didn't cleanup correctly!
            // we can ignore the exception, it happened on the "WaitOne" but still the mutex belongs to us
            _logger.LogWarning(e, "{resourceName} didn't cleanup correctly, but we got the mutex {mutexId}.", _resourceName, _mutexId);
        }
        catch (UnauthorizedAccessException e)
        {
            _logger.LogError(
                e,
                "{resourceName} is most likely already running for a different user in the same session, can't create/get mutex {mutexId} due to error.",
                _resourceName,
                _mutexId);
            IsLocked = false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Problem obtaining the Mutex {mutexId} for {resourceName}, assuming it was already taken!", _resourceName, _mutexId);
            IsLocked = false;
        }

        return IsLocked;
    }

    /// <summary>
    /// Releases all resources used by the current instance and releases the associated application mutex if it is held.
    /// </summary>
    /// <remarks>Call this method when you are finished using the object to ensure that the mutex is properly
    /// released and resources are freed. After calling Dispose, the object should not be used. This method is safe to
    /// call multiple times; subsequent calls have no effect.</remarks>
    public void Dispose()
    {
        if (_disposedValue)
        {
            return;
        }

        _disposedValue = true;
        if (_applicationMutex == null)
        {
            return;
        }

        try
        {
            if (IsLocked)
            {
                _applicationMutex.ReleaseMutex();
                IsLocked = false;
                _logger.LogInformation("Released Mutex {mutexId} for {resourceName}", _mutexId, _resourceName);
            }

            _applicationMutex.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error releasing Mutex {mutexId} for {resourceName}", _mutexId, _resourceName);
        }

        _applicationMutex = null!;
    }
}
