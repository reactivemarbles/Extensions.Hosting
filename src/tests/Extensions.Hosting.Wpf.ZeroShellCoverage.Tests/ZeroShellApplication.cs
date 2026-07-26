// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using System.Windows;

namespace Extensions.Hosting.Wpf.ZeroShellCoverage.Tests;

/// <summary>Provides the WPF application used by the zero-shell lifecycle test.</summary>
public sealed class ZeroShellApplication : Application
{
    /// <summary>Gets a signal for when the application starts.</summary>
    public static TaskCompletionSource Started { get; } =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <inheritdoc />
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        _ = Started.TrySetResult();
    }
}
