// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows;

namespace ReactiveMarbles.Extensions.Hosting.Wpf;

/// <summary>Defines a service for initializing Windows Presentation Foundation (WPF) components from the UI thread.</summary>
public interface IWpfService
{
    /// <summary>Initializes the specified application instance, preparing it for use.</summary>
    /// <param name="application">The application instance to initialize. Cannot be null.</param>
    void Initialize(Application application);
}
