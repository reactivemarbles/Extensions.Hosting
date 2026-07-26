// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Windows;
using ReactiveMarbles.Extensions.Hosting.Wpf;

namespace Extensions.Hosting.Wpf.MultipleShellCoverage.Tests;

/// <summary>Provides the second WPF shell window for the lifecycle test.</summary>
public sealed class SecondShellWindow : Window, IWpfShell
{
    /// <summary>Gets the registered second shell window type.</summary>
    public static Type RegistrationType => typeof(SecondShellWindow);
}
