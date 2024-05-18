// Copyright (c) 2019-2024 ReactiveUI Association Incorporated. All rights reserved.
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
///     This protects your resources or application from running more than once
///     Simplifies the usage of the Mutex class, as described here:
///     https://msdn.microsoft.com/en-us/library/System.Threading.Mutex.aspx.
/// </summary>
public sealed class ResourceMutex : IDisposable
{
    private readonly ILogger _logger;

    private readonly string _mutexId;
    private readonly string _resourceName;
    private Mutex? _applicationMutex;
    private bool _disposedValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceMutex"/> class.
    /// Private constructor.
    /// </summary>
    /// <param name="logger">ILogger.</param>
    /// <param name="mutexId">string with a unique Mutex ID.</param>
    /// <param name="resourceName">optional name for the resource.</param>
    private ResourceMutex(ILogger logger, string mutexId, string? resourceName = null)
    {
        _logger = logger;
        _mutexId = mutexId;
        _resourceName = resourceName ?? mutexId;
    }

    /// <summary>
    ///     Gets or sets a value indicating whether test if the Mutex was created and locked.
    /// </summary>
    public bool IsLocked { get; set; }

    /// <summary>
    ///     Create a ResourceMutex for the specified mutex id and resource-name.
    /// </summary>
    /// <param name="logger">ILogger.</param>
    /// <param name="mutexId">ID of the mutex, preferably a Guid as string.</param>
    /// <param name="resourceName">Name of the resource to lock, e.g your application name, useful for logs.</param>
    /// <param name="global">true to have a global mutex see: https://msdn.microsoft.com/en-us/library/bwe34f1k.aspx.</param>
    /// <returns>A ResourceMutex.</returns>
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
    ///     This tries to get the Mutex, which takes care of having multiple instances running.
    /// </summary>
    /// <returns>true if it worked, false if another instance is already running or something went wrong.</returns>
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
    ///     Dispose the application mutex.
    /// </summary>
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
