// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveMarbles.Extensions.Hosting.WinUI.Internals;

/// <summary>Starts a platform UI thread.</summary>
internal interface IUiThreadStarter
{
    /// <summary>Starts the UI thread.</summary>
    void Start();
}
