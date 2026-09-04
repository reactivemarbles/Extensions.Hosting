---
name: extensions-hosting
description: Configure, compose, troubleshoot, and migrate CP.Extensions.Hosting packages for Generic Host applications. Use for WPF, WinForms, WinUI, Avalonia, MAUI, ReactiveUI/Splat, single-instance enforcement, plug-ins, Windows service hosting, normal versus .Reactive package selection, and V3.1.26-to-V4.0.0 upgrades.
---

# Extensions.Hosting

Use this skill when a project references a `CP.Extensions.Hosting.*` package or needs a
Microsoft Generic Host around a desktop UI, plug-in system, or Windows service.

Read the package-root `README.md` before changing code. It contains the complete API
tables, platform recipes, exceptions, and migration guide. Treat the installed package
version and the project's target framework as authoritative.

## Select packages

Choose one platform host when the application owns a UI:

| Application | Host package | Optional ReactiveUI/Splat bridge |
| --- | --- | --- |
| WPF | `CP.Extensions.Hosting.Wpf` | `CP.Extensions.Hosting.ReactiveUI.Wpf` |
| WinForms | `CP.Extensions.Hosting.WinForms` | `CP.Extensions.Hosting.ReactiveUI.WinForms` |
| WinUI | `CP.Extensions.Hosting.WinUI` | `CP.Extensions.Hosting.ReactiveUI.WinUI` |
| Avalonia | `CP.Extensions.Hosting.Avalonia` | `CP.Extensions.Hosting.ReactiveUI.Avalonia` |
| MAUI | `CP.Extensions.Hosting.Maui` | `CP.Extensions.Hosting.ReactiveUI.Maui` |

Add supporting packages only for features that are used:

- `CP.Extensions.Hosting.SingleInstance` for mutex-backed single-instance enforcement.
- `CP.Extensions.Hosting.Plugins` for plug-in discovery and hosted-service plug-ins.
- `CP.Extensions.Hosting.PluginService` for console or Windows service plug-in hosts.
- `CP.Extensions.Hosting.Identity.EntityFrameworkCore.SqlServer` or `.Sqlite` for
  database and Identity registration.

Do not reference `CP.Extensions.Hosting.MainUIThread` directly unless implementing a
new platform adapter.

## Choose the normal or `.Reactive` family

Use normal packages with the normal ReactiveUI and ReactiveUI.Primitives family.
Select the `.Reactive` variant for plug-in, PluginService, or ReactiveUI bridge
features when that project and its plug-ins use the
`ReactiveUI.Primitives.*.Reactive` or `ReactiveUI.*.Reactive` dependency family. A UI
bridge is not required for `Plugins.Reactive` or `PluginService.Reactive`.

| Normal | `.Reactive` |
| --- | --- |
| `CP.Extensions.Hosting.Plugins` | `CP.Extensions.Hosting.Plugins.Reactive` |
| `CP.Extensions.Hosting.PluginService` | `CP.Extensions.Hosting.PluginService.Reactive` |
| `CP.Extensions.Hosting.ReactiveUI.<Platform>` | `CP.Extensions.Hosting.ReactiveUI.<Platform>.Reactive` |
| `ReactiveMarbles.Extensions.Hosting.Plugins` | `ReactiveMarbles.Extensions.Hosting.Reactive.Plugins` |
| `ReactiveMarbles.Extensions.Hosting.PluginService` | `ReactiveMarbles.Extensions.Hosting.Reactive.PluginService` |
| `ReactiveMarbles.Extensions.Hosting.ReactiveUI` | `ReactiveMarbles.Extensions.Hosting.Reactive.ReactiveUI` |

Platform hosts (`CP.Extensions.Hosting.Wpf`, `.WinForms`, `.WinUI`, `.Avalonia`,
`.Maui`), `CP.Extensions.Hosting.SingleInstance`,
`CP.Extensions.Hosting.MainUIThread`, Identity helpers, and Log4Net have no
`.Reactive` siblings. Keep those normal packages even when using a `.Reactive` bridge
or plug-in package. For example, `CP.Extensions.Hosting.ReactiveUI.Wpf.Reactive` still
uses `CP.Extensions.Hosting.Wpf`.

Never mix normal and `.Reactive` variants of the same feature in one project.
Do not add direct Rx.NET references merely to use these hosting APIs.

## Compose the host

Apply configuration before the matching lifetime method, then build once:

```csharp
using Microsoft.Extensions.Hosting;
using ReactiveMarbles.Extensions.Hosting.AppServices;
using ReactiveMarbles.Extensions.Hosting.ReactiveUI;
using ReactiveMarbles.Extensions.Hosting.Wpf;

var builder = Host.CreateApplicationBuilder(args);

builder.ConfigureSingleInstance("Contoso.Desktop");
builder.ConfigureSplatForMicrosoftDependencyResolver();
builder.ConfigureWpf(wpf => wpf
    .UseApplication(typeof(App))
    .UseWindow(typeof(MainWindow)));
builder.UseWpfLifetime();

using var host = builder.Build();
host.MapSplatLocator(_ => { });
await host.RunAsync().ConfigureAwait(false);
```

Ordering rules:

1. Register application services and optional Splat integration.
2. Configure single-instance enforcement before platform-hosted services when used.
3. Call `ConfigureWpf`, `ConfigureWinForms`, `ConfigureWinUI`, `ConfigureAvalonia`, or
   `ConfigureMaui`.
4. Call the matching `Use*Lifetime` method where the platform exposes one.
5. Build the host.
6. Call `MapSplatLocator` only on the built host.
7. Run and dispose the host.

`UseWpfLifetime` and `UseAvaloniaLifetime` throw when called before their matching
`Configure*` method. WinUI installs its hosted lifetime in `ConfigureWinUI`; it has no
`UseWinUILifetime` method.

## Initialize services on the UI thread

Register platform initialization services with DI. The host invokes them on the
platform UI thread:

```csharp
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

Equivalent contracts are `IWinFormsService`, `IWinUIService`, `IAvaloniaService`, and
`IMauiService`.

## ReactiveUI schedulers

Call the platform `ConfigureSplatForMicrosoftDependencyResolver()` extension. For WPF,
V4 automatically binds `RxSchedulers.MainThreadScheduler` to the dispatcher owned by
the hosted application through an internal `IWpfService`.

Do not add or register `SchedulerRegistrar`. Do not use removed `RxApp` scheduler
workarounds.

## Plug-ins

Configure scan roots and explicit include patterns:

```csharp
builder.ConfigurePlugins(plugins =>
{
    plugins?.AddScanDirectories(AppContext.BaseDirectory);
    plugins?.IncludePlugins("plugins/**/*.Plugin.dll");
    plugins?.ExcludePlugins("plugins/**/*.Disabled.dll");
    plugins?.RequirePlugins();
});
```

Implement `IPlugin.ConfigureHost` to register services. Use `PluginOrderAttribute` for
deterministic order and `HostedServiceBase<T>` only when its application-lifetime
callbacks fit the service. `OnStarted()` runs once; implement bounded retry and
cancellation explicitly when needed.

## Single-instance applications

Use `ConfigureSingleInstance` for hosted applications:

```csharp
builder.ConfigureSingleInstance(mutex =>
{
    mutex.MutexId = "Contoso.Desktop";
    mutex.IsGlobal = false;
    mutex.WhenNotFirstInstance = (_, logger) =>
        logger.LogWarning("Another instance is already running.");
});
```

Use one of the four explicit `ResourceMutex.Create` overloads for direct locking. A
blank mutex id is invalid. Dispose the mutex on its owning thread.

## Diagnose lifecycle failures

- Missing UI: verify the application/window/page type is registered and the shell
  implements the platform shell interface.
- Host never exits: enable the platform lifetime after platform configuration.
- WPF/Avalonia lifetime throws during setup: reverse the `Use*Lifetime` and
  `Configure*` calls.
- MAUI/WinUI shutdown throws: the running application must expose a usable dispatcher;
  rejected dispatch is an `InvalidOperationException`, and cancellation is honored.
- Avalonia fails on Windows startup: enter the hosted UI from an STA thread.
- Splat cannot resolve host services: call `MapSplatLocator` after `Build`.
- No plug-ins load: verify scan roots, glob patterns, runtime subdirectory, and
  `ValidatePlugin`.

## Migrate V3.1.26 to V4.0.0

1. Upgrade every `CP.Extensions.Hosting.*` reference together.
2. Rebuild all consumers; V4 changes binary signatures even where source calls remain.
3. Choose the normal or `.Reactive` family and update namespaces as a unit.
4. Replace `System.Reactive.ICancelable` assumptions on `HostedServiceBase<T>` with
   `ReactiveUI.Primitives.Disposables.IsDisposed` and `IDisposable`.
5. Replace optional `ResourceMutex.Create` assumptions with an explicit overload.
6. Replace partial `ServiceHost.Create`/`CreateApplication` calls with either the
   two-argument overload or all six arguments.
7. Make custom WPF, Avalonia, WinForms, and MAUI shells implement their new platform
   base interface.
8. Remove `SchedulerRegistrar`; use hosted WPF scheduler wiring.
9. Retarget MAUI platform-workload applications, and all `ReactiveUI.Maui`
   consumers, to net10 or later. The base `CP.Extensions.Hosting.Maui` host still
   supports net9; its platform workload assets begin at net10. Move Log4Net
   `netstandard2.0` consumers to a supported target.
10. Run a warning-as-error build and exercise startup and shutdown on the native UI.

## Validate

For a consumer project, restore, build, and test the application for every target
platform/runtime, then manually exercise native startup and shutdown.

For a repository checkout:

```powershell
cd src
dotnet workload restore
dotnet restore Extensions.Hosting.slnx
dotnet build Extensions.Hosting.slnx -c Release -warnaserror
dotnet test --solution Extensions.Hosting.slnx -c Release
```

For consumer projects, validate both the target platform and deployment runtime.
MAUI, WinUI, WPF, WinForms, and Windows service behavior cannot be fully established
by a platform-neutral unit test alone.
