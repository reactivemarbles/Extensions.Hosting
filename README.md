# Extensions.Hosting

`Extensions.Hosting` adds Generic Host integration for desktop UI frameworks, ReactiveUI/Splat, plug-ins, Windows service hosting, single-instance applications, and Identity/Entity Framework Core helpers.

The public namespaces use `ReactiveMarbles.Extensions.Hosting.*`.

Use this guide as both a tutorial and the package API reference. Public APIs are
grouped by feature, every overload is listed explicitly, and the
[V3.1.26 to V4.0.0 migration guide](#migrate-from-v3126-to-v400) calls out source,
binary, dependency, target-framework, and behavior changes.

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
| `CP.Extensions.Logging.Log4Net` | `ReactiveMarbles.Extensions.Logging` | Microsoft.Extensions.Logging provider backed by log4net, including scopes, configuration overrides, and custom event/level adapters. |

Install only the feature packages that the application uses. The platform packages
already reference `CP.Extensions.Hosting.MainUIThread`; reference that shared package
directly only when building a new UI adapter.

```powershell
dotnet add package CP.Extensions.Hosting.Wpf --version 4.0.0
dotnet add package CP.Extensions.Hosting.ReactiveUI.Wpf --version 4.0.0
dotnet add package CP.Extensions.Hosting.SingleInstance --version 4.0.0
```

Replace the platform names in this example for WinForms, WinUI, Avalonia, or MAUI.
Use matching versions for all `CP.Extensions.Hosting.*` packages in an application.

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
| MAUI host | `net9.0`, `net10.0`, `net11.0`; platform workloads start at `net10.0-*` and include Android, iOS, Mac Catalyst, macOS, and tvOS where supported |
| ReactiveUI MAUI | `net10.0`, `net11.0`; matching platform workload TFMs start at `net10.0-*` |
| Identity EF Core helpers | `net8.0`, `net9.0`, `net10.0`, `net11.0` |
| Log4Net | `net462`, `net472`, `net48`, `net481`, `net8.0`, `net9.0`, `net10.0`, `net11.0` |

The repository uses Central Package Management in `Directory.Packages.props`. Notable
centrally managed versions include `StyleSharp.Analyzers` `3.39.0`,
`ReactiveUI.Primitives` `7.1.0`, `ReactiveUI` `24.0.0`,
`ReactiveUI.Avalonia` `12.1.0`, and `Splat` `20.2.0`. `net11.0` support uses preview
SDK and workload packages until those dependencies become stable.

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

The extension methods are fluent. Platform packages register a context, a UI-thread
adapter, and an `IHostedService` that starts the UI loop with the host. These framework
hosted-service types are public for composition and testing, but applications normally
let `Configure*` register them rather than registering them a second time.

### Configuration Order

1. Add application services and, when needed, call
   `ConfigureSplatForMicrosoftDependencyResolver()`.
2. Call the platform `Configure*` method.
3. Call the matching `Use*Lifetime` method where one exists.
4. Build the host once.
5. Call `MapSplatLocator` on the built host when Splat needs the final service provider.
6. Run and dispose the host.

`UseWpfLifetime` and `UseAvaloniaLifetime` throw `NotSupportedException` when invoked
before their matching `Configure*` method. WinUI installs its hosted lifetime from
`ConfigureWinUI<TApp,TAppWindow>()`; there is no `UseWinUILifetime` method.

### Shared Platform Lifecycle

Every platform context inherits `IUiContext.IsRunning` and
`IUiContext.IsLifetimeLinked`. `IsRunning` becomes `true` when platform startup is
complete. When lifetime linking is enabled, UI exit requests
`IHostApplicationLifetime.StopApplication()`. Host shutdown dispatches the matching UI
exit operation when the context is still running.

Register platform services to perform UI-thread initialization after the platform
application has been created:

| Platform | UI-thread initialization contract |
| --- | --- |
| WPF | `IWpfService.Initialize(System.Windows.Application)` |
| WinForms | `IWinFormsService.Initialize()` |
| WinUI | `IWinUIService.Initialize(Microsoft.UI.Xaml.Application)` |
| Avalonia | `IAvaloniaService.Initialize(Avalonia.Application)` |
| MAUI | `IMauiService.Initialize(Microsoft.Maui.Controls.Application)` |

```csharp
using Microsoft.Extensions.DependencyInjection;
using ReactiveMarbles.Extensions.Hosting.Wpf;

public sealed class ThemeInitializer : IWpfService
{
    public void Initialize(System.Windows.Application application)
    {
        application.Resources["AccentBrush"] =
            new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Colors.CornflowerBlue);
    }
}

builder.Services.AddSingleton<IWpfService, ThemeInitializer>();
```

Keep `Initialize` work short and non-blocking because it runs on the UI thread.
Platform thread types in `*.Internals` are public for compatibility and advanced
adapter/testing scenarios. Treat them as framework-owned: prefer `Configure*` and do
not replace or double-register the context, thread, or hosted service in normal apps.

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
| `BaseUiThread<TContext>(IServiceProvider)` | Creates an adapter that owns a dedicated UI thread. |
| `BaseUiThread<TContext>(IServiceProvider, bool useDedicatedUiThread)` | Allows an adapter to run on a dedicated thread or synchronously on the caller. |
| `BaseUiThread<TContext>.UiContext` | Protected platform context resolved from DI. |
| `BaseUiThread<TContext>.ServiceProvider` | Protected service provider used for platform application, shell, and service resolution. |
| `BaseUiThread<TContext>.Start()` | Signals or starts the UI loop. |
| `BaseUiThread<TContext>.Dispose()` | Releases startup synchronization resources. |
| `BaseUiThread<TContext>.Dispose(bool)` | Virtual protected disposal hook for derived adapters. |
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

On Windows, dedicated UI threads are created as STA threads. With
`useDedicatedUiThread: false`, `Start()` executes the UI loop synchronously and can
block the caller until that loop exits. `HandleApplicationExit()` requests host
shutdown only when lifetime linking is enabled and the host is not already stopping or
stopped.

## WPF Hosting

Package: `CP.Extensions.Hosting.Wpf`

Namespace: `ReactiveMarbles.Extensions.Hosting.Wpf`

### API

| API | Description |
| --- | --- |
| `ConfigureWpf()` / `ConfigureWpf(Action<IWpfBuilder>? configureDelegate)` | Registers WPF context, UI thread, hosted service, and optional application/window types. Both overloads are available on `IHostBuilder` and `IHostApplicationBuilder`. |
| `UseWpfLifetime()` / `UseWpfLifetime(ShutdownMode shutdownMode)` | Links host shutdown to WPF. The no-argument overload uses `OnLastWindowClose`. Call after `ConfigureWpf`; otherwise it throws `NotSupportedException`. |
| `IWpfBuilder.UseApplication<TApplication>()` | Registers a WPF `Application` type. |
| `IWpfBuilder.UseCurrentApplication<TApplication>(TApplication currentApplication)` | Registers an existing WPF `Application` instance. |
| `IWpfBuilder.UseWindow<TWindow>() where TWindow : Window` | Registers a singleton WPF window. If it implements `IWpfShell`, it is also registered as the shell. |
| `IWpfBuilder.ConfigureContext(Action<IWpfContext> configureAction)` | Customizes the WPF context before it is used. |
| `IWpfBuilder.ApplicationType` / `Application` | Configured application type and optional current application instance. |
| `IWpfBuilder.WindowTypes` | Ordered list of singleton window types to register and show. |
| `IWpfBuilder.ConfigureContextAction` | Context callback recorded by the fluent builder. |
| `IWpfContext.ShutdownMode` | WPF shutdown behavior. |
| `IWpfContext.WpfApplication` | Current WPF application instance. |
| `IWpfContext.Dispatcher` | WPF dispatcher for UI-thread work. |
| `IWpfService.Initialize(Application)` | Initializes a registered service on the hosted WPF UI thread. |
| `IWpfShell : IInputElement` | Shell contract. A registered shell normally also derives from `Window`. |
| `WpfThread(IServiceProvider)` | Public platform adapter constructed automatically by `ConfigureWpf`. |
| `WpfHostedService(ILogger<WpfHostedService>, WpfThread, IWpfContext)` | Hosted lifetime service constructed automatically by DI. |
| `WpfHostedService.StartAsync` / `StopAsync` | Starts the registered `WpfThread`; on host shutdown, dispatcher-invokes `Application.Shutdown()` when WPF is running. |

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

Register zero, one, or multiple shell windows. One shell is passed to
`Application.Run`; multiple shells are shown at startup. When using an existing
application whose dispatcher belongs to another thread, remove `StartupUri` and
provide a shell window or an existing `MainWindow`.

## WinForms Hosting

Package: `CP.Extensions.Hosting.WinForms`

Namespace: `ReactiveMarbles.Extensions.Hosting.WinForms`

### API

| API | Description |
| --- | --- |
| `ConfigureWinForms()` / `ConfigureWinForms(Action<IWinFormsContext>? configureAction)` | Registers WinForms context, UI thread, and hosted service. Available on both host builder styles. |
| `ConfigureWinForms<TView>()` / `ConfigureWinForms<TView>(Action<IWinFormsContext>? configureAction) where TView : Form` | Registers a form as a singleton. If it implements `IWinFormsShell`, it is also mapped as the shell. |
| `ConfigureWinFormsShell<TShell>() where TShell : Form, IWinFormsShell` | Registers a singleton shell form. |
| `UseWinFormsLifetime()` | Links host shutdown to the WinForms message loop. |
| `IWinFormsContext.EnableVisualStyles` | Enables or disables WinForms visual styles before the UI starts. |
| `IWinFormsContext.Dispatcher` | Optional dispatcher used for marshaling work. |
| `IWinFormsService.Initialize()` | Initializes a registered service on the WinForms UI thread before the message loop runs. |
| `IWinFormsShell : IComponent` | Main-form contract; normal shell implementations derive from `Form`. |
| `WinFormsThread(IServiceProvider)` | Public platform adapter constructed automatically by `ConfigureWinForms`. |
| `MultiShellContext(params Form[] forms)` | `ApplicationContext` used internally to keep a multiple-form message loop alive until every shell closes. |
| `WinFormsHostedService(ILogger<WinFormsHostedService>, WinFormsThread, IWinFormsContext)` | Hosted lifetime service constructed automatically by DI. |
| `WinFormsHostedService.StartAsync` / `StopAsync` | Starts the UI thread. Shutdown snapshots all open forms, closes and disposes each, logs individual cleanup failures, and exits the UI thread. |

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

Registering no shell starts an empty WinForms message loop. One shell is passed to
`Application.Run`; multiple shells use a shared `ApplicationContext` and exit after the
last form closes. The classic builder extensions retain nullable `IHostBuilder?` return
types for compatibility; modern builder overloads return `IHostApplicationBuilder`.

## WinUI Hosting

Package: `CP.Extensions.Hosting.WinUI`

Namespace: `ReactiveMarbles.Extensions.Hosting.WinUI`

### API

| API | Description |
| --- | --- |
| `ConfigureWinUI<TApp,TAppWindow>() where TApp : Application where TAppWindow : Window` | Registers the WinUI application, main window, context, UI thread, and hosted service. Available on both builder styles. |
| `IWinUIContext.AppWindow` | Current WinUI window instance. |
| `IWinUIContext.AppWindowType` | Window type to create. Set by `ConfigureWinUI<TApp, TAppWindow>()`. |
| `IWinUIContext.Dispatcher` | `DispatcherQueue` for UI-thread work. |
| `IWinUIContext.WinUIApplication` | Current WinUI application instance. |
| `IWinUIService.Initialize(Application)` | Initializes a registered service on the hosted WinUI thread before the main window is activated. |
| `WinUIThread(IServiceProvider)` | Public platform adapter constructed automatically by `ConfigureWinUI`. |
| `WinUIHostedService(ILogger<WinUIHostedService>, WinUIThread, IWinUIContext)` | Hosted lifetime service constructed automatically by DI. |
| `WinUIHostedService.StartAsync` / `StopAsync` | Starts the WinUI loop and dispatcher-enqueues `Application.Exit()` during host shutdown. |

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

`ConfigureWinUI` provides the lifetime integration; do not look for a separate
`UseWinUILifetime`. If the application is running, `StopAsync` throws
`InvalidOperationException` when no dispatcher is available or `TryEnqueue` rejects the
exit callback. It waits for the queued exit callback and honors cancellation both
before and while waiting. A pre-cancelled `StartAsync` does not start the UI thread.

## Avalonia Hosting

Package: `CP.Extensions.Hosting.Avalonia`

Namespace: `ReactiveMarbles.Extensions.Hosting.Avalonia`

### API

| API | Description |
| --- | --- |
| `ConfigureAvalonia()` / `ConfigureAvalonia(Action<IAvaloniaBuilder>? configureDelegate)` | Registers Avalonia context, application builder, hosted service, and optional application/window types. Available on both builder styles. |
| `UseAvaloniaLifetime()` / `UseAvaloniaLifetime(ShutdownMode shutdownMode)` | Links host shutdown to the desktop lifetime. The no-argument overload uses `OnLastWindowClose`. Call after `ConfigureAvalonia`; otherwise it throws `NotSupportedException`. |
| `IAvaloniaBuilder.UseApplication<TApplication>()` | Registers an Avalonia `Application` type. |
| `IAvaloniaBuilder.UseCurrentApplication<TApplication>(TApplication currentApplication)` | Registers an existing Avalonia application instance. |
| `IAvaloniaBuilder.UseWindow<TWindow>()` | Registers an Avalonia `Window` type. If it implements `IAvaloniaShell`, it is also registered as the shell. |
| `IAvaloniaBuilder.ConfigureContext(Action<IAvaloniaContext> configureAction)` | Customizes the Avalonia context. |
| `IAvaloniaBuilder.ConfigureAppBuilder(Action<AppBuilder> configureAction)` | Customizes Avalonia `AppBuilder` before startup. |
| `IAvaloniaBuilder.ApplicationType` / `Application` | Configured application type and optional current application instance. |
| `IAvaloniaBuilder.WindowTypes` | Ordered list of singleton window types to register. |
| `IAvaloniaBuilder.ConfigureContextAction` / `ConfigureAppBuilderAction` | Callbacks recorded by the fluent builder. |
| `IAvaloniaContext.ShutdownMode` | Avalonia shutdown behavior. |
| `IAvaloniaContext.AvaloniaApplication` | Current Avalonia application. |
| `IAvaloniaContext.ApplicationLifetime` | Avalonia desktop lifetime. |
| `IAvaloniaContext.Dispatcher` | Avalonia dispatcher. |
| `IAvaloniaService.Initialize(Application)` | Initializes a registered service on the Avalonia UI thread during desktop startup. |
| `IAvaloniaShell : IInputElement` | Shell contract; normal implementations derive from `Window`. |
| `AvaloniaThread(IServiceProvider, AppBuilder)` | Public platform adapter constructed automatically by `ConfigureAvalonia`. |
| `AvaloniaHostedService(ILogger<AvaloniaHostedService>, AvaloniaThread, IAvaloniaContext)` | Hosted lifetime service constructed automatically by DI. |
| `AvaloniaHostedService.StartAsync` / `StopAsync` | Runs classic desktop lifetime and dispatcher-invokes lifetime shutdown. |

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

On Windows, call `RunAsync` from an STA entry thread; `AvaloniaHostedService.StartAsync`
throws `InvalidOperationException` when the entry thread is not STA. One registered
shell becomes `MainWindow`; multiple shells are shown in registration order. Zero
shells is valid when the application creates its own windows.

## MAUI Hosting

Package: `CP.Extensions.Hosting.Maui`

Namespace: `ReactiveMarbles.Extensions.Hosting.Maui`

### API

| API | Description |
| --- | --- |
| `ConfigureMaui()` / `ConfigureMaui(Action<IMauiBuilder>? configureDelegate)` | Creates/configures `MauiAppBuilder` and registers MAUI context, startup, and hosted services. It does not eagerly build a `MauiApp`. Available on both builder styles. |
| `UseMauiLifetime()` | Links host shutdown to the MAUI lifetime. |
| `ConfigureMauiShell<TShell>()` | Registers a singleton shell page where `TShell : Page, IMauiShell`. |
| `IMauiBuilder.AddSingletonPage<TPage>()` | Registers a MAUI `Page` as a singleton. If it implements `IMauiShell`, it is also registered as the shell. |
| `IMauiBuilder.UseMauiApp<TApplication>()` / `(Action<MauiAppBuilder>? configureMauiApp)` | Registers a MAUI application type and optionally configures the underlying builder. |
| `IMauiBuilder.UseMauiApp<TApplication>(TApplication currentApplication)` / `(TApplication currentApplication, Action<MauiAppBuilder>? configureMauiApp)` | Registers an existing MAUI application and optionally configures the underlying builder. |
| `IMauiBuilder.ConfigureContext(Action<IMauiContext> configureAction)` | Customizes the MAUI context. |
| `IMauiBuilder.ApplicationType` / `Application` | Configured application type and optional existing application instance. |
| `IMauiBuilder.PageTypes` | Ordered list of singleton pages to register. |
| `IMauiBuilder.ConfigureContextAction` | Context callback recorded by the fluent builder. |
| `IMauiBuilder.MauiAppBuilder` | Underlying MAUI builder for fonts, handlers, services, and app configuration. |
| `IMauiContext.MauiApplication` | Current MAUI application. |
| `IMauiContext.Dispatcher` | MAUI dispatcher. |
| `IMauiService.Initialize(Application)` | Initializes a registered service on the MAUI UI thread after application creation. |
| `IMauiShell : IView` | Shell contract; normal implementations derive from `Page` or `Shell`. |
| `MauiThread(IServiceProvider)` | Public platform adapter constructed automatically by `ConfigureMaui`. |
| `MauiHostedService(ILogger<MauiHostedService>, MauiThread, IMauiContext)` | Hosted lifetime service constructed automatically by DI. |
| `MauiHostedService.StartAsync` / `StopAsync` | Starts MAUI through the configured dispatcher and dispatches `Application.Quit()` during host shutdown. |

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

MAUI application construction is deferred to UI startup. Configure `IMauiBuilder`
before `Build` and do not expect an eagerly built `MauiApp` immediately after
`ConfigureMaui`. A pre-cancelled start is skipped. For a running app, shutdown throws
`InvalidOperationException` when the dispatcher is absent or rejects the quit
callback, and honors cancellation before and while awaiting that callback.

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
| `MapSplatLocator(Action<IServiceProvider?> containerFactory)` on `IHost` | Maps Splat to the built host service provider, invokes the callback, and returns the same host (or `null` for a null receiver). Call after `Build`. |

Each platform package initializes the matching ReactiveUI builder extension:

| Package | ReactiveUI builder call |
| --- | --- |
| WPF | `WithWpf()` |
| WinForms | `WithWinForms()` |
| WinUI | `WithWinUI()` |
| Avalonia | `WithAvalonia()` |
| MAUI | `WithMaui()` |

For hosted WPF, `ConfigureSplatForMicrosoftDependencyResolver()` also registers an
internal `IWpfService` that binds `RxSchedulers.MainThreadScheduler` to the dispatcher
owned by the hosted application. Do not add a `SchedulerRegistrar`, and do not use
removed `RxApp` scheduler workarounds. The same public configuration call selects the
appropriate ReactiveUI platform initialization for the other four UI frameworks.

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

The modern Avalonia form is identical at the call site:

```csharp
using ReactiveMarbles.Extensions.Hosting.Avalonia;
using ReactiveMarbles.Extensions.Hosting.ReactiveUI;

var builder = Host.CreateApplicationBuilder(args);
builder.ConfigureSplatForMicrosoftDependencyResolver();
builder.ConfigureAvalonia(avalonia => avalonia
    .UseApplication<App>()
    .UseWindow<MainWindow>());
builder.UseAvaloniaLifetime();

using var host = builder.Build();
host.MapSplatLocator(_ => { });
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
| `ResourceMutex.Create(ILogger? logger, string? mutexId)` | Creates and locks a local named mutex whose resource name is the mutex id. |
| `ResourceMutex.Create(ILogger? logger, string? mutexId, string? resourceName)` | Creates and locks a local mutex with a diagnostic resource name. |
| `ResourceMutex.Create(ILogger? logger, string? mutexId, bool global)` | Creates and locks a local or global mutex. |
| `ResourceMutex.Create(ILogger? logger, string? mutexId, string? resourceName, bool global)` | Full explicit factory overload. |
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

The factory attempts acquisition immediately, waits up to two seconds, and `Lock()` can
be called again when the instance does not own the mutex. A null logger uses internal
no-op logging. A null,
empty, or whitespace `mutexId` throws `ArgumentException`. Global mutexes require the
process permissions needed for the `Global\` namespace. Dispose on the thread that
owns the mutex; disposal coordinates release with that owner thread.

## Plug-ins

Packages:

- `CP.Extensions.Hosting.Plugins`
- `CP.Extensions.Hosting.Plugins.Reactive`

Normal namespace: `ReactiveMarbles.Extensions.Hosting.Plugins`

Reactive namespace: `ReactiveMarbles.Extensions.Hosting.Reactive.Plugins`

### API

| API | Description |
| --- | --- |
| `ConfigurePlugins(Action<IPluginBuilder?> configurePlugin)` | Configures plug-in scanning and loading. Available on both builder styles; the callback itself may be null at runtime and receives the persisted builder instance. |
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
| `RequirePlugins()` / `RequirePlugins(bool failIfNone)` | Requires at least one discovered plug-in or explicitly sets `FailIfNoPlugins`. |
| `PluginScanner.ByNamingConvention(Assembly pluginAssembly)` | Instantiates the public `{AssemblyName}.Plugin` type when it implements `IPlugin`. |
| `PluginScanner.ScanForPluginInstances(Assembly pluginAssembly)` | Instantiates exported, public, concrete classes implementing `IPlugin`. |
| `PluginOrderAttribute()` / `(int order)` / `(object enumValue)` | Orders discovered plug-ins. Lower values run earlier; discovery order is preserved within an equal-order bucket. |
| `PluginOrderAttribute.Order` / `EnumValue` | Integer order and, when the object constructor was used, its original enum/object value. |
| `PluginBase<T>`, `PluginBase<T1,T2>`, `PluginBase<T1,T2,T3>` | Base plug-ins that register one, two, or three types; every generic parameter must implement `IHostedService`. |
| `HostedServiceBase<T>(ILogger<T>, IHostApplicationLifetime)` | Base hosted service with logging, cleanup, and lifetime callback hooks. |

`HostedServiceBase<T>` exposes:

| Member | Description |
| --- | --- |
| `CleanUp` | Disposable collection cleaned when the service stops or is disposed. |
| `Logger` | Logger passed to the base constructor. |
| `IsDisposed` | Indicates whether the cleanup collection has been disposed. |
| `OnStarted()` | Override to start work after the application starts. |
| `OnStopping()` | Override to stop work and dispose resources. Call the base implementation unless you intentionally replace cleanup behavior. |
| `OnStopped()` | Override to run final stopped logic. |
| `StartAsync(CancellationToken)` | Registers callbacks for `ApplicationStarted`, `ApplicationStopping`, and `ApplicationStopped`. |
| `StopAsync(CancellationToken)` | Completes immediately; shutdown work is driven by lifetime callbacks. |
| `Dispose()` | Disposes the cleanup collection and suppresses finalization. |

`OnStarted()` is invoked once asynchronously after `ApplicationStarted`. Its exception
is logged and is not retried or propagated through the lifetime callback. Implement
bounded retry and cancellation explicitly when the service requires them. The normal
package uses `ReactiveUI.Primitives.Disposables.CompositeDisposable`; the `.Reactive`
package exposes the same hosting lifecycle and API shape in its reactive namespace.

The following public loader compatibility APIs live in the `.Plugins.Internals`
namespace. Prefer the higher-level builder and scanner APIs unless implementing a
custom loader:

| API | Description |
| --- | --- |
| `AssemblyDependencyResolver(string pluginPath)` | Creates a resolver rooted beside the supplied plug-in assembly path. |
| `ResolveAssemblyToPath(AssemblyName)` | Returns an existing managed assembly path or `null`. |
| `ResolveUnmanagedDllToPath(string)` | Returns an existing native DLL path or `null`; blank names throw `ArgumentException`. |
| `AssemblyLoadContext(string name)` | Creates the package's cross-target load-context abstraction. |
| `AssemblyLoadContext.Default` / `Assemblies` / `Name` | Default context, currently loaded application-domain assemblies, and context name. |
| `AssemblyLoadContext.LoadFromAssemblyPath(string)` | Static managed assembly load by full path. |
| `AssemblyLoadContext.LoadFromAssemblyName(AssemblyName)` | Instance managed assembly load by identity. |
| `AssemblyLoadContext.Load(AssemblyName)` / `LoadUnmanagedDll(string)` | Protected virtual resolution hooks for derived contexts. |
| `AssemblyLoadContext.LoadUnmanagedDllFromPath(string)` | Protected static native-load hook. |
| `TryGetAssembly(AssemblyName, out Assembly?)` | Searches loaded assemblies by simple name. |

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
| `ServiceHost.Create(Type type, string[] args)` | Creates and runs a classic service host with the default plug-in namespace/runtime rules. |
| `ServiceHost.Create(Type type, string[] args, Func<IHostBuilder?,IHostBuilder?>? hostBuilder, Action<IHost>? configureHost, string nameSpace, string? targetRuntime)` | Full classic factory. The six-argument overload has no optional arguments. |
| `ServiceHost.CreateApplication(Type type, string[] args)` | Creates and runs a modern application-builder service host with defaults. |
| `ServiceHost.CreateApplication(Type type, string[] args, Func<IHostApplicationBuilder?,IHostApplicationBuilder?>? hostBuilder, Action<IHost>? configureHost, string nameSpace, string? targetRuntime)` | Full modern factory. The six-argument overload has no optional arguments. |
| `UseServiceBaseLifetime()` on `IHostBuilder` | Registers `ServiceBaseLifetime` as `IHostLifetime`. |
| `UseServiceBaseLifetime()` on `IHostApplicationBuilder` | Registers `ServiceBaseLifetime` as `IHostLifetime`. |
| `UseConsoleLifetime()` on `IHostApplicationBuilder` | Registers `ConsoleLifetime` as `IHostLifetime`. |
| `RunAsServiceAsync()` / `RunAsServiceAsync(CancellationToken)` on `IHostBuilder` | Builds and runs with `ServiceBaseLifetime`. |
| `RunAsServiceAsync()` / `RunAsServiceAsync(CancellationToken)` on `HostApplicationBuilder` | Configures service lifetime, builds, and runs the modern builder. |
| `ServiceBaseLifetime(IHostApplicationLifetime)` | `IHostLifetime` backed by `System.ServiceProcess.ServiceBase`. |
| `ServiceBaseLifetime.WaitForStartAsync(CancellationToken)` | Starts the Windows service dispatcher on a background task and completes when the service reports start. |
| `ServiceBaseLifetime.StopAsync(CancellationToken)` | Stops the service when the lifetime is active. |
| `DefaultLogger.Logger` | Exposes the logger used by `ServiceHost` after DI construction. |

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
    nameSpace: "Contoso.Plugin",
    targetRuntime: "win-x64")
    .ConfigureAwait(false);
```

The full factory overload deliberately requires all six arguments. Use the
two-argument overload when every default is acceptable; do not rely on the V3
three-to-five-argument optional-parameter call forms.

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

Across both providers, `TContext : DbContext`; Identity user and role type parameters
are constrained to `class`. The helpers register the requested DbContext provider,
Identity stores, and the selected service lifetime.

### SQL Server API

| API | Description |
| --- | --- |
| `IConfiguration.HasConnectionString(string connectionStringName)` | Returns `true` when the named connection string exists and is not empty. |
| `IConfiguration.GetRequiredConnectionString(string connectionStringName)` | Gets the named connection string or throws a descriptive `InvalidOperationException`. |
| `IHostApplicationBuilder.AddSqlServerDbContext<TContext>(string connectionStringName)` / `(string connectionStringName, ServiceLifetime serviceLifetime)` | Registers a SQL Server `DbContext`; the short overload uses `Scoped`. |
| `IHostApplicationBuilder.AddSqlServerWithIdentity<TContext,TUser,TRole>(string connectionStringName)` / `(..., ServiceLifetime serviceLifetime)` | Registers SQL Server EF Core plus Identity with roles; the short overload uses `Scoped`. |
| `IHostApplicationBuilder.AddSqlServerWithIdentity<TContext,TUser>(string connectionStringName)` / `(..., ServiceLifetime serviceLifetime)` | Registers SQL Server EF Core plus Identity without a custom role type. |
| `IServiceCollection.UseEntityFrameworkCoreSqlServer<TContext,TUser,TRole>(WebHostBuilderContext context, string connectionStringName)` / `(..., ServiceLifetime serviceLifetime)` | Registers SQL Server EF Core plus Identity with roles in a web-host callback. |
| `IServiceCollection.UseEntityFrameworkCoreSqlServer<TContext,TUser>(WebHostBuilderContext context, string connectionStringName)` / `(..., ServiceLifetime serviceLifetime)` | Registers SQL Server EF Core plus Identity without a custom role type. |
| `IServiceCollection.AddSqlServerDbContext<TContext>(IConfiguration configuration, string connectionStringName)` / `(..., ServiceLifetime serviceLifetime)` | Registers a SQL Server `DbContext` from configuration. |
| `IServiceCollection.AddSqlServerDbContextWithConnectionString<TContext>(string connectionString)` / `(..., ServiceLifetime serviceLifetime)` | Registers a SQL Server `DbContext` from a direct connection string. |

### SQLite API

| API | Description |
| --- | --- |
| `IHostApplicationBuilder.AddSqliteDbContext<TContext>(string connectionStringName)` / `(string connectionStringName, ServiceLifetime serviceLifetime)` | Registers a SQLite `DbContext`; the short overload uses `Scoped`. |
| `IHostApplicationBuilder.AddSqliteWithIdentity<TContext,TUser,TRole>(string connectionStringName)` / `(..., ServiceLifetime serviceLifetime)` | Registers SQLite EF Core plus Identity with roles. |
| `IHostApplicationBuilder.AddSqliteWithIdentity<TContext,TUser>(string connectionStringName)` / `(..., ServiceLifetime serviceLifetime)` | Registers SQLite EF Core plus Identity without a custom role type. |
| `IServiceCollection.UseEntityFrameworkCoreSqlite<TContext,TUser,TRole>(WebHostBuilderContext context, string connectionStringName)` / `(..., ServiceLifetime serviceLifetime)` | Registers SQLite EF Core plus Identity with roles in a web-host callback. |
| `IServiceCollection.UseEntityFrameworkCoreSqlite<TContext,TUser>(WebHostBuilderContext context, string connectionStringName)` / `(..., ServiceLifetime serviceLifetime)` | Registers SQLite EF Core plus Identity without a custom role type. |
| `IServiceCollection.AddSqliteDbContext<TContext>(IConfiguration configuration, string connectionStringName)` / `(..., ServiceLifetime serviceLifetime)` | Registers a SQLite `DbContext` from configuration. |
| `IServiceCollection.AddSqliteDbContextWithConnectionString<TContext>(string connectionString)` / `(..., ServiceLifetime serviceLifetime)` | Registers a SQLite `DbContext` from a direct connection string. |
| `CreateInMemoryConnectionString()` / `CreateInMemoryConnectionString(string? databaseName)` | Creates a named or unnamed SQLite in-memory connection string. |
| `CreateFileConnectionString(string filePath)` | Creates a SQLite file connection string. |

Both provider packages expose six `IHostBuilder.UseWebHostServices` overloads:

| Overload shape | Behavior |
| --- | --- |
| `UseWebHostServices(Action<WebHostBuilderContext,IServiceCollection> configureServices)` | Configures web-host services with default scope validation. |
| `UseWebHostServices(Action<...> configureServices, bool validateScopes)` | Also controls service-provider scope validation. |
| `UseWebHostServices(Func<IWebHostBuilder,IWebHostBuilder> configureWebHost, Action<...> configureServices)` | Customizes the web host and services. |
| Previous overload plus `bool validateScopes` | Adds explicit scope validation. |
| Previous overload plus `Func<IApplicationBuilder,IApplicationBuilder> configureApp` | Also customizes the terminal application pipeline. |
| Previous overload plus `bool validateScopes` | Full web-host, services, pipeline, and validation overload. |

When no `configureApp` delegate is supplied, the helper installs a terminal no-op
pipeline. Use the explicit `ServiceLifetime` overload when a context lifetime other
than the default `Scoped` registration is intentional.

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

## Log4Net Logging

Package: `CP.Extensions.Logging.Log4Net`

Main namespace: `ReactiveMarbles.Extensions.Logging`

Entity namespace: `ReactiveMarbles.Extensions.Logging.Log4Net.Entities`

Extension namespace: `ReactiveMarbles.Extensions.Logging.Log4Net.Extensions`

### API

| API | Description |
| --- | --- |
| `ILoggerFactory.AddLog4Net()` | Adds a provider using `log4net.config` without file watching. |
| `ILoggerFactory.AddLog4Net(string configFile)` | Adds a provider using the named configuration file. |
| `ILoggerFactory.AddLog4Net(string configFile, bool watch)` | Adds a provider and optionally reloads when the XML file changes. |
| `ILoggerFactory.AddLog4Net(Log4NetProviderOptions options)` | Adds a directly constructed provider with advanced options. |
| `ILoggingBuilder.AddLog4Net()` / `(string)` / `(string,bool)` / `(Log4NetProviderOptions)` | DI registration equivalents for Generic Host logging configuration. |
| `Log4NetProvider()` / `(string configFile)` / `(Log4NetProviderOptions options)` | Creates the concrete `ILoggerProvider`. |
| `Log4NetProvider.CreateLogger()` / `CreateLogger(string categoryName)` | Creates a default-category or named logger. |
| `ILoggerProvider.CreateLogger(Type nameType)` | Creates a type-named logger; throws `ArgumentOutOfRangeException` for a provider other than `Log4NetProvider`. |
| `Log4NetProvider.ExternalScopeProvider` / `SetScopeProvider(...)` | Gets or replaces the Microsoft logging scope provider. |
| `Log4NetProvider.Dispose()` | Disposes cached loggers/provider resources. |
| `Log4NetLogger.Name` / `BeginScope` / `IsEnabled` / `Log` | Concrete Microsoft `ILogger` implementation used by the provider. |
| `ILog.Critical(object, Exception)` / `Trace(object, Exception)` | Adds explicit log4net Critical and Trace calls. |
| `ILog4NetLoggingEventFactory.CreateLoggingEvent<TState>(...)` | Customizes conversion from Microsoft log state to a log4net `LoggingEvent`. |
| `ILog4NetLogLevelTranslator.TranslateLogLevel(...)` | Customizes Microsoft-to-log4net level mapping. |
| `Log4NetLoggingEventFactory` / `Log4NetLogLevelTranslator` | Default implementations of the two customization interfaces. |
| `MessageCandidate<TState>` | Immutable input record containing `LogLevel`, `EventId`, state, exception, and formatter for event creation. |
| `NodeInfo` | XML override descriptor with `XPath`, `NodeContent`, and a mutable get-only `Attributes` dictionary. |

`Log4NetProviderOptions` supports:

| Option | Meaning |
| --- | --- |
| `Name` | Optional provider/configuration name. |
| `Log4NetConfigFileName` | XML configuration path; defaults to `log4net.config`. |
| `LoggerRepository` | Explicit log4net repository name. |
| `OverrideCriticalLevelWith` | Maps Microsoft `Critical` to a configured log4net level name. |
| `PropertyOverrides` | Mutable get-only list of `NodeInfo` XML overrides. |
| `Watch` | Reloads the configuration file after changes. |
| `ExternalConfigurationSetup` | Leaves repository configuration to application code and skips provider configuration. |
| `UseWebOrAppConfig` | Reads log4net configuration from `web.config` or `app.config`. |
| `ConfigurationAssembly` | Assembly used to establish repository identity when no repository name is supplied. |
| `LoggingEventFactory` | Optional `ILog4NetLoggingEventFactory` replacement. |
| `LogLevelTranslator` | Optional `ILog4NetLogLevelTranslator` replacement. |

### Generic Host Example

```csharp
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReactiveMarbles.Extensions.Logging;
using ReactiveMarbles.Extensions.Logging.Log4Net.Entities;

var builder = Host.CreateApplicationBuilder(args);

var log4NetOptions = new Log4NetProviderOptions("log4net.config", watch: true)
{
    Name = "Desktop",
    OverrideCriticalLevelWith = "FATAL",
};

var appenderOverride = new NodeInfo
{
    XPath = "/log4net/appender[@name='RollingFile']/file",
};
appenderOverride.Attributes["value"] = "logs/application.log";
log4NetOptions.PropertyOverrides.Add(appenderOverride);

builder.Logging.AddLog4Net(log4NetOptions);

using var host = builder.Build();
await host.RunAsync().ConfigureAwait(false);
```

If log4net is configured before the Generic Host starts, set
`ExternalConfigurationSetup = true` so the provider preserves that repository.
`PropertyOverrides` and `NodeInfo.Attributes` are get-only mutable collections: use
`Add` or index assignment instead of replacing the collection.

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

## Migrate From V3.1.26 To V4.0.0

V4 is a major-version migration. Upgrade all `CP.Extensions.Hosting.*` references
together and rebuild every consuming assembly, including plug-ins. Source calls that
still look the same may bind to different explicit overloads, so copying V3 binaries
into a V4 process is unsupported.

### Migration Checklist

1. Move the application to a supported target framework and install the .NET workloads
   required by its UI platform.
2. Upgrade all Extensions.Hosting packages to `4.0.0`.
3. Choose either the normal or `.Reactive` package family for plug-ins,
   PluginService, and ReactiveUI bridges.
4. Update reactive namespaces when choosing `.Reactive`.
5. Replace V3 `System.Reactive` disposable contracts used through
   `HostedServiceBase<T>`.
6. Rebuild custom shell implementations against the new platform-typed interfaces.
7. Update `ResourceMutex` and `ServiceHost` calls to explicit V4 overloads.
8. Remove application `SchedulerRegistrar`/`RxApp` workarounds.
9. Exercise native startup, dispatcher shutdown, cancellation, plug-in discovery, and
   service/console mode.

### Package And Namespace Selection

V4 default packages use the normal ReactiveUI.Primitives family. V4 adds explicit
`.Reactive` packages for applications that use the reactive bridge family:

| V3.1.26 package | V4 default | V4 `.Reactive` alternative |
| --- | --- | --- |
| `CP.Extensions.Hosting.Plugins` | Same package | `CP.Extensions.Hosting.Plugins.Reactive` |
| `CP.Extensions.Hosting.PluginService` | Same package | `CP.Extensions.Hosting.PluginService.Reactive` |
| `CP.Extensions.Hosting.ReactiveUI.Wpf` | Same package | `CP.Extensions.Hosting.ReactiveUI.Wpf.Reactive` |
| `CP.Extensions.Hosting.ReactiveUI.WinForms` | Same package | `CP.Extensions.Hosting.ReactiveUI.WinForms.Reactive` |
| `CP.Extensions.Hosting.ReactiveUI.WinUI` | Same package | `CP.Extensions.Hosting.ReactiveUI.WinUI.Reactive` |
| `CP.Extensions.Hosting.ReactiveUI.Avalonia` | Same package | `CP.Extensions.Hosting.ReactiveUI.Avalonia.Reactive` |
| `CP.Extensions.Hosting.ReactiveUI.Maui` | Same package | `CP.Extensions.Hosting.ReactiveUI.Maui.Reactive` |

```xml
<!-- V3.1.26 or the V4 default/Primitives path -->
<PackageReference Include="CP.Extensions.Hosting.Plugins" Version="4.0.0" />

<!-- V4 explicit reactive bridge path -->
<PackageReference Include="CP.Extensions.Hosting.Plugins.Reactive" Version="4.0.0" />
```

```csharp
// V4 default packages
using ReactiveMarbles.Extensions.Hosting.Plugins;
using ReactiveMarbles.Extensions.Hosting.PluginService;
using ReactiveMarbles.Extensions.Hosting.ReactiveUI;

// V4 .Reactive packages
using ReactiveMarbles.Extensions.Hosting.Reactive.Plugins;
using ReactiveMarbles.Extensions.Hosting.Reactive.PluginService;
using ReactiveMarbles.Extensions.Hosting.Reactive.ReactiveUI;
```

Do not mix a normal package and its `.Reactive` sibling in the same project.

### HostedServiceBase Disposable And Startup Changes

`HostedServiceBase<T>` no longer implements `System.Reactive.ICancelable`. It implements
`ReactiveUI.Primitives.Disposables.IsDisposed`, remains `IDisposable`, and exposes a
Primitives `CompositeDisposable` through `CleanUp`.

```csharp
// V3
System.Reactive.ICancelable service = hostedService;
if (!service.IsDisposed)
{
    service.Dispose();
}

// V4
ReactiveUI.Primitives.Disposables.IsDisposed service = hostedService;
if (!service.IsDisposed)
{
    ((IDisposable)hostedService).Dispose();
}
```

`OnStarted()` is now invoked once asynchronously after `ApplicationStarted`.
Exceptions are logged and are neither retried nor propagated through the lifetime
callback. Replace any reliance on the previous Rx `Retry()` path with an explicit,
bounded retry policy that honors application cancellation.

### Platform-Typed Shell Contracts

The shell interfaces are no longer unconstrained markers:

| V4 shell | Required base interface |
| --- | --- |
| `IWpfShell` | `System.Windows.IInputElement` |
| `IAvaloniaShell` | `Avalonia.Input.IInputElement` |
| `IWinFormsShell` | `System.ComponentModel.IComponent` |
| `IMauiShell` | `Microsoft.Maui.IView` |

Normal `Window`, `Form`, `Page`, and `Shell` implementations already satisfy these
requirements. Custom marker-only implementations must now implement the platform
interface or, preferably, compose the shell contract into a real platform view.

### Explicit Overload Changes

V3 exposed optional arguments for `ResourceMutex.Create`; V4 exposes four methods:

```csharp
// V3
using var mutex = ResourceMutex.Create(
    logger,
    "Contoso.Desktop",
    resourceName: "Desktop application",
    global: true);

// V4: select one explicit overload
using var mutex = ResourceMutex.Create(
    logger,
    "Contoso.Desktop",
    "Desktop application",
    global: true);
```

V3 allowed partial `ServiceHost.Create` and `CreateApplication` calls through optional
arguments. V4 supports either two arguments or all six:

```csharp
// V3.1.26
await ServiceHost.Create(
    typeof(Program),
    args,
    hostBuilder: ConfigureHostBuilder);

// V4 default path
await ServiceHost.Create(typeof(Program), args);

// V4 configured path
await ServiceHost.Create(
    typeof(Program),
    args,
    ConfigureHostBuilder,
    configureHost: null,
    nameSpace: "Contoso.Plugin",
    targetRuntime: null);
```

`RunAsServiceAsync(CancellationToken cancellationToken = default)` was likewise split
into parameterless and `CancellationToken` overloads. Normal parameterless call syntax
is unchanged.

V4 implementation source uses C# extension blocks and explicit no-argument/full
overload pairs throughout the hosting APIs. Consumer syntax remains
`builder.ConfigureWpf(...)`, `builder.ConfigurePlugins(...)`, and so on, but
reflection-based code must not assume V3 optional-parameter metadata.

### Scheduler And UI Lifecycle Changes

- WPF `ConfigureSplatForMicrosoftDependencyResolver()` now installs hosted dispatcher
  scheduling through an internal `IWpfService`. Remove `SchedulerRegistrar` and old
  `RxApp` scheduler setup; use `RxSchedulers`.
- MAUI application construction is deferred to UI startup. Do not resolve an eagerly
  built `MauiApp` immediately after `ConfigureMaui`.
- MAUI and WinUI shutdown now fails fast instead of waiting indefinitely when the
  dispatcher is absent or rejects the callback. Handle `InvalidOperationException`,
  and expect `OperationCanceledException` when shutdown is cancelled.
- WinForms shutdown snapshots open forms before closing and disposing them. One form's
  cleanup failure is logged and does not prevent the remaining forms from being
  processed.
- WPF and Avalonia lifetime methods must follow their `Configure*` methods and throw
  `NotSupportedException` when that order is violated.

### Plug-in API Changes

Plug-ins with the same `PluginOrderAttribute.Order` retain discovery order within that
order bucket. The attribute lookup is cached per plug-in type.
`PluginOrderAttribute(object enumValue)` now preserves the supplied value through
`EnumValue` as well as exposing its converted integer `Order`.

Types under `ReactiveMarbles.Extensions.Hosting.Plugins.Internals` are public for
historical compatibility but remain implementation APIs. V4 changes include:

- `AssemblyDependencyResolver` is sealed and
  `ResolveUnmanagedDllToPath` returns `string?`.
- `AssemblyLoadContext.Assemblies`, `LoadFromAssemblyPath`, and
  `LoadUnmanagedDllFromPath` are static.
- `PluginLoadContext` is sealed and its former public `ResolveAssemblyPath` is
  internal.

Migrate callers to `ConfigurePlugins`, `IPluginBuilder`, and `PluginScanner` instead of
binding to those loader implementation types.

### Log4Net Changes

`CP.Extensions.Logging.Log4Net` no longer targets `netstandard2.0`. Move consumers to
.NET Framework 4.6.2 or later, or .NET 8 or later.

```csharp
// V3
ILogger logger = provider.CreateLogger<MyCategory>();

// V4
ILogger logger = provider.CreateLogger(typeof(MyCategory));
```

`Log4NetProviderOptions.PropertyOverrides` and `NodeInfo.Attributes` are get-only
mutable collections:

```csharp
// V3 assignment
options.PropertyOverrides = new List<NodeInfo> { node };
node.Attributes = new Dictionary<string, string> { ["value"] = "app.log" };

// V4 mutation
options.PropertyOverrides.Add(node);
node.Attributes["value"] = "app.log";
```

When `ExternalConfigurationSetup` is `true`, V4 preserves the externally configured
repository and skips provider configuration.

### Target Framework And Dependency Changes

- Most packages add `net48` and `net11.0`/`net11.0-windows`; net11 support is preview.
- The base MAUI host retains `net9.0`, but V4 MAUI platform workload assets begin at
  `net10.0-*`. ReactiveUI MAUI V4 begins at net10.
- Log4Net removes `netstandard2.0`.
- ReactiveUI moves from 23.2.27 to 24.0.0, Splat from 19.3.1 to 20.2.0, Avalonia from
  12.0.2 to 12.1.0, and the default packages use ReactiveUI.Primitives 7.1.
- EF helper call sites remain conceptually compatible. Short overloads use
  `ServiceLifetime.Scoped`; use the new explicit-lifetime overloads when a different
  lifetime is required.

Consumers do not need to enable C# preview merely because V4 implementation assemblies
use preview language features; they need only a supported SDK and target framework.

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
