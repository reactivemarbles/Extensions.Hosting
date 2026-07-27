// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows.Forms;
using ReactiveMarbles.Extensions.Hosting.WinForms;
using FormsTimer = System.Windows.Forms.Timer;

namespace Extensions.Hosting.WinForms.Tests;

/// <summary>Provides a shell that closes after the Windows Forms message loop starts.</summary>
public sealed class AutoClosingTestShell : Form, IWinFormsShell
{
    /// <summary>Stores the delay before the test shell closes.</summary>
    private const int CloseDelayMilliseconds = 250;

    /// <summary>Stores the timer that schedules the shell shutdown.</summary>
    private readonly FormsTimer _closeTimer = new() { Interval = CloseDelayMilliseconds };

    /// <summary>Initializes a new instance of the <see cref="AutoClosingTestShell"/> class.</summary>
    public AutoClosingTestShell() => _closeTimer.Tick += OnCloseTimerTick;

    /// <inheritdoc />
    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        _closeTimer.Start();
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _closeTimer.Stop();
            _closeTimer.Dispose();
        }

        base.Dispose(disposing);
    }

    /// <summary>Stops the delay timer and closes the shell.</summary>
    /// <param name="sender">The timer that raised the event.</param>
    /// <param name="e">The event data.</param>
    private void OnCloseTimerTick(object? sender, EventArgs e)
    {
        _ = sender;
        _ = e;
        _closeTimer.Stop();
        Close();
    }
}
