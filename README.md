# Extensions.Hosting

`Extensions.Hosting` adds Generic Host integration for desktop UI frameworks, ReactiveUI/Splat, plug-ins, Windows service hosting, single-instance applications, and Identity/Entity Framework Core helpers.

The public namespaces use `ReactiveMarbles.Extensions.Hosting.*`.

## Package Matrix

| Package | Main namespace | Purpose |
| --- | --- | --- |
| `CP.Extensions.Hosting.MainUIThread` | `ReactiveMarbles.Extensions.Hosting.UiThread` | Shared UI context and UI-thread base types used by the platform host packages. |
| `CP.Extensions.Hosting.Wpf` | `ReactiveMarbles.Extensions.Hosting.Wpf` | WPF host integration, application/window registration, and WPF lifetime linking. |
| `CP.Extensions.Hosting.WinForms` | `ReactiveMarbles.Extensions.Hosting.WinForms` | WinForms host integration, form/shell registration, and WinForms lifetime linking. |
| `CP.Extensions.Hosting.WinUI` | `ReactiveMarbles.Extensions.Hosting.WinUI` | WinUI host integration for `Application` and main `Window` types. |
| `CP.Extensions.Hosting.Avalonia` | `ReactiveMarbles.Extensions.Hosting.Avalonia` | Avalonia host integration, `AppBuilder` configuration, application/window registration, and Avalonia lifetime linking. |
| `CP.Extensions.Hosting.Maui` | `ReactiveMarbles.Extensions.Hosting.Maui` | .NET MAUI host integration, `MauiAppBuilder` configuration, application/page registration, and MAUI lifetime linking. |
| `CP.Extensions.Hosting.SingleInstance` | `ReactiveMarbles.Extensions.Hosting.AppServices` | Mutex-backed single-instance enforcement and direct named resource mutex helpers. |
| `CP.Extensions.Hosting.Plugins` | `ReactiveMarbles.Extensions.Hosting.Plugins` | Plug-in discovery, assembly loading, ordering, and hosted-service plug-in base types. |
| `CP.Extensions.Hosting.Plugins.Reactive` | `ReactiveMarbles.Extensions.Hosting.Reactive.Plugins` | Reactive package variant of the plug-in API with the same source and API shape. |
| `CP.Extensions.Hosting.PluginService` | `ReactiveMarbles.Extensions.Hosting.PluginService` | Service-host bootstrapper, plug-in service discovery defaults, and `ServiceBase` lifetime support. |
| `CP.Extensions.Hosting.PluginService.Reactive` | `ReactiveMarbles.Extensions.Hosting.Reactive.PluginService` | Reactive package variant of the service-host API, referencing `CP.Extensions.Hosting.Plugins.Reactive`. |
| `CP.Extensions.Hosting.ReactiveUI.Wpf` | `ReactiveMarbles.Extensions.Hosting.ReactiveUI` | ReactiveUI/Splat integration for WPF using the standard ReactiveUI package set. |
| `CP.Extensions.Hosting.ReactiveUI.Wpf.Reactive` | `ReactiveMarbles.Extensions.Hosting.Reactive.ReactiveUI` | Reactive package variant of the WPF ReactiveUI/Splat integration. |
| `CP.Extensions.Hosting.ReactiveUI.WinForms` | `ReactiveMarbles.Extensions.Hosting.ReactiveUI` | ReactiveUI/Splat integration for WinForms. |
| `CP.Extensions.Hosting.ReactiveUI.WinForms.Reactive` | `ReactiveMarbles.Extensions.Hosting.Reactive.ReactiveUI` | Reactive package variant of the WinForms ReactiveUI/Splat integration. |
| `CP.Extensions.Hosting.ReactiveUI.WinUI` | `ReactiveMarbles.Extensions.Hosting.ReactiveUI` | ReactiveUI/Splat integration for WinUI. |
| `CP.Extensions.Hosting.ReactiveUI.WinUI.Reactive` | `ReactiveMarbles.Extensions.Hosting.Reactive.ReactiveUI` | Reactive package variant of the WinUI ReactiveUI/Splat integration. |
| `CP.Extensions.Hosting.ReactiveUI.Avalonia` | `ReactiveMarbles.Extensions.Hosting.ReactiveUI` | ReactiveUI/Splat integration for Avalonia. |
| `CP.Extensions.Hosting.ReactiveUI.Avalonia.Reactive` | `ReactiveMarbles.Extensions.Hosting.Reactive.ReactiveUI` | Reactive package variant of the Avalonia ReactiveUI/Splat integration. |
| `CP.Extensions.Hosting.ReactiveUI.Maui` | `ReactiveMarbles.Extensions.Hosting.ReactiveUI` | ReactiveUI/Splat integration for .NET MAUI. |
| `CP.Extensions.Hosting.ReactiveUI.Maui.Reactive` | `ReactiveMarbles.Extensions.Hosting.Reactive.ReactiveUI` | Reactive package variant of the MAUI ReactiveUI/Splat integration. |
| `CP.Extensions.Hosting.Identity.EntityFrameworkCore.SqlServer` | `ReactiveMarbles.Extensions.Hosting.Identity.EntityFrameworkCore` | SQL Server DbContext and ASP.NET Core Identity registration helpers. |
| `CP.Extensions.Hosting.Identity.EntityFrameworkCore.Sqlite` | `ReactiveMarbles.Extensions.Hosting.Identity.EntityFrameworkCore.Sqlite` | SQLite DbContext and ASP.NET Core Identity registration helpers. |

## Normal And Reactive Packages

The normal packages use the standard `ReactiveMarbles.Extensions.Hosting.*` namespaces. The `.Reactive` packages are source-linked sibling projects compiled with `REACTIVE_SHIM` and use `ReactiveMarbles.Extensions.Hosting.Reactive.*` namespaces where a namespace split is needed.

Use the normal packages when the application uses the standard ReactiveUI/ReactiveUI.Primitives package set. Use the `.Reactive` packages when the application uses `ReactiveUI.Primitives.*.Reactive` or `ReactiveUI.*.Reactive` packages. The hosting APIs are intentionally the same; migration usually means changing the package reference and namespace.

Do not add direct Rx.NET package references to application projects for this repository's hosting APIs. Use the ReactiveUI.Primitives package family, and use the `.Reactive` package variants only when a reactive bridge package is required.

```xml
<ItemGroup>
  <PackageReference Include="CP.Extensions.Hosting.Plugins" />
  <PackageReference Include="CP.Extensions.Hosting.ReactiveUI.Wpf" />
</ItemGroup>
```

```xml
<ItemGroup>
  <PackageReference Include="CP.Extensions.Hosting.Plugins.Reactive" />
  <PackageReference Include="CP.Extensions.Hosting.ReactiveUI.Wpf.Reactive" />
</ItemGroup>
```

```csharp
// Normal package
using ReactiveMarbles.Extensions.Hosting.Plugins;
using ReactiveMarbles.Extensions.Hosting.ReactiveUI;

// Reactive package
using ReactiveMarbles.Extensions.Hosting.Reactive.Plugins;
using ReactiveMarbles.Extensions.Hosting.Reactive.ReactiveUI;
```

For ReactiveUI.Primitives application code, project-file using aliases keep view models small and avoid repeated namespace noise:

```xml
<ItemGroup>
  <Using Include="ReactiveUI.Primitives" />
  <Using Include="ReactiveUI.Primitives.RxVoid" Alias="Unit" />
  <Using Include="ReactiveUI.Primitives.Signals" />
</ItemGroup>
```

## Target Frameworks

The libraries target .NET Framework where the platform supports it, current .NET TFMs, and `net11.0` preview TFMs. Windows desktop packages use Windows-specific TFMs.

| Area | Target frameworks |
| --- | --- |
| Shared, plug-ins, single instance | `net462`, `net472`, `net48`, `net481`, `net8.0`, `net9.0`, `net10.0`, `net11.0` |
| WPF, WinForms, PluginService | .NET Framework TFMs plus `net8.0-windows`, `net9.0-windows`, `net10.0-windows`, `net11.0-windows` |
| ReactiveUI WPF/WinForms | .NET Framework TFMs plus `net8.0-windows10.0.19041`, `net9.0-windows10.0.19041`, `net10.0-windows10.0.19041`, `net11.0-windows10.0.19041` |
| WinUI and ReactiveUI WinUI | `net8.0-windows10.0.19041`, `net9.0-windows10.0.19041`, `net10.0-windows10.0.19041`, `net11.0-windows10.0.19041` |
| Avalonia and ReactiveUI Avalonia | `net8.0`, `net9.0`, `net10.0`, `net11.0` |
| MAUI and ReactiveUI MAUI | `net10.0`, `net11.0`, Android, iOS, Mac Catalyst, macOS, tvOS, and Windows where supported |
| Identity EF Core helpers | `net8.0`, `net9.0`, `net10.0`, `net11.0` |

The repository uses Central Package Management in `Directory.Packages.props`. Notable centrally managed versions include `StyleSharp.Analyzers` `3.16.0`, `ReactiveUI.Primitives` `6.0.0`, `ReactiveUI` `24.0.0-beta.3`, and `ReactiveUI.Avalonia` `12.1.0-beta.1`.

## Host Builder Styles

Most packages support both Generic Host builder styles:

```csharp
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureServices(services =>
    {
        // Classic IHostBuilder configuration.
    })
    .Build();
```

```csharp
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// Modern IHostApplicationBuilder configuration.

using var host = builder.Build();
await host.RunAsync().ConfigureAwait(false);
```

The extension methods are fluent. Platform packages usually register a context, a UI thread adapter, and an `IHostedService` that starts the UI loop with the host.

## Shared UI Thread Infrastructure

Package: `CP.Extensions.Hosting.MainUIThread`

Namespace: `ReactiveMarbles.Extensions.Hosting.UiThread`

### API

| API | Description |
| --- | --- |
| `IUiContext.IsLifetimeLinked` | When `true`, the host is stopped when the UI application exits. |
| `IUiContext.IsRunning` | Tracks whether the UI application has started and is still running. |
| `BaseUiContext` | Simple base implementation of `IUiContext`. |
| `BaseUiThread<TContext>` | Base class for UI-thread adapters where `TContext : class, IUiContext`. |
| `BaseUiThread<TContext>.Start()` | Signals or starts the UI loop. |
| `BaseUiThread<TContext>.Dispose()` | Releases startup synchronization resources. |
| `BaseUiThread<TContext>.PreUiThreadStart()` | Override point that runs before the UI loop starts. |
| `BaseUiThread<TContext>.UiThreadStart()` | Override point that runs the platform message loop. |
| `BaseUiThread<TContext>.HandleApplicationExit()` | Marks the UI context stopped and requests host shutdown when lifetime is linked. |

### Custom UI Adapter Example

Most applications consume a platform package instead of deriving from `BaseUiThread<TContext>` directly. Use the base class when adding a new UI framework adapter.

```csharp
using Microsoft.Extensions.DependencyInjection;
using ReactiveMarbles.Extensions.Hosting.UiThread;

public sealed class CustomUiContext : BaseUiContext
{
    public object? Dispatcher { get; set; }
}

public sealed class CustomUiThread(IServiceProvider services)
    : BaseUiThread<CustomUiContext>(services)
{
    protected override void PreUiThreadStart()
    {
        UiContext.Dispatcher = new object();
    }

    protected override void UiThreadStart()
    {
        try
        {
            // Run the UI framework message loop here.
        }
        finally
        {
            HandleApplicationExit();
        }
    }
}
```

## WPF Hosting

Package: `CP.Extensions.Hosting.Wpf`

Namespace: `ReactiveMarbles.Extensions.Hosting.Wpf`

### API

| API | Description |
| --- | --- |
| `ConfigureWpf(Action<IWpfBuilder>? configureDelegate = null)` | Registers WPF hosting services and optional application/window types. Available on `IHostBuilder` and `IHostApplicationBuilder`. |
| `UseWpfLifetime(ShutdownMode shutdownMode = ShutdownMode.OnLastWindowClose)` | Links host shutdown to the WPF application lifetime. Call after `ConfigureWpf`. |
| `IWpfBuilder.UseApplication<TApplication>()` | Registers a WPF `Application` type. |
| `IWpfBuilder.UseCurrentApplication<TApplication>(TApplication currentApplication)` | Registers an existing WPF `Application` instance. |
| `IWpfBuilder.UseWindow<TWindow>()` | Registers a WPF `Window` type. If the window implements `IWpfShell`, it is also registered as the shell. |
| `IWpfBuilder.ConfigureContext(Action<IWpfContext> configureAction)` | Customizes the WPF context before it is used. |
| `IWpfContext.ShutdownMode` | WPF shutdown behavior. |
| `IWpfContext.WpfApplication` | Current WPF application instance. |
| `IWpfContext.Dispatcher` | WPF dispatcher for UI-thread work. |
| `IWpfShell` | Marker interface for a shell window. |

### Example

```csharp
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactiveMarbles.Extensions.Hosting.Wpf;

public sealed partial class App : Application
{
    private IHost? _host;

    protected override void OnStartup(StartupEventArgs e)
    {
        _host = new HostBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton<MainViewModel>();
            })
            .ConfigureWpf(wpf =>
            {
                wpf.UseCurrentApplication(this);
                wpf.UseWindow<MainWindow>();
                wpf.ConfigureContext(context =>
                    context.ShutdownMode = ShutdownMode.OnLastWindowClose);
            })
            .UseWpfLifetime()
            .Build();

        _ = _host.RunAsync();
        base.OnStartup(e);
    }
}

public sealed partial class MainWindow : Window, IWpfShell
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
```

## WinForms Hosting

Package: `CP.Extensions.Hosting.WinForms`

Namespace: `ReactiveMarbles.Extensions.Hosting.WinForms`

### API

| API | Description |
| --- | --- |
| `ConfigureWinForms(Action<IWinFormsContext>? configureAction = null)` | Registers WinForms hosting services. Available on `IHostBuilder` and `IHostApplicationBuilder`. |
| `ConfigureWinForms<TView>(Action<IWinFormsContext>? configureAction = null)` | Registers a `Form` as a singleton. If it implements `IWinFormsShell`, it is also registered as the shell. |
| `ConfigureWinFormsShell<TShell>()` | Registers a shell form where `TShell : Form, IWinFormsShell`. |
| `UseWinFormsLifetime()` | Links host shutdown to the WinForms message loop. |
| `IWinFormsContext.EnableVisualStyles` | Enables or disables WinForms visual styles before the UI starts. |
| `IWinFormsContext.Dispatcher` | Optional dispatcher used for marshaling work. |
| `IWinFormsShell` | Marker interface for the main form. |

### Example

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactiveMarbles.Extensions.Hosting.WinForms;
using System.Windows.Forms;

[STAThread]
public static class Program
{
    public static async Task Main(string[] args)
    {
        using var host = new HostBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton<MainPresenter>();
            })
            .ConfigureWinForms<MainForm>(context => context.EnableVisualStyles = true)
            .UseWinFormsLifetime()
            .Build();

        await host.RunAsync().ConfigureAwait(false);
    }
}

public sealed class MainForm : Form, IWinFormsShell
{
    public MainForm(MainPresenter presenter)
    {
        Text = presenter.Title;
    }
}
```

## WinUI Hosting

Package: `CP.Extensions.Hosting.WinUI`

Namespace: `ReactiveMarbles.Extensions.Hosting.WinUI`

### API

| API | Description |
| --- | --- |
| `ConfigureWinUI<TApp, TAppWindow>()` | Registers the WinUI `Application` and main `Window` types, the WinUI context, and the hosted service. Available on `IHostBuilder` and `IHostApplicationBuilder`. |
| `IWinUIContext.AppWindow` | Current WinUI window instance. |
| `IWinUIContext.AppWindowType` | Window type to create. Set by `ConfigureWinUI<TApp, TAppWindow>()`. |
| `IWinUIContext.Dispatcher` | `DispatcherQueue` for UI-thread work. |
| `IWinUIContext.WinUIApplication` | Current WinUI application instance. |
| `IWinUIService` | Marker interface for WinUI services. |

### Example

```csharp
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using ReactiveMarbles.Extensions.Hosting.WinUI;

public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        _host = new HostBuilder()
            .ConfigureWinUI<App, MainWindow>()
            .Build();
    }
}

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }
}
```

## Avalonia Hosting

Package: `CP.Extensions.Hosting.Avalonia`

Namespace: `ReactiveMarbles.Extensions.Hosting.Avalonia`

### API

| API | Description |
| --- | --- |
| `ConfigureAvalonia(Action<IAvaloniaBuilder>? configureDelegate = null)` | Registers Avalonia hosting services and optional application/window types. Available on `IHostBuilder` and `IHostApplicationBuilder`. |
| `UseAvaloniaLifetime(ShutdownMode shutdownMode = ShutdownMode.OnLastWindowClose)` | Links host shutdown to the Avalonia desktop lifetime. |
| `IAvaloniaBuilder.UseApplication<TApplication>()` | Registers an Avalonia `Application` type. |
| `IAvaloniaBuilder.UseCurrentApplication<TApplication>(TApplication currentApplication)` | Registers an existing Avalonia application instance. |
| `IAvaloniaBuilder.UseWindow<TWindow>()` | Registers an Avalonia `Window` type. If it implements `IAvaloniaShell`, it is also registered as the shell. |
| `IAvaloniaBuilder.ConfigureContext(Action<IAvaloniaContext> configureAction)` | Customizes the Avalonia context. |
| `IAvaloniaBuilder.ConfigureAppBuilder(Action<AppBuilder> configureAction)` | Customizes Avalonia `AppBuilder` before startup. |
| `IAvaloniaContext.ShutdownMode` | Avalonia shutdown behavior. |
| `IAvaloniaContext.AvaloniaApplication` | Current Avalonia application. |
| `IAvaloniaContext.ApplicationLifetime` | Avalonia desktop lifetime. |
| `IAvaloniaContext.Dispatcher` | Avalonia dispatcher. |
| `IAvaloniaShell` | Marker interface for the main window. |

### Example

```csharp
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Microsoft.Extensions.Hosting;
using ReactiveMarbles.Extensions.Hosting.Avalonia;

public static class Program
{
    [STAThread]
    public static int Main(string[] args)
    {
        using var host = new HostBuilder()
            .ConfigureAvalonia(avalonia =>
            {
                avalonia.UseApplication<App>();
                avalonia.UseWindow<MainWindow>();
                avalonia.ConfigureAppBuilder(appBuilder =>
                    appBuilder.UsePlatformDetect().LogToTrace());
            })
            .UseAvaloniaLifetime(ShutdownMode.OnLastWindowClose)
            .Build();

        host.Run();
        return 0;
    }
}

public sealed class MainWindow : Window, IAvaloniaShell
{
}
```

## MAUI Hosting

Package: `CP.Extensions.Hosting.Maui`

Namespace: `ReactiveMarbles.Extensions.Hosting.Maui`

### API

| API | Description |
| --- | --- |
| `ConfigureMaui(Action<IMauiBuilder>? configureDelegate = null)` | Registers MAUI hosting services, builds a `MauiApp`, and registers application/page types. Available on `IHostBuilder` and `IHostApplicationBuilder`. |
| `UseMauiLifetime()` | Links host shutdown to the MAUI lifetime. |
| `ConfigureMauiShell<TShell>()` | Registers a singleton shell page where `TShell : Page, IMauiShell`. |
| `IMauiBuilder.AddSingletonPage<TPage>()` | Registers a MAUI `Page` as a singleton. If it implements `IMauiShell`, it is also registered as the shell. |
| `IMauiBuilder.UseMauiApp<TApplication>(Action<MauiAppBuilder>? configureMauiApp = null)` | Registers a MAUI `Application` type and configures the underlying `MauiAppBuilder`. |
| `IMauiBuilder.UseMauiApp<TApplication>(TApplication currentApplication, Action<MauiAppBuilder>? configureMauiApp = null)` | Registers an existing MAUI application instance. |
| `IMauiBuilder.ConfigureContext(Action<IMauiContext> configureAction)` | Customizes the MAUI context. |
| `IMauiBuilder.MauiAppBuilder` | Underlying MAUI builder for fonts, handlers, services, and app configuration. |
| `IMauiContext.MauiApplication` | Current MAUI application. |
| `IMauiContext.Dispatcher` | MAUI dispatcher. |
| `IMauiShell` | Marker interface for the main shell page. |

### Example

```csharp
using Microsoft.Extensions.Hosting;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;
using ReactiveMarbles.Extensions.Hosting.Maui;

public static class MauiProgram
{
    public static IHost CreateHost()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureMaui(maui =>
            {
                maui.UseMauiApp<App>(builder =>
                {
                    builder.ConfigureFonts(fonts =>
                    {
                        fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    });
                });

                maui.AddSingletonPage<AppShell>();
            })
            .UseMauiLifetime()
            .Build();
    }
}

public sealed class AppShell : Shell, IMauiShell
{
}
```

## ReactiveUI And Splat Integration

Packages:

- `CP.Extensions.Hosting.ReactiveUI.Wpf`
- `CP.Extensions.Hosting.ReactiveUI.WinForms`
- `CP.Extensions.Hosting.ReactiveUI.WinUI`
- `CP.Extensions.Hosting.ReactiveUI.Avalonia`
- `CP.Extensions.Hosting.ReactiveUI.Maui`
- `.Reactive` siblings for each package

Normal namespace: `ReactiveMarbles.Extensions.Hosting.ReactiveUI`

Reactive namespace: `ReactiveMarbles.Extensions.Hosting.Reactive.ReactiveUI`

### API

| API | Description |
| --- | --- |
| `ConfigureSplatForMicrosoftDependencyResolver()` on `IHostBuilder` | Registers Splat to use Microsoft.Extensions.DependencyInjection and initializes ReactiveUI for the target platform. |
| `ConfigureSplatForMicrosoftDependencyResolver()` on `IHostApplicationBuilder` | Same behavior for the modern builder API. |
| `MapSplatLocator(Action<IServiceProvider?> containerFactory)` on `IHost` | Maps Splat to the built host service provider and invokes a custom service-provider callback. |

Each platform package initializes the matching ReactiveUI builder extension:

| Package | ReactiveUI builder call |
| --- | --- |
| WPF | `WithWpf()` |
| WinForms | `WithWinForms()` |
| WinUI | `WithWinUI()` |
| Avalonia | `WithAvalonia()` |
| MAUI | `WithMaui()` |

### WPF Example

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactiveMarbles.Extensions.Hosting.ReactiveUI;
using ReactiveMarbles.Extensions.Hosting.Wpf;
using ReactiveUI;

var host = new HostBuilder()
    .ConfigureSplatForMicrosoftDependencyResolver()
    .ConfigureServices(services =>
    {
        services.AddSingleton<MainWindowViewModel>();
        services.AddTransient<IViewFor<MainWindowViewModel>, MainWindow>();
    })
    .ConfigureWpf(wpf => wpf.UseApplication<App>().UseWindow<MainWindow>())
    .UseWpfLifetime()
    .Build();

host.MapSplatLocator(serviceProvider =>
{
    // Register additional Splat services that need the built provider.
});

await host.RunAsync().ConfigureAwait(false);
```

### Reactive Package Example

The API is the same. Use the `.Reactive` package and namespace.

```csharp
using Microsoft.Extensions.Hosting;
using ReactiveMarbles.Extensions.Hosting.Reactive.ReactiveUI;
using ReactiveMarbles.Extensions.Hosting.Wpf;

var host = new HostBuilder()
    .ConfigureSplatForMicrosoftDependencyResolver()
    .ConfigureWpf(wpf => wpf.UseApplication<App>().UseWindow<MainWindow>())
    .UseWpfLifetime()
    .Build();
```

### ReactiveUI.Primitives View Model Example

```csharp
using ReactiveUI;

public sealed class SearchViewModel : ReactiveObject
{
    private string? _searchText;
    private IReadOnlyList<string> _results = [];

    public string? SearchText
    {
        get => _searchText;
        set => this.RaiseAndSetIfChanged(ref _searchText, value);
    }

    public IReadOnlyList<string> Results
    {
        get => _results;
        private set => this.RaiseAndSetIfChanged(ref _results, value);
    }

    public void Search(IEnumerable<string> source)
    {
        var filter = SearchText ?? string.Empty;
        Results = source
            .Where(item => item.Contains(filter, StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }
}
```

## Single-Instance Applications

Package: `CP.Extensions.Hosting.SingleInstance`

Namespace: `ReactiveMarbles.Extensions.Hosting.AppServices`

### API

| API | Description |
| --- | --- |
| `ConfigureSingleInstance(Action<IMutexBuilder> configureAction)` | Registers mutex-backed single-instance enforcement. Available on `IHostBuilder` and `IHostApplicationBuilder`. |
| `ConfigureSingleInstance(string mutexId)` | Convenience overload that sets only the mutex id. |
| `IMutexBuilder.MutexId` | Unique named mutex identifier. |
| `IMutexBuilder.IsGlobal` | Uses the `Global\` mutex namespace when `true`; otherwise uses `Local\`. |
| `IMutexBuilder.WhenNotFirstInstance` | Callback invoked with `IHostEnvironment` and `ILogger` when another instance already owns the mutex. |
| `ResourceMutex.Create(ILogger? logger, string? mutexId, string? resourceName = null, bool global = false)` | Creates and locks a named mutex directly. |
| `ResourceMutex.IsLocked` | Indicates whether the mutex was acquired. |
| `ResourceMutex.Lock()` | Attempts to acquire the mutex. |
| `ResourceMutex.Dispose()` | Releases the mutex on the owning thread. |

### Host Example

```csharp
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReactiveMarbles.Extensions.Hosting.AppServices;

var host = new HostBuilder()
    .ConfigureSingleInstance(singleInstance =>
    {
        singleInstance.MutexId = "{D7F93D6D-6B4D-4F58-8E76-E68C51D9E92C}";
        singleInstance.IsGlobal = false;
        singleInstance.WhenNotFirstInstance = (environment, logger) =>
        {
            logger.LogWarning(
                "Application {ApplicationName} is already running.",
                environment.ApplicationName);
        };
    })
    .Build();

await host.RunAsync().ConfigureAwait(false);
```

### Direct ResourceMutex Example

```csharp
using Microsoft.Extensions.Logging;
using ReactiveMarbles.Extensions.Hosting.AppServices;

using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger("Mutex");

using var mutex = ResourceMutex.Create(
    logger,
    "import-job",
    resourceName: "Nightly import",
    global: false);

if (!mutex.IsLocked)
{
    return;
}

// Run the protected work.
```

## Plug-ins

Packages:

- `CP.Extensions.Hosting.Plugins`
- `CP.Extensions.Hosting.Plugins.Reactive`

Normal namespace: `ReactiveMarbles.Extensions.Hosting.Plugins`

Reactive namespace: `ReactiveMarbles.Extensions.Hosting.Reactive.Plugins`

### API

| API | Description |
| --- | --- |
| `ConfigurePlugins(Action<IPluginBuilder?> configurePlugin)` | Configures plug-in scanning and loading. Available on `IHostBuilder` and `IHostApplicationBuilder`. |
| `IPlugin.ConfigureHost(object hostBuilderContext, IServiceCollection serviceCollection)` | Called for each discovered plug-in so it can register services. |
| `IPluginBuilder.PluginDirectories` | Root directories scanned for plug-in assemblies. |
| `IPluginBuilder.FrameworkDirectories` | Root directories scanned for framework assemblies. |
| `IPluginBuilder.UseContentRoot` | Adds the host content root to the scan directories when enabled. |
| `IPluginBuilder.FailIfNoPlugins` | Throws during startup when no plug-ins are discovered. Defaults to `false`. |
| `IPluginBuilder.FrameworkMatcher` | Glob matcher for framework assemblies. |
| `IPluginBuilder.PluginMatcher` | Glob matcher for plug-in assemblies. |
| `IPluginBuilder.ValidatePlugin` | Predicate used before loading a plug-in assembly path. |
| `IPluginBuilder.AssemblyScanFunc` | Assembly scanning delegate. Defaults to scanning exported public plug-in types. |
| `AddScanDirectories(params string[] directories)` | Adds directories to both framework and plug-in scan roots. |
| `IncludeFrameworks(params string[] frameworkGlobs)` | Adds framework include globs. |
| `ExcludeFrameworks(params string[] frameworkGlobs)` | Adds framework exclude globs. |
| `IncludePlugins(params string[] pluginGlobs)` | Adds plug-in include globs. |
| `ExcludePlugins(params string[] pluginGlobs)` | Adds plug-in exclude globs. |
| `RequirePlugins(bool failIfNone = true)` | Sets `FailIfNoPlugins`. |
| `PluginScanner.ByNamingConvention(Assembly pluginAssembly)` | Finds a conventional `{AssemblyName}.Plugin` type. |
| `PluginScanner.ScanForPluginInstances(Assembly pluginAssembly)` | Finds public concrete classes implementing `IPlugin`. |
| `PluginOrderAttribute` | Orders discovered plug-ins. Lower values run earlier. Constructors accept no value, an `int`, or an enum/object value. |
| `PluginBase<T>`, `PluginBase<T1,T2>`, `PluginBase<T1,T2,T3>` | Base plug-ins that register one, two, or three hosted services. |
| `HostedServiceBase<T>` | Base hosted service with logging, cleanup, and lifetime callback hooks. |

`HostedServiceBase<T>` exposes:

| Member | Description |
| --- | --- |
| `CleanUp` | Disposable collection cleaned when the service stops or is disposed. |
| `Logger` | Logger passed to the base constructor. |
| `IsDisposed` | Indicates whether the cleanup collection has been disposed. |
| `OnStarted()` | Override to start work after the application starts. |
| `OnStopping()` | Override to stop work and dispose resources. Call the base implementation unless you intentionally replace cleanup behavior. |
| `OnStopped()` | Override to run final stopped logic. |

### Host Scanning Example

```csharp
using Microsoft.Extensions.Hosting;
using ReactiveMarbles.Extensions.Hosting.Plugins;

var host = new HostBuilder()
    .ConfigurePlugins(plugins =>
    {
        plugins?.AddScanDirectories(AppContext.BaseDirectory);
        plugins?.IncludeFrameworks("frameworks/**/*.dll");
        plugins?.IncludePlugins("plugins/**/*.Plugin.dll");
        plugins?.ExcludePlugins("plugins/**/*.Disabled.dll");
        plugins?.RequirePlugins();
        if (plugins is not null)
        {
            plugins.AssemblyScanFunc = PluginScanner.ScanForPluginInstances;
        }
    })
    .Build();

await host.RunAsync().ConfigureAwait(false);
```

### Plug-in Assembly Example

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReactiveMarbles.Extensions.Hosting.Plugins;

[PluginOrder(10)]
public sealed class Plugin : IPlugin
{
    public void ConfigureHost(object hostBuilderContext, IServiceCollection serviceCollection)
    {
        serviceCollection.AddHostedService<ImportHostedService>();
    }
}

public sealed class ImportHostedService(
    ILogger<ImportHostedService> logger,
    IHostApplicationLifetime lifetime)
    : HostedServiceBase<ImportHostedService>(logger, lifetime)
{
    public override Task OnStarted()
    {
        Logger.LogInformation("Import service started.");
        return Task.CompletedTask;
    }
}
```

### Plug-in Base Example

```csharp
using ReactiveMarbles.Extensions.Hosting.Plugins;

public sealed class Plugin
    : PluginBase<ImportHostedService, ExportHostedService>
{
}
```

### Reactive Plug-in Example

Only the package and namespace change.

```csharp
using Microsoft.Extensions.DependencyInjection;
using ReactiveMarbles.Extensions.Hosting.Reactive.Plugins;

public sealed class Plugin : IPlugin
{
    public void ConfigureHost(object hostBuilderContext, IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<ReactivePluginService>();
    }
}
```

## Plug-in Service Hosting

Packages:

- `CP.Extensions.Hosting.PluginService`
- `CP.Extensions.Hosting.PluginService.Reactive`

Normal namespace: `ReactiveMarbles.Extensions.Hosting.PluginService`

Reactive namespace: `ReactiveMarbles.Extensions.Hosting.Reactive.PluginService`

### API

| API | Description |
| --- | --- |
| `ServiceHost.Logger` | Gets the `ILogger` created by the service host after startup. |
| `ServiceHost.Create(Type type, string[] args, Func<IHostBuilder?, IHostBuilder?>? hostBuilder = default, Action<IHost>? configureHost = default, string nameSpace = "ReactiveMarbles.Plugin", string? targetRuntime = null)` | Creates, configures, and runs a classic `IHostBuilder` service host. |
| `ServiceHost.CreateApplication(Type type, string[] args, Func<IHostApplicationBuilder?, IHostApplicationBuilder?>? hostBuilder = default, Action<IHost>? configureHost = default, string nameSpace = "ReactiveMarbles.Plugin", string? targetRuntime = null)` | Creates, configures, and runs a modern `IHostApplicationBuilder` service host. |
| `UseServiceBaseLifetime()` on `IHostBuilder` | Registers `ServiceBaseLifetime` as `IHostLifetime`. |
| `UseServiceBaseLifetime()` on `IHostApplicationBuilder` | Registers `ServiceBaseLifetime` as `IHostLifetime`. |
| `UseConsoleLifetime()` on `IHostApplicationBuilder` | Registers `ConsoleLifetime` as `IHostLifetime`. |
| `RunAsServiceAsync(CancellationToken cancellationToken = default)` on `IHostBuilder` | Builds and runs with `ServiceBaseLifetime`. |
| `RunAsServiceAsync(CancellationToken cancellationToken = default)` on `HostApplicationBuilder` | Builds and runs with `ServiceBaseLifetime`. |
| `ServiceBaseLifetime` | `IHostLifetime` implementation backed by `System.ServiceProcess.ServiceBase`. |

`ServiceHost` configures:

- Content root as the current directory.
- Logging from configuration plus console, event log, log4net, and debug providers.
- Host configuration from `hostsettings.json`, environment variables with the `PREFIX_` prefix, and command line.
- Application configuration from `appsettings.json`, `appsettings.{Environment}.json`, environment variables with the `PREFIX_` prefix, and command line.
- Plug-in scanning from the process directory, framework assemblies matching `\netstandard2.0\*.FrameworkLib.dll`, and plug-ins matching `\Plugins\{runtime}\{nameSpace}*.dll`.
- Windows service lifetime when a debugger is not attached and `--console` is not supplied; console lifetime otherwise.

### Console Or Service Entry Point

```csharp
using ReactiveMarbles.Extensions.Hosting.PluginService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

await ServiceHost.Create(
    typeof(Program),
    args,
    hostBuilder: builder => builder?.ConfigureServices(services =>
    {
        services.AddSingleton<SharedService>();
    }),
    configureHost: host =>
    {
        ServiceHost.Logger?.LogInformation("Host configured.");
    },
    nameSpace: "Contoso.Plugin",
    targetRuntime: "win-x64")
    .ConfigureAwait(false);
```

### Modern Builder Entry Point

```csharp
using ReactiveMarbles.Extensions.Hosting.PluginService;

await ServiceHost.CreateApplication(
    typeof(Program),
    args,
    hostBuilder: builder =>
    {
        builder?.Services.AddSingleton<SharedService>();
        return builder;
    })
    .ConfigureAwait(false);
```

### Direct Service Lifetime Example

```csharp
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using ReactiveMarbles.Extensions.Hosting.PluginService;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.UseServiceBaseLifetime();

await builder.Build().RunAsync().ConfigureAwait(false);
```

### Reactive Service Host Example

```csharp
using ReactiveMarbles.Extensions.Hosting.Reactive.PluginService;

await ServiceHost.CreateApplication(typeof(Program), args)
    .ConfigureAwait(false);
```

## Identity And Entity Framework Core

SQL Server package: `CP.Extensions.Hosting.Identity.EntityFrameworkCore.SqlServer`

SQL Server namespace: `ReactiveMarbles.Extensions.Hosting.Identity.EntityFrameworkCore`

SQLite package: `CP.Extensions.Hosting.Identity.EntityFrameworkCore.Sqlite`

SQLite namespace: `ReactiveMarbles.Extensions.Hosting.Identity.EntityFrameworkCore.Sqlite`

### SQL Server API

| API | Description |
| --- | --- |
| `IConfiguration.HasConnectionString(string connectionStringName)` | Returns `true` when the named connection string exists and is not empty. |
| `IConfiguration.GetRequiredConnectionString(string connectionStringName)` | Gets the named connection string or throws a descriptive `InvalidOperationException`. |
| `IHostApplicationBuilder.AddSqlServerDbContext<TContext>(string connectionStringName, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)` | Registers a SQL Server `DbContext`. |
| `IHostApplicationBuilder.AddSqlServerWithIdentity<TContext,TUser,TRole>(string connectionStringName, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)` | Registers SQL Server EF Core plus ASP.NET Core Identity with roles. |
| `IHostApplicationBuilder.AddSqlServerWithIdentity<TContext,TUser>(string connectionStringName, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)` | Registers SQL Server EF Core plus ASP.NET Core Identity without a custom role type. |
| `IHostBuilder.UseWebHostServices(...)` | Adds minimal ASP.NET Core web-host services. Three overloads support service-only, web-host customization, and web-host plus app-pipeline customization. |
| `IServiceCollection.UseEntityFrameworkCoreSqlServer<TContext,TUser,TRole>(WebHostBuilderContext context, string connectionStringName, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)` | Registers SQL Server EF Core plus Identity with roles in a web-host service callback. |
| `IServiceCollection.UseEntityFrameworkCoreSqlServer<TContext,TUser>(WebHostBuilderContext context, string connectionStringName, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)` | Registers SQL Server EF Core plus Identity without a custom role type. |
| `IServiceCollection.AddSqlServerDbContext<TContext>(IConfiguration configuration, string connectionStringName, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)` | Registers a SQL Server `DbContext` from configuration. |
| `IServiceCollection.AddSqlServerDbContextWithConnectionString<TContext>(string connectionString, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)` | Registers a SQL Server `DbContext` from a direct connection string. |

### SQLite API

| API | Description |
| --- | --- |
| `IHostApplicationBuilder.AddSqliteDbContext<TContext>(string connectionStringName, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)` | Registers a SQLite `DbContext`. |
| `IHostApplicationBuilder.AddSqliteWithIdentity<TContext,TUser,TRole>(string connectionStringName, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)` | Registers SQLite EF Core plus ASP.NET Core Identity with roles. |
| `IHostApplicationBuilder.AddSqliteWithIdentity<TContext,TUser>(string connectionStringName, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)` | Registers SQLite EF Core plus ASP.NET Core Identity without a custom role type. |
| `IHostBuilder.UseWebHostServices(...)` | Adds minimal ASP.NET Core web-host services. Three overloads match the SQL Server package. |
| `IServiceCollection.UseEntityFrameworkCoreSqlite<TContext,TUser,TRole>(WebHostBuilderContext context, string connectionStringName, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)` | Registers SQLite EF Core plus Identity with roles in a web-host service callback. |
| `IServiceCollection.UseEntityFrameworkCoreSqlite<TContext,TUser>(WebHostBuilderContext context, string connectionStringName, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)` | Registers SQLite EF Core plus Identity without a custom role type. |
| `IServiceCollection.AddSqliteDbContext<TContext>(IConfiguration configuration, string connectionStringName, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)` | Registers a SQLite `DbContext` from configuration. |
| `IServiceCollection.AddSqliteDbContextWithConnectionString<TContext>(string connectionString, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)` | Registers a SQLite `DbContext` from a direct connection string. |
| `CreateInMemoryConnectionString(string? databaseName = null)` | Creates a SQLite in-memory connection string. |
| `CreateFileConnectionString(string filePath)` | Creates a SQLite file connection string. |

### Modern Builder Example

```csharp
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using ReactiveMarbles.Extensions.Hosting.Identity.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.AddSqlServerWithIdentity<AppDbContext, IdentityUser, IdentityRole>(
    "DefaultConnection");

using var host = builder.Build();
await host.RunAsync().ConfigureAwait(false);

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<IdentityUser, IdentityRole, string>(options)
{
}
```

### Web Host Services Example

```csharp
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;
using ReactiveMarbles.Extensions.Hosting.Identity.EntityFrameworkCore.Sqlite;

var host = new HostBuilder()
    .UseWebHostServices((context, services) =>
    {
        services.UseEntityFrameworkCoreSqlite<AppDbContext, IdentityUser>(
            context,
            "DefaultConnection");
    })
    .Build();

await host.RunAsync().ConfigureAwait(false);
```

### Direct SQLite Connection Example

```csharp
using Microsoft.Extensions.DependencyInjection;
using ReactiveMarbles.Extensions.Hosting.Identity.EntityFrameworkCore.Sqlite;

var services = new ServiceCollection();
var connectionString =
    HostBuilderEntityFrameworkCoreExtensions.CreateInMemoryConnectionString("tests");

services.AddSqliteDbContextWithConnectionString<AppDbContext>(connectionString);
```

## Recommended Composition

The packages are designed to compose as small host extensions. A desktop app can combine single-instance enforcement, ReactiveUI/Splat, platform lifetime, and plug-in loading in one host.

```csharp
using Microsoft.Extensions.Hosting;
using ReactiveMarbles.Extensions.Hosting.AppServices;
using ReactiveMarbles.Extensions.Hosting.Plugins;
using ReactiveMarbles.Extensions.Hosting.ReactiveUI;
using ReactiveMarbles.Extensions.Hosting.Wpf;

var host = new HostBuilder()
    .ConfigureSplatForMicrosoftDependencyResolver()
    .ConfigureSingleInstance("{1E938C68-6B3E-4105-BE52-2432FE80F19C}")
    .ConfigurePlugins(plugins =>
    {
        plugins?.AddScanDirectories(AppContext.BaseDirectory);
        plugins?.IncludePlugins("plugins/**/*.dll");
    })
    .ConfigureWpf(wpf => wpf.UseApplication<App>().UseWindow<MainWindow>())
    .UseWpfLifetime()
    .Build();

await host.RunAsync().ConfigureAwait(false);
```

## Build And Test

The repository uses the XML `.slnx` solution format and Microsoft Testing Platform with TUnit.

```powershell
cd src
dotnet workload restore
dotnet restore Extensions.Hosting.slnx
dotnet build Extensions.Hosting.slnx -c Release -warnaserror
dotnet test --solution Extensions.Hosting.slnx -c Release
dotnet test --solution Extensions.Hosting.slnx --coverage --coverage-output-format cobertura
```

Run full builds on Windows because WPF, WinForms, WinUI, Windows service, and .NET Framework target frameworks are part of the solution.
