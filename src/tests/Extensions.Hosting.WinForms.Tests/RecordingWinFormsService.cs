// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveMarbles.Extensions.Hosting.WinForms;

namespace Extensions.Hosting.WinForms.Tests;

/// <summary>Records whether initialization occurred on the Windows Forms UI thread.</summary>
public sealed class RecordingWinFormsService : IWinFormsService
{
    /// <summary>Gets completion for the service initialization operation.</summary>
    public TaskCompletionSource Initialized { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <inheritdoc />
    public void Initialize() => _ = Initialized.TrySetResult();
}
