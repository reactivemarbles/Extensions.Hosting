// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReactiveMarbles.Extensions.Hosting.Maui;

namespace Extensions.Hosting.Maui.Example;

/// <summary>
/// MauiProgram.
/// </summary>
public static class MauiProgram
{
    /// <summary>
    /// Initializes the <see cref="MauiProgram"/> class.
    /// </summary>
    static MauiProgram()
    {
        // Create the host
        var builder = Host.CreateApplicationBuilder();
        builder.Services.AddSingleton<IMauiService, ExampleMauiService>();
        builder
            .ConfigureMaui(maui =>
            {
                maui.UseMauiApp<App>(mauiapp =>
                {
                    mauiapp
                    .ConfigureFonts(fonts =>
                    {
                        fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                        fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    });

#if DEBUG
                    mauiapp.Logging.AddDebug();
#endif
                });
                maui.AddSingletonPage<MainPage>();
                maui.AddSingletonPage<SecondPage>();
                maui.ConfigureContext(ctx =>
                {
                    // Example: configure the MAUI context
                    ctx.IsLifetimeLinked = true;
                });
            })
            .UseMauiLifetime();

        HostApp = builder.Build();
    }

    /// <summary>
    /// Gets the host.
    /// </summary>
    /// <value>
    /// The host.
    /// </value>
    public static IHost HostApp { get; }

    /// <summary>
    /// Creates the maui app.
    /// </summary>
    /// <returns>A MauiApp.</returns>
    public static MauiApp GetMauiApp() => HostApp.Services.GetRequiredService<MauiApp>();
}
