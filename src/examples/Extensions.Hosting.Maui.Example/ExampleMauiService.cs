// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;
using ReactiveMarbles.Extensions.Hosting.Maui;
using Application = Microsoft.Maui.Controls.Application;

namespace Extensions.Hosting.Maui.Example;

/// <summary>Example MAUI service.</summary>
/// <param name="logger">The logger used to record service initialization.</param>
public class ExampleMauiService(ILogger<ExampleMauiService> logger) : IMauiService
{
    /// <summary>Logs that the MAUI application was initialized.</summary>
    private static readonly Action<ILogger, Exception?> ApplicationInitialized =
        LoggerMessage.Define(LogLevel.Information, new EventId(1, nameof(ApplicationInitialized)), "MAUI application initialized via IMauiService");

    /// <summary>Initializes the specified application.</summary>
    /// <param name="application">The application.</param>
    public void Initialize(Application application)
    {
        ApplicationInitialized(logger, null);
    }
}
