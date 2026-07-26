// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows.Forms;
using ReactiveMarbles.Extensions.Hosting.WinForms;

namespace Extensions.Hosting.WinForms.Tests;

/// <summary>Provides a shell form for Windows Forms host integration tests.</summary>
public sealed class TestShell : Form, IWinFormsShell;
