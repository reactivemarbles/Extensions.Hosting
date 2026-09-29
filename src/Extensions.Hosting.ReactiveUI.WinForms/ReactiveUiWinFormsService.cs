// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveMarbles.Extensions.Hosting.WinForms;
#if REACTIVE_SHIM
using ControlSequencer = ReactiveUI.Primitives.Reactive.Concurrency.ControlSequencer;
using RxSchedulers = ReactiveUI.Reactive.RxSchedulers;
#else
using ControlSequencer = ReactiveUI.Primitives.Concurrency.ControlSequencer;
using RxSchedulers = ReactiveUI.RxSchedulers;
#endif

#if REACTIVE_SHIM
namespace ReactiveMarbles.Extensions.Hosting.Reactive.ReactiveUI;
#else
namespace ReactiveMarbles.Extensions.Hosting.ReactiveUI;
#endif

/// <summary>Initializes ReactiveUI on the hosted Windows Forms UI thread.</summary>
internal sealed class ReactiveUiWinFormsService : IWinFormsService
{
    /// <inheritdoc />
    public void Initialize() => RxSchedulers.MainThreadScheduler = ControlSequencer.Main;
}
