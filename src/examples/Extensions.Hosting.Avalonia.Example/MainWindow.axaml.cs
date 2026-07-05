// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Microsoft.Extensions.Logging;
using ReactiveMarbles.Extensions.Hosting.Avalonia;

namespace Extensions.Hosting.Avalonia.Example;

/// <summary>A simple example Avalonia window.</summary>
public partial class MainWindow : Window, IAvaloniaShell
{
    /// <summary>Logs exit button clicks.</summary>
    private static readonly Action<ILogger, Exception?> ExitButtonClicked =
        LoggerMessage.Define(LogLevel.Information, new EventId(1, nameof(ExitButton_Click)), "Exit-Button was clicked!");

    /// <summary>Stores the logger value.</summary>
    private readonly ILogger<MainWindow> _logger;

    /// <summary>Initializes a new instance of the <see cref="MainWindow"/> class and sets up the user interface and event handlers.</summary>
    /// <remarks>This constructor initializes the UI components and attaches the Click event handler for the
    /// Exit button, enabling the application to close when the button is clicked.</remarks>
    /// <param name="logger">The logger used to record events and errors within the MainWindow.</param>
    public MainWindow(ILogger<MainWindow> logger)
    {
        InitializeComponent();
        _logger = logger;

        // Attach event handlers
        var exitButton = this.FindControl<Button>("ExitButton");
        exitButton?.Click += ExitButton_Click;
    }

    /// <summary>Handles exit button clicks.</summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    protected void ExitButton_Click(object? sender, RoutedEventArgs e)
    {
        ExitButtonClicked(_logger, null);

        // Close the application
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime lifetime)
        {
            return;
        }

        lifetime.Shutdown();
    }
}
