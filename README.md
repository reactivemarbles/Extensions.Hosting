NOTE: The namespacing has been changed to ReactiveMarbles.Extensions.Hosting. Please update your references to the new namespace.

# ReactiveMarbles.Extensions.Hosting
Extensions for Microsoft.Extensions.Hosting that bring WPF, WinForms, WinUI, ReactiveUI, plug-ins, single-instance control, and common host utilities to desktop apps.

This repository supports both classic IHostBuilder and the newer IHostApplicationBuilder hosting model introduced in .NET 8+. Existing IHostBuilder APIs remain unchanged; equivalent IHostApplicationBuilder overloads are available where appropriate.

Supported targets include .NET Framework 4.6.2/4.8, .NET Standard 2.0, and .NET 8/9 (Windows where applicable).

## Quick start

Choose a hosting model:
- IHostBuilder (generic host): Host.CreateDefaultBuilder(args)
- IHostApplicationBuilder (new app builder): Host.CreateApplicationBuilder(args)

### Example: WPF app with IHostBuilder
```csharp
using Microsoft.Extensions.Hosting;
using ReactiveMarbles.Extensions.Hosting.Wpf;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureWpf(wpf =>
    {
        // Optional: register Application type and windows via the WpfBuilder
        wpf.ApplicationType = typeof(App);
        wpf.WindowTypes.Add(typeof(MainWindow));
        wpf.ConfigureContextAction = ctx => ctx.ShutdownMode = ShutdownMode.OnMainWindowClose;
    })
    .UseWpfLifetime(ShutdownMode.OnMainWindowClose)
    .Build();

await host.RunAsync();
```

### Example: WPF app with IHostApplicationBuilder
```csharp
using Microsoft.Extensions.Hosting;
using ReactiveMarbles.Extensions.Hosting.Wpf;

var builder = Host.CreateApplicationBuilder(args)
    .ConfigureWpf(wpf =>
    {
        wpf.ApplicationType = typeof(App);
        wpf.WindowTypes.Add(typeof(MainWindow));
        wpf.ConfigureContextAction = ctx => ctx.ShutdownMode = ShutdownMode.OnMainWindowClose;
    })
    .UseWpfLifetime(ShutdownMode.OnMainWindowClose);

await builder.Build().RunAsync();
```

---

## Packages and APIs
The following sections outline the main features, their APIs for both hosting models, and example usage.

### WPF (CP.Extensions.Hosting.Wpf)
Namespace: ReactiveMarbles.Extensions.Hosting.Wpf
- ConfigureWpf
  - IHostBuilder ConfigureWpf(this IHostBuilder, Action<IWpfBuilder>?)
  - IHostApplicationBuilder ConfigureWpf(this IHostApplicationBuilder, Action<IWpfBuilder>?)
  - Use IWpfBuilder to set:
    - Type? ApplicationType
    - Application? Application (optional existing instance)
    - IList<Type> WindowTypes
    - Action<IWpfContext>? ConfigureContextAction
- UseWpfLifetime
  - IHostBuilder UseWpfLifetime(this IHostBuilder, ShutdownMode = OnLastWindowClose)
  - IHostApplicationBuilder UseWpfLifetime(this IHostApplicationBuilder, ShutdownMode = OnLastWindowClose)
- IWpfContext
  - ShutdownMode ShutdownMode { get; set; }
  - Application? WpfApplication { get; set; }
  - Dispatcher Dispatcher { get; }
  - bool IsLifetimeLinked { get; set; } (set internally when using UseWpfLifetime)

Example (builder model): see Quick start above.

### WinForms (CP.Extensions.Hosting.WinForms)
Namespace: ReactiveMarbles.Extensions.Hosting.WinForms
- ConfigureWinForms
  - IHostBuilder ConfigureWinForms(this IHostBuilder, Action<IWinFormsContext>?)
  - IHostApplicationBuilder ConfigureWinForms(this IHostApplicationBuilder, Action<IWinFormsContext>?)
- ConfigureWinForms<TView>() where TView : Form
  - IHostBuilder ConfigureWinForms<TView>(...)
  - IHostApplicationBuilder ConfigureWinForms<TView>(...)
  - Registers the main form and, if it implements IWinFormsShell, also registers it as IWinFormsShell
- ConfigureWinFormsShell<TShell>() where TShell : Form, IWinFormsShell
  - IHostBuilder ConfigureWinFormsShell<TShell>()
  - IHostApplicationBuilder ConfigureWinFormsShell<TShell>()
- UseWinFormsLifetime
  - IHostBuilder UseWinFormsLifetime(this IHostBuilder)
  - IHostApplicationBuilder UseWinFormsLifetime(this IHostApplicationBuilder)
- IWinFormsContext
  - bool EnableVisualStyles { get; set; }
  - Dispatcher? Dispatcher { get; set; } (WinForms dispatcher abstraction)

Example (application builder):
```csharp
using Microsoft.Extensions.Hosting;
using ReactiveMarbles.Extensions.Hosting.WinForms;

var builder = Host.CreateApplicationBuilder(args)
    .ConfigureWinForms(ctx =>
    {
        ctx.EnableVisualStyles = true;
    })
    .ConfigureWinFormsShell<MainForm>()
    .UseWinFormsLifetime();

await builder.Build().RunAsync();
```

### WinUI (CP.Extensions.Hosting.WinUI)
Namespace: ReactiveMarbles.Extensions.Hosting.WinUI
- ConfigureWinUI<TApp, TAppWindow>() where TApp : Microsoft.UI.Xaml.Application where TAppWindow : Microsoft.UI.Xaml.Window
  - IHostBuilder ConfigureWinUI<TApp, TAppWindow>()
  - IHostApplicationBuilder ConfigureWinUI<TApp, TAppWindow>()
- IWinUIContext
  - Window? AppWindow { get; set; }
  - Type? AppWindowType { get; set; }
  - DispatcherQueue? Dispatcher { get; set; }
  - Application? WinUIApplication { get; set; }

Example (application builder):
```csharp
using Microsoft.Extensions.Hosting;
using ReactiveMarbles.Extensions.Hosting.WinUI;

var builder = Host.CreateApplicationBuilder(args)
    .ConfigureWinUI<App, MainWindow>();

await builder.Build().RunAsync();
```

### ReactiveUI integration
Namespaces: ReactiveMarbles.Extensions.Hosting.ReactiveUI (per UI stack)
- ConfigureSplatForMicrosoftDependencyResolver
  - IHostBuilder ConfigureSplatForMicrosoftDependencyResolver(this IHostBuilder)
  - IHostApplicationBuilder ConfigureSplatForMicrosoftDependencyResolver(this IHostApplicationBuilder)
- Variants:
  - WPF: Adds WithWpf() builder stage
  - WinForms: Adds WithWinForms() builder stage
  - WinUI: Adds WithWinUI() builder stage
- MapSplatLocator
  - IHost MapSplatLocator(this IHost host, Action<IServiceProvider?> containerFactory)

Example (WPF + ReactiveUI with application builder):
```csharp
using Microsoft.Extensions.Hosting;
using ReactiveMarbles.Extensions.Hosting.ReactiveUI;
using ReactiveMarbles.Extensions.Hosting.Wpf;

var builder = Host.CreateApplicationBuilder(args)
    .ConfigureSplatForMicrosoftDependencyResolver()
    .ConfigureWpf(wpf =>
    {
        wpf.ApplicationType = typeof(App);
        wpf.WindowTypes.Add(typeof(MainWindow));
    })
    .UseWpfLifetime();

await builder.Build().RunAsync();
```

### Plug-in system (CP.Extensions.Hosting.Plugins)
Namespace: ReactiveMarbles.Extensions.Hosting.Plugins
- ConfigurePlugins
  - IHostBuilder ConfigurePlugins(this IHostBuilder, Action<IPluginBuilder?> configure)
  - IHostApplicationBuilder ConfigurePlugins(this IHostApplicationBuilder, Action<IPluginBuilder?> configure)
- IPluginBuilder key options (typical):
  - UseContentRoot (bool): also scan content root
  - IncludeFrameworks(params string[] globs)
  - IncludePlugins(params string[] globs)
  - AddScanDirectories(params string[] directories)
  - PluginMatcher / FrameworkMatcher (advanced globbing)
  - AssemblyScanFunc: Func<Assembly, IEnumerable<IPlugin?>> for discovery

Example (application builder):
```csharp
using Microsoft.Extensions.Hosting;
using ReactiveMarbles.Extensions.Hosting.Plugins;

var builder = Host.CreateApplicationBuilder(args)
    .ConfigurePlugins(plugins =>
    {
        plugins.UseContentRoot = true;
        // Add framework assemblies and plugin patterns
        plugins.IncludeFrameworks(@"\\netstandard2.0\\*.FrameworkLib.dll");
        plugins.IncludePlugins(@"\\Plugins\\{runtime}\\ReactiveMarbles.Plugin.*.dll");
    });

await builder.Build().RunAsync();
```

### Single instance (CP.Extensions.Hosting.SingleInstance)
Namespace: ReactiveMarbles.Extensions.Hosting.AppServices
- ConfigureSingleInstance
  - IHostBuilder ConfigureSingleInstance(this IHostBuilder, Action<IMutexBuilder> configure)
  - IHostApplicationBuilder ConfigureSingleInstance(this IHostApplicationBuilder, Action<IMutexBuilder> configure)
  - IHostBuilder ConfigureSingleInstance(this IHostBuilder, string mutexId)
  - IHostApplicationBuilder ConfigureSingleInstance(this IHostApplicationBuilder, string mutexId)
- IMutexBuilder
  - string MutexId { get; set; }
  - Action<IHostEnvironment, ILogger>? WhenNotFirstInstance { get; set; }

Example (application builder):
```csharp
using Microsoft.Extensions.Hosting;
using ReactiveMarbles.Extensions.Hosting.AppServices;

var builder = Host.CreateApplicationBuilder(args)
    .ConfigureSingleInstance(cfg =>
    {
        cfg.MutexId = "{ea031523-3a63-45e5-85f2-6fa75fbf37ed}";
        cfg.WhenNotFirstInstance = (env, logger) =>
            logger.LogWarning("Application {0} already running.", env.ApplicationName);
    });

await builder.Build().RunAsync();
```

### Service host utilities (CP.Extensions.Hosting.PluginService)
Namespace: ReactiveMarbles.Extensions.Hosting.PluginService
- UseServiceBaseLifetime
  - IHostBuilder UseServiceBaseLifetime(this IHostBuilder)
  - IHostApplicationBuilder UseServiceBaseLifetime(this IHostApplicationBuilder)
- UseConsoleLifetime (IHostApplicationBuilder only)
  - IHostApplicationBuilder UseConsoleLifetime(this IHostApplicationBuilder)
- RunAsServiceAsync
  - Task RunAsServiceAsync(this IHostBuilder, CancellationToken = default)
  - Task RunAsServiceAsync(this HostApplicationBuilder, CancellationToken = default)
- ServiceHost
  - Task Create(Type type, string[] args, Func<IHostBuilder?, IHostBuilder?>? configureHostBuilder = null, Action<IHost>? configureHost = null, string nameSpace = "ReactiveMarbles.Plugin", string? targetRuntime = null)
  - Task CreateApplication(Type type, string[] args, Func<IHostApplicationBuilder?, IHostApplicationBuilder?>? configureHostBuilder = null, Action<IHost>? configureHost = null, string nameSpace = "ReactiveMarbles.Plugin", string? targetRuntime = null)

Example (service/console dual mode using IHostApplicationBuilder):
```csharp
using Microsoft.Extensions.Hosting;
using ReactiveMarbles.Extensions.Hosting.PluginService;

var builder = Host.CreateApplicationBuilder(args)
    .UseContentRoot(Directory.GetCurrentDirectory())
    .ConfigureLogging()
    .ConfigureConfiguration(args)
    .UseConsoleLifetime(); // or .UseServiceBaseLifetime() for Windows Service

await builder.Build().RunAsync();
```

Example (helper):
```csharp
using ReactiveMarbles.Extensions.Hosting.PluginService;

await ServiceHost.CreateApplication(
    typeof(Program),
    args,
    hb => hb // external builder customization
            .ConfigurePlugins(pb => { /* plugin globs */ }),
    host => { /* use host.Services */ },
    nameSpace: "ReactiveMarbles.Plugin");
```

### Identity + Entity Framework Core
Namespaces:
- ReactiveMarbles.Extensions.Hosting.Identity.EntityFrameworkCore (SqlServer)
- ReactiveMarbles.Extensions.Hosting.Identity.EntityFrameworkCore.Sqlite (Sqlite)

APIs (service collection extensions used inside UseWebHostServices):
- IServiceCollection UseEntityFrameworkCoreSqlServer<TContext, TUser, TRole>(WebHostBuilderContext, string connectionStringName, ServiceLifetime = Scoped)
- IServiceCollection UseEntityFrameworkCoreSqlServer<TContext, TUser>(WebHostBuilderContext, string connectionStringName, ServiceLifetime = Scoped)
- IServiceCollection UseEntityFrameworkCoreSqlite<TContext, TUser, TRole>(WebHostBuilderContext, string connectionStringName, ServiceLifetime = Scoped)
- IServiceCollection UseEntityFrameworkCoreSqlite<TContext, TUser>(WebHostBuilderContext, string connectionStringName, ServiceLifetime = Scoped)

Host/web host wiring:
- IHostBuilder UseWebHostServices(this IHostBuilder, Action<WebHostBuilderContext, IServiceCollection> configureServices, bool validateScopes = false)
- IHostBuilder UseWebHostServices(this IHostBuilder, Action<WebHostBuilderContext, IServiceCollection> configureServices, Func<IWebHostBuilder, IWebHostBuilder> configureWebHost, bool validateScopes = false)
- IHostBuilder UseWebHostServices(this IHostBuilder, Action<WebHostBuilderContext, IServiceCollection> configureServices, Func<IWebHostBuilder, IWebHostBuilder> configureWebHost, Func<IApplicationBuilder, IApplicationBuilder> configureApp, bool validateScopes = false)

Example:
```csharp
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactiveMarbles.Extensions.Hosting.Identity.EntityFrameworkCore;

var host = Host.CreateDefaultBuilder(args)
    .UseWebHostServices((whb, services) =>
    {
        services.UseEntityFrameworkCoreSqlServer<AppDbContext, IdentityUser, IdentityRole>(whb, "DefaultConnection");
    })
    .Build();

await host.RunAsync();
```

### Reactive examples
- WPF + ReactiveUI + Single instance:
```csharp
var builder = Host.CreateApplicationBuilder(args)
    .ConfigureSplatForMicrosoftDependencyResolver()
    .ConfigureWpf(wpf =>
    {
        wpf.ApplicationType = typeof(App);
        wpf.WindowTypes.Add(typeof(MainWindow));
    })
    .UseWpfLifetime()
    .ConfigureSingleInstance("{ea031523-3a63-45e5-85f2-6fa75fbf37ed}");

await builder.Build().RunAsync();
```

### Notes
- WPF/WinForms/WinUI components target Windows only.
- When using IHostApplicationBuilder, prefer chaining extension methods that return IHostApplicationBuilder so you can call Build() on the final HostApplicationBuilder instance.
- Plugin scanning uses glob patterns. Ensure your plugin folders are copied to output and the patterns match your runtime folder (e.g., Plugins\net9.0-windows\Your.Plugin.*.dll).

---

## Legacy snippets (IHostBuilder only)

#### ReactiveMarbles.Extensions.Hosting.Plugins
```csharp
.ConfigurePlugins(pluginBuilder =>
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("Running using dotNet {0}", Environment.Version);

    var process = Process.GetCurrentProcess();
    var fullPath = process.MainModule?.FileName?.Replace(process.MainModule.ModuleName!, string.Empty);
    Console.WriteLine("Add Scan Directories: {0}", fullPath);
    pluginBuilder?.AddScanDirectories(fullPath!);

    pluginBuilder?.IncludeFrameworks(@"\netstandard2.0\*.FrameworkLib.dll");

    var runtime = Path.GetFileName(AppContext.BaseDirectory);
    Console.WriteLine(@"Include Plugins from: \Plugins\{0}\{1}*.dll", runtime, "ReactiveMarbles.Plugin");
    pluginBuilder?.IncludePlugins(@$"\Plugins\{runtime}\{{YourPluginNamespace}}*.dll");
    Console.ResetColor();
})
```

#### ReactiveMarbles.Extensions.Hosting.PluginService
```csharp
await ServiceHost.Create(
    typeof(Program),
    args,
    hb => hb, // Configure the HostBuilder
    host => {}, // Configure the Host
    nameSpace: "ReactiveMarbles.Plugin").ConfigureAwait(false);
