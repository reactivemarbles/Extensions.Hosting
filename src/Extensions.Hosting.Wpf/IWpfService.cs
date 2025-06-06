﻿// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows;

namespace ReactiveMarbles.Extensions.Hosting.Wpf;

/// <summary>
/// This defines a service which is called before the message loop is started.
/// </summary>
public interface IWpfService
{
    /// <summary>
    /// Do whatever you need to do to initialize WPF, this is called from the UI thread.
    /// </summary>
    /// <param name="application">Application.</param>
    void Initialize(Application application);
}
