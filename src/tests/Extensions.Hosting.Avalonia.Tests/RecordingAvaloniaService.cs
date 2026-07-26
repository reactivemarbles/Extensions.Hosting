// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Avalonia;
using ReactiveMarbles.Extensions.Hosting.Avalonia;

namespace Extensions.Hosting.Avalonia.Tests;

/// <summary>Signals application service initialization without opening a window.</summary>
/// <param name="signals">The signal store updated on application initialization.</param>
public sealed class RecordingAvaloniaService(LifecycleSignals signals) : IAvaloniaService
{
    /// <inheritdoc />
    public void Initialize(Application application) => _ = signals.Initialized.TrySetResult(application);
}
