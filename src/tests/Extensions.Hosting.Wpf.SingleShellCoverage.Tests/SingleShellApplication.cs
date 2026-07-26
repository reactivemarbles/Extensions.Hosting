// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Extensions.Hosting.Wpf.SingleShellCoverage.Tests;

/// <summary>Provides the application used by the single-shell lifecycle test.</summary>
public sealed class SingleShellApplication : Application
{
    /// <summary>Gets a signal for when the application starts.</summary>
    public static TaskCompletionSource<Dispatcher> Started { get; } =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <inheritdoc />
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        _ = Started.TrySetResult(Dispatcher);
    }
}
