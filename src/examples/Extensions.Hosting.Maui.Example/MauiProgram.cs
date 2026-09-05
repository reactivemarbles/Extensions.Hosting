// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Extensions.Hosting;
#if DEBUG
using Microsoft.Extensions.Logging;
#endif
using ReactiveMarbles.Extensions.Hosting.Maui;

namespace Extensions.Hosting.Maui.Example;

/// <summary>Configures and exposes the sample MAUI host.</summary>
public static class MauiProgram
{
    /// <summary>Initializes the <see cref="MauiProgram"/> class.</summary>
    static MauiProgram()
    {
        // Create the host
        var builder = Host.CreateApplicationBuilder();
        _ = builder.Services.AddSingleton<IMauiService, ExampleMauiService>();
        _ = builder
            .ConfigureMaui(static maui =>
            {
                _ = maui.UseMauiApp(static _ => new App(), static mauiapp =>
                {
                    _ = mauiapp
                        .ConfigureFonts(static fonts =>
                    {
                        _ = fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                        _ = fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    });

#if DEBUG
                    _ = mauiapp.Logging.AddDebug();
#endif
                });
                _ = maui.AddSingletonPage(typeof(MainPage));
                _ = maui.AddSingletonPage(typeof(SecondPage));
                _ = maui.ConfigureContext(static ctx =>
                {
                    // Example: configure the MAUI context
                    ctx.IsLifetimeLinked = true;
                });
            })
            .UseMauiLifetime();

        HostApp = builder.Build();
    }

    /// <summary>Gets the host.</summary>
    /// <value>
    /// The host.
    /// </value>
    public static IHost HostApp { get; }

    /// <summary>Creates the maui app.</summary>
    /// <returns>A MauiApp.</returns>
    public static MauiApp GetMauiApp() => HostApp.Services.GetRequiredService<MauiApp>();
}
