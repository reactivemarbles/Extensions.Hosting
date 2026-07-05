// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Threading;
using Microsoft.Extensions.Logging;

#if NET462
using System.Security.AccessControl;
using System.Security.Principal;
#endif

namespace ReactiveMarbles.Extensions.Hosting.AppServices;

/// <summary>Provides a cross-process mutex for synchronizing access to a named resource, ensuring that only one instance can hold the lock at a time across the system or user session.</summary>
/// <remarks>Use this class to prevent multiple instances of an application or process from accessing the same
/// resource simultaneously. The mutex can be created as either global (system-wide) or local (per user session),
/// depending on the use case. The class is disposable; always call Dispose when the mutex is no longer needed to
/// release the lock and associated resources. Thread safety is not guaranteed for multiple threads using the same
/// ResourceMutex instance.</remarks>
public sealed class ResourceMutex : IDisposable
{
    /// <summary>Logs a mutex acquisition attempt.</summary>
    private static readonly Action<ILogger, string, string, Exception?> _tryingToGetMutex =
        LoggerMessage.Define<string, string>(LogLevel.Debug, new EventId(1, nameof(_tryingToGetMutex)), "{ResourceName} is trying to get Mutex {MutexId}");

    /// <summary>Logs a mutex owner release timeout.</summary>
    private static readonly Action<ILogger, string, string, Exception?> _mutexOwnerReleaseTimedOut =
        LoggerMessage.Define<string, string>(LogLevel.Warning, new EventId(2, nameof(_mutexOwnerReleaseTimedOut)), "Timed out waiting for mutex owner thread to release Mutex {MutexId} for {ResourceName}");

    /// <summary>Logs that a mutex is already in use.</summary>
    private static readonly Action<ILogger, string, string, Exception?> _mutexAlreadyInUse =
        LoggerMessage.Define<string, string>(LogLevel.Warning, new EventId(3, nameof(_mutexAlreadyInUse)), "Mutex {MutexId} is already in use and couldn't be locked for the caller {ResourceName}");

    /// <summary>Logs that an existing mutex was claimed.</summary>
    private static readonly Action<ILogger, string, string, Exception?> _mutexClaimed =
        LoggerMessage.Define<string, string>(LogLevel.Information, new EventId(4, nameof(_mutexClaimed)), "{ResourceName} has claimed the mutex {MutexId}");

    /// <summary>Logs that a new mutex was created and claimed.</summary>
    private static readonly Action<ILogger, string, string, Exception?> _mutexCreatedAndClaimed =
        LoggerMessage.Define<string, string>(LogLevel.Information, new EventId(5, nameof(_mutexCreatedAndClaimed)), "{ResourceName} has created & claimed the mutex {MutexId}");

    /// <summary>Logs that an abandoned mutex was recovered.</summary>
    private static readonly Action<ILogger, string, string, Exception?> _abandonedMutexRecovered =
        LoggerMessage.Define<string, string>(LogLevel.Warning, new EventId(6, nameof(_abandonedMutexRecovered)), "{ResourceName} didn't cleanup correctly, but we got the mutex {MutexId}.");

    /// <summary>Logs that mutex access was unauthorized.</summary>
    private static readonly Action<ILogger, string, string, Exception?> _mutexUnauthorized =
        LoggerMessage.Define<string, string>(LogLevel.Error, new EventId(7, nameof(_mutexUnauthorized)), "{ResourceName} is most likely already running for a different user in the same session, can't create/get mutex {MutexId} due to error.");

    /// <summary>Logs that mutex acquisition failed.</summary>
    private static readonly Action<ILogger, string, string, Exception?> _mutexAcquisitionFailed =
        LoggerMessage.Define<string, string>(LogLevel.Error, new EventId(8, nameof(_mutexAcquisitionFailed)), "Problem obtaining the Mutex {MutexId} for {ResourceName}, assuming it was already taken!");

    /// <summary>Logs that a mutex was released.</summary>
    private static readonly Action<ILogger, string, string, Exception?> _mutexReleased =
        LoggerMessage.Define<string, string>(LogLevel.Information, new EventId(9, nameof(_mutexReleased)), "Released Mutex {MutexId} for {ResourceName}");

    /// <summary>Logs that mutex release failed.</summary>
    private static readonly Action<ILogger, string, string, Exception?> _mutexReleaseFailed =
        LoggerMessage.Define<string, string>(LogLevel.Error, new EventId(10, nameof(_mutexReleaseFailed)), "Error releasing Mutex {MutexId} for {ResourceName}");

    /// <summary>Stores the logger value.</summary>
    private readonly ILogger _logger;

    /// <summary>Stores the mutex id value.</summary>
    private readonly string _mutexId;

    /// <summary>Stores the resource name value.</summary>
    private readonly string _resourceName;

    /// <summary>Stores the mutex factory value.</summary>
    private readonly Func<string, (Mutex Mutex, bool CreatedNew)> _createMutex;

    /// <summary>Stores the mutex release action value.</summary>
    private readonly Action<Mutex> _releaseMutex;

    /// <summary>Stores the optional before release action value.</summary>
    private readonly Action? _beforeReleaseMutex;

    /// <summary>Stores the owner thread join timeout value.</summary>
    private readonly TimeSpan _ownerThreadJoinTimeout;

    /// <summary>Stores the lock acquired signal value.</summary>
    private readonly ManualResetEventSlim _lockAcquiredSignal = new(initialState: false);

    /// <summary>Stores the release signal value.</summary>
    private readonly ManualResetEventSlim _releaseSignal = new(initialState: false);

    /// <summary>Stores the disposed value.</summary>
    private bool _disposedValue;

    /// <summary>Stores the owner thread value.</summary>
    private Thread? _ownerThread;

    /// <summary>Initializes a new instance of the <see cref="ResourceMutex"/> class with testable mutex operations.</summary>
    /// <param name="logger">The logger instance used to record diagnostic and operational messages for the mutex.</param>
    /// <param name="mutexId">The unique identifier for the mutex. Used to distinguish this mutex from others.</param>
    /// <param name="resourceName">The name of the resource associated with the mutex. If null, the mutex identifier is used as the resource name.</param>
    /// <param name="createMutex">The operation used to create the underlying mutex.</param>
    /// <param name="releaseMutex">The operation used to release the underlying mutex.</param>
    /// <param name="ownerThreadJoinTimeout">The amount of time to wait for the owner thread to exit during disposal.</param>
    /// <param name="beforeReleaseMutex">An optional operation to run on the owner thread before releasing the mutex.</param>
    internal ResourceMutex(
        ILogger logger,
        string mutexId,
        string? resourceName,
        Func<string, (Mutex Mutex, bool CreatedNew)> createMutex,
        Action<Mutex> releaseMutex,
        TimeSpan ownerThreadJoinTimeout,
        Action? beforeReleaseMutex = null)
    {
        _logger = logger;
        _mutexId = mutexId;
        _resourceName = resourceName ?? mutexId;
        _createMutex = createMutex;
        _releaseMutex = releaseMutex;
        _ownerThreadJoinTimeout = ownerThreadJoinTimeout;
        _beforeReleaseMutex = beforeReleaseMutex;
    }

    /// <summary>Initializes a new instance of the <see cref="ResourceMutex"/> class with the specified logger, mutex identifier, and optional. resource name.</summary>
    /// <param name="logger">The logger instance used to record diagnostic and operational messages for the mutex.</param>
    /// <param name="mutexId">The unique identifier for the mutex. Used to distinguish this mutex from others.</param>
    /// <param name="resourceName">The name of the resource associated with the mutex. If null, the mutex identifier is used as the resource name.</param>
    private ResourceMutex(ILogger logger, string mutexId, string? resourceName = null)
        : this(logger, mutexId, resourceName, CreateMutex, static mutex => mutex.ReleaseMutex(), TimeSpan.FromSeconds(5), null)
    {
    }

    /// <summary>Gets a value indicating whether the object is locked.</summary>
    public bool IsLocked { get; private set; }

    /// <summary>Creates and acquires a new resource mutex for synchronizing access to a named resource across processes.</summary>
    /// <remarks>The returned ResourceMutex is locked upon creation. Callers are responsible for releasing the
    /// mutex when it is no longer needed. If a logger is not provided, a default logger is created
    /// internally.</remarks>
    /// <param name="logger">The logger to use for diagnostic messages. If null, a default logger is created.</param>
    /// <param name="mutexId">The unique identifier for the mutex. Cannot be null, empty, or consist only of white-space characters.</param>
    /// <param name="resourceName">An optional name for the resource being protected. May be null if not applicable.</param>
    /// <param name="global">true to create a system-wide (global) mutex; false to create a local mutex scoped to the current user session.</param>
    /// <returns>A ResourceMutex instance that is already acquired and can be used to synchronize access to the specified
    /// resource.</returns>
    /// <exception cref="ArgumentException">Thrown if mutexId is null, empty, or consists only of white-space characters.</exception>
    public static ResourceMutex Create(ILogger? logger, string? mutexId, string? resourceName = null, bool global = false)
    {
        var resourceMutexId = !string.IsNullOrWhiteSpace(mutexId)
            ? mutexId
            : throw new ArgumentException("The mutex identifier cannot be null, empty, or consist only of white-space characters.", nameof(mutexId));

        logger ??= new LoggerFactory().CreateLogger<ResourceMutex>();
        var applicationMutex = new ResourceMutex(logger, (global ? @"Global\" : @"Local\") + resourceMutexId, resourceName);
        _ = applicationMutex.Lock();
        return applicationMutex;
    }

    /// <summary>Attempts to acquire an exclusive application-wide mutex to prevent multiple instances from running concurrently.</summary>
    /// <remarks>This method spawns a dedicated background thread to own the mutex for its entire lifetime.
    /// This ensures that <see cref="Dispose"/> can safely release the mutex from any calling thread, because
    /// <see cref="Mutex.ReleaseMutex"/> is always invoked on the thread that originally
    /// acquired it. If the mutex is already held by another process or user, the method returns false. The lock
    /// should be released by calling <see cref="Dispose"/> when no longer needed to avoid resource
    /// contention.</remarks>
    /// <returns>true if the mutex was successfully acquired and the lock is held by the current instance; otherwise, false.</returns>
    public bool Lock()
    {
        _tryingToGetMutex(_logger, _resourceName, _mutexId, null);

        _ownerThread = new(AcquireAndHoldMutex)
        {
            IsBackground = true,
            Name = $"ResourceMutex-{_resourceName}",
        };
        _ownerThread.Start();

        // Wait until the dedicated thread has finished the acquire attempt.
        _lockAcquiredSignal.Wait();

        return IsLocked;
    }

    /// <summary>Releases all resources used by the current instance and releases the associated application mutex if it is held.</summary>
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

        // Signal the owning thread to release the mutex, then wait for it to finish.
        _releaseSignal.Set();
        if (_ownerThread?.Join(_ownerThreadJoinTimeout) == false)
        {
            _mutexOwnerReleaseTimedOut(_logger, _mutexId, _resourceName, null);
        }

        _releaseSignal.Dispose();
        _lockAcquiredSignal.Dispose();
    }

    /// <summary>Creates the underlying named mutex.</summary>
    /// <param name="mutexId">The fully qualified mutex identifier.</param>
    /// <returns>The created mutex and a value indicating whether it was newly created.</returns>
    private static (Mutex Mutex, bool CreatedNew) CreateMutex(string mutexId)
    {
#if NET462
        // Added Mutex Security, hopefully this prevents the UnauthorizedAccessException more gracefully
        var sid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
        var mutexSecurity = new MutexSecurity();
        mutexSecurity.AddAccessRule(new(sid, MutexRights.FullControl, AccessControlType.Allow));
        mutexSecurity.AddAccessRule(new(sid, MutexRights.ChangePermissions, AccessControlType.Deny));
        mutexSecurity.AddAccessRule(new(sid, MutexRights.Delete, AccessControlType.Deny));

        // 1) Create Mutex
        return (new Mutex(true, mutexId, out var createdNew, mutexSecurity), createdNew);
#else
        // 1) Create Mutex
        return (new Mutex(true, mutexId, out var createdNew), createdNew);
#endif
    }

    /// <summary>
    /// Runs on the dedicated owner thread: acquires the mutex, signals the caller, waits for the dispose
    /// signal, then releases the mutex - all on the same thread to satisfy <see cref="Mutex"/>
    /// thread-affinity requirements.
    /// </summary>
    private void AcquireAndHoldMutex()
    {
        IsLocked = true;
        var mutexAcquired = false;
        Mutex? applicationMutex = null;

        // check whether there's an local instance running already, but use local so this works in a multi-user environment
        try
        {
            var mutex = _createMutex(_mutexId);
            applicationMutex = mutex.Mutex;
            var createdNew = mutex.CreatedNew;

            // 2) if the mutex wasn't created new get the right to it, this returns false if it's already locked
            if (!createdNew)
            {
                IsLocked = applicationMutex.WaitOne(2000, false);
                if (!IsLocked)
                {
                    _mutexAlreadyInUse(_logger, _mutexId, _resourceName, null);

                    // Clean up
                    applicationMutex.Dispose();
                    applicationMutex = null;
                }
                else
                {
                    _mutexClaimed(_logger, _resourceName, _mutexId, null);
                    mutexAcquired = true;
                }
            }
            else
            {
                _mutexCreatedAndClaimed(_logger, _resourceName, _mutexId, null);
                mutexAcquired = true;
            }
        }
        catch (AbandonedMutexException e)
        {
            // Another instance didn't cleanup correctly!
            // we can ignore the exception, it happened on the "WaitOne" but still the mutex belongs to us
            _abandonedMutexRecovered(_logger, _resourceName, _mutexId, e);
            mutexAcquired = true;
        }
        catch (UnauthorizedAccessException e)
        {
            _mutexUnauthorized(_logger, _resourceName, _mutexId, e);
            IsLocked = false;
        }
        catch (Exception ex)
        {
            _mutexAcquisitionFailed(_logger, _mutexId, _resourceName, ex);
            IsLocked = false;
        }
        finally
        {
            // Always unblock the caller of Lock() once the acquire attempt is complete.
            _lockAcquiredSignal.Set();
        }

        if (!mutexAcquired)
        {
            return;
        }

        // Hold until Dispose() signals that we should release.
        _releaseSignal.Wait();
        _beforeReleaseMutex?.Invoke();

        // Release on this thread - the same thread that acquired the mutex - to satisfy Mutex thread-affinity.
        try
        {
            if (IsLocked && applicationMutex is not null)
            {
                _releaseMutex(applicationMutex);
                IsLocked = false;
                _mutexReleased(_logger, _mutexId, _resourceName, null);
            }
        }
        catch (Exception ex)
        {
            _mutexReleaseFailed(_logger, _mutexId, _resourceName, ex);
        }
        finally
        {
            applicationMutex?.Dispose();
        }
    }
}
