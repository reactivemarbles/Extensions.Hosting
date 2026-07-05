// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveMarbles.Extensions.Hosting.WinForms;

/// <summary>Defines a service that can be initialized from the Windows Forms UI thread.</summary>
public interface IWinFormsService
{
    /// <summary>Initializes the component and prepares it for use.</summary>
    void Initialize();
}
