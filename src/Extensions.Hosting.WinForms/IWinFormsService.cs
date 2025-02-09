// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveMarbles.Extensions.Hosting.WinForms;

/// <summary>
/// This defines a service which is called before the message loop is started.
/// </summary>
public interface IWinFormsService
{
    /// <summary>
    /// Do whatever you need to do to initialize, this is called from the UI thread.
    /// </summary>
    void Initialize();
}
