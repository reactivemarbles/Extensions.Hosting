// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows.Forms;
using ReactiveMarbles.Extensions.Hosting.WinForms;

namespace Extensions.Hosting.WinForms.Tests;

/// <summary>Provides a shell that fails once during hosted-service cleanup.</summary>
public sealed class ThrowingDisposeTestShell : Form, IWinFormsShell
{
    /// <summary>Indicates whether the next disposal attempt should fail.</summary>
    private bool _throwOnDispose = true;

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing && IsDisposed && _throwOnDispose)
        {
            _throwOnDispose = false;
            throw new InvalidOperationException("The test shell cleanup failed.");
        }

        base.Dispose(disposing);
    }
}
