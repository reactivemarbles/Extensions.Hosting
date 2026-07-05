// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows;

namespace ReactiveMarbles.Extensions.Hosting.Wpf;

/// <summary>Defines the contract for a WPF shell, which serves as the main entry point or container for a WPF application's user interface.</summary>
/// <remarks>Implementations of this interface typically provide the primary window or host for WPF application
/// content. The shell may manage navigation, layout, or other application-wide services.</remarks>
public interface IWpfShell : IInputElement;
