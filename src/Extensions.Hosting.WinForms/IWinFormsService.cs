// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveMarbles.Extensions.Hosting.WinForms;

/// <summary>
/// Defines a service that can be initialized from the Windows Forms UI thread.
/// </summary>
public interface IWinFormsService
{
    /// <summary>
    /// Initializes the component and prepares it for use.
    /// </summary>
    void Initialize();
}
