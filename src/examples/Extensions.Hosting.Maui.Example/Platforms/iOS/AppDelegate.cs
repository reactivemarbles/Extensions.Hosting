using Foundation;

namespace Extensions.Hosting.Maui.Example;

/// <summary>
/// AppDelegate.
/// </summary>
[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    /// <summary>
    /// Creates the maui application.
    /// </summary>
    /// <returns>A MauiApp.</returns>
    protected override MauiApp CreateMauiApp() => MauiProgram.GetMauiApp();
}
