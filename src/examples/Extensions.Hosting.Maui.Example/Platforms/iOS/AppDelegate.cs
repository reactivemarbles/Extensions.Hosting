using Foundation;

namespace Extensions.Hosting.Maui.Example;

/// <summary>Provides the iOS application delegate for the sample MAUI application.</summary>
[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    /// <summary>Creates the maui application.</summary>
    /// <returns>A MauiApp.</returns>
    protected override MauiApp CreateMauiApp() => MauiProgram.GetMauiApp();
}
