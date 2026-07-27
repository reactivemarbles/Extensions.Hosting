// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows;
using System.Windows.Threading;

namespace Extensions.Hosting.ReactiveUI.Wpf.Tests;

/// <summary>Signals when the hosted WPF application has entered its startup lifecycle.</summary>
public sealed class TestApplication : Application
{
    /// <summary>Gets the dispatcher reported by the hosted application at startup.</summary>
    public static TaskCompletionSource<Dispatcher> Started { get; } =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <inheritdoc />
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        _ = Started.TrySetResult(Dispatcher);
    }
}
