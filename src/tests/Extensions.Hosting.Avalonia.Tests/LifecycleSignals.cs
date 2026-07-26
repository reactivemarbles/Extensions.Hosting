// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Avalonia;

namespace Extensions.Hosting.Avalonia.Tests;

/// <summary>Captures bounded Avalonia application lifecycle events.</summary>
public sealed class LifecycleSignals
{
    /// <summary>Gets the application initialization completion source.</summary>
    public TaskCompletionSource<Application> Initialized { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <summary>Gets or sets a value indicating whether the app builder was configured.</summary>
    public bool AppBuilderConfigured { get; set; }
}
