// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Windows.Forms;

namespace CP.Extensions.Hosting.WinForms.Internals;

/// <summary>
/// This provides context for running multiple forms which are defined as IWinFormsShell.
/// </summary>
internal class MultiShellContext : ApplicationContext
{
    private int _openForms;

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiShellContext" /> class.
    /// Constructor which specified the.
    /// </summary>
    /// <param name="forms">The forms.</param>
    public MultiShellContext(params Form[] forms)
    {
        _openForms = forms.Length;
        foreach (var form in forms)
        {
            form.FormClosed += OnFormClosed;
            form.Show();
        }
    }

    /// <summary>
    /// Make sure the application stops if all shells are closed.
    /// </summary>
    /// <param name="s">The s.</param>
    /// <param name="args">The <see cref="FormClosedEventArgs"/> instance containing the event data.</param>
    private void OnFormClosed(object? s, FormClosedEventArgs args)
    {
        // When we have closed the last of the "starting" forms, end the program.
        if (Interlocked.Decrement(ref _openForms) == 0)
        {
            ExitThread();
        }
    }
}
