// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Avalonia.Controls;
using ReactiveMarbles.Extensions.Hosting.Avalonia;

namespace Extensions.Hosting.Avalonia.Tests;

/// <summary>A shell window used to verify shell registration.</summary>
public sealed class TestShellWindow : Window, IAvaloniaShell;
