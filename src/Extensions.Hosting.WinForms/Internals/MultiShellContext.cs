// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Threading;
using System.Windows.Forms;

namespace ReactiveMarbles.Extensions.Hosting.WinForms.Internals;

/// <summary>
/// Provides an application context that manages the lifetime of multiple top-level forms, ensuring the application
/// exits when all forms are closed.
/// </summary>
/// <remarks>Use this context to run a Windows Forms application with multiple main forms. The application will
/// remain running until all specified forms are closed, at which point the message loop will exit
/// automatically.</remarks>
internal class MultiShellContext : ApplicationContext
{
    private int _openForms;

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiShellContext"/> class with the specified collection of forms to be managed.
    /// as part of the application context.
    /// </summary>
    /// <remarks>Each form provided is immediately shown and monitored for closure. The context remains active
    /// until all managed forms are closed. This constructor is useful for applications that require multiple main
    /// windows to be open simultaneously.</remarks>
    /// <param name="forms">An array of Form instances to be shown and managed. Each form will be displayed and tracked until it is closed.
    /// Cannot be null or contain null elements.</param>
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
    /// Handles the FormClosed event for a form and performs application shutdown if all tracked forms have been closed.
    /// </summary>
    /// <remarks>This method is intended to be used as an event handler for form closure events in
    /// applications that track multiple startup forms. When the last tracked form is closed, the application thread is
    /// terminated.</remarks>
    /// <param name="s">The source of the event. This is typically the form that was closed.</param>
    /// <param name="args">A FormClosedEventArgs object that contains the event data.</param>
    private void OnFormClosed(object? s, FormClosedEventArgs args)
    {
        // When we have closed the last of the "starting" forms, end the program.
        if (Interlocked.Decrement(ref _openForms) == 0)
        {
            ExitThread();
        }
    }
}
