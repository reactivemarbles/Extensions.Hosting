// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows;
using ReactiveMarbles.Extensions.Hosting.Wpf;
#if REACTIVE_SHIM
using DispatcherSequencer = ReactiveUI.Primitives.Reactive.Concurrency.DispatcherSequencer;
using RxSchedulers = ReactiveUI.Reactive.RxSchedulers;
#else
using DispatcherSequencer = ReactiveUI.Primitives.Concurrency.DispatcherSequencer;
using RxSchedulers = ReactiveUI.RxSchedulers;
#endif

#if REACTIVE_SHIM
namespace ReactiveMarbles.Extensions.Hosting.Reactive.ReactiveUI;
#else
namespace ReactiveMarbles.Extensions.Hosting.ReactiveUI;
#endif

/// <summary>Binds ReactiveUI scheduling to the dispatcher owned by the hosted WPF application.</summary>
internal sealed class ReactiveUiWpfSchedulerService : IWpfService
{
    /// <inheritdoc />
    public void Initialize(Application application)
    {
        _ = application ?? throw new ArgumentNullException(nameof(application));

        RxSchedulers.MainThreadScheduler = new DispatcherSequencer(application.Dispatcher);
    }
}
