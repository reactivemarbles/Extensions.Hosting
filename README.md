NOTE: The namespacing has been changed to ReactiveMarbles.Extensions.Hosting. Please update your references to the new namespace.

# ReactiveMarbles.Extensions.Hosting
An Extension of the Microsoft.Extensions.Hosting library with the aim of allowing windows applications to use the hosting base. 

#### ReactiveMarbles.Extensions.Hosting.Identity.EntityFrameworkCore.Sqlite
![Nuget](https://img.shields.io/nuget/v/CP.Extensions.Hosting.Identity.EntityFrameworkCore.Sqlite) ![Nuget](https://img.shields.io/nuget/dt/CP.Extensions.Hosting.Identity.EntityFrameworkCore.Sqlite)

```C#
.UseWebHostServices((whb, services) =>
{
    services.UseEntityFrameworkCoreSqlite<DBContext, IdentityUser, IdentityRole>(whb, "DefaultConnection")
    .Configure<IdentityOptions>(options =>
    {
        // Configure options
    });
})
```

#### ReactiveMarbles.Extensions.Hosting.Identity.EntityFrameworkCore.SqlServer
![Nuget](https://img.shields.io/nuget/v/CP.Extensions.Hosting.Identity.EntityFrameworkCore.SqlServer) ![Nuget](https://img.shields.io/nuget/dt/CP.Extensions.Hosting.Identity.EntityFrameworkCore.SqlServer)

```C#
.UseWebHostServices((whb, services) =>
{
    services.UseEntityFrameworkCoreSqlServer<DBContext, IdentityUser, IdentityRole>(whb, "DefaultConnection")
    .Configure<IdentityOptions>(options =>
    {
        // Configure options
    });
})
```

#### ReactiveMarbles.Extensions.Hosting.MainUIThread
![Nuget](https://img.shields.io/nuget/v/CP.Extensions.Hosting.MainUIThread) ![Nuget](https://img.shields.io/nuget/dt/CP.Extensions.Hosting.MainUIThread)

Used to run the main UI thread in a Wpf / WinUI / WinForms application.


#### ReactiveMarbles.Extensions.Hosting.Plugins
![Nuget](https://img.shields.io/nuget/v/CP.Extensions.Hosting.Plugins) ![Nuget](https://img.shields.io/nuget/dt/CP.Extensions.Hosting.Plugins)

```C#
.ConfigurePlugins(pluginBuilder =>
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("Running using dotNet {0}", Environment.Version);

    //// Specify the location from where the Dll's are "globbed"
    var process = Process.GetCurrentProcess();
    var fullPath = process.MainModule?.FileName?.Replace(process.MainModule.ModuleName!, string.Empty);
    Console.WriteLine("Add Scan Directories: {0}", fullPath);
    pluginBuilder?.AddScanDirectories(fullPath!);

    //// Add the framework libraries which can be found with the specified globs
    pluginBuilder?.IncludeFrameworks(@"\netstandard2.0\*.FrameworkLib.dll");

    //// Add the plugins which can be found with the specified globs
    var runtime = targetRuntime ?? Path.GetFileName(executableLocation);
    Console.WriteLine(@"Include Plugins from: \Plugins\{0}\{1}*.dll", runtime, nameSpace);
    pluginBuilder?.IncludePlugins(@$"\Plugins\{runtime}\{##YourPluginNameSpace##}*.dll");
    Console.ResetColor();
})
```

```C#
/// <summary>
/// This plug-in configures the HostBuilderContext to have the hosted services
/// </summary>
public class Plugin : PluginBase<FirstService, SecondService, ThirdService>
{
}
```

#### ReactiveMarbles.Extensions.Hosting.PluginService
![Nuget](https://img.shields.io/nuget/v/CP.Extensions.Hosting.PluginService) ![Nuget](https://img.shields.io/nuget/dt/CP.Extensions.Hosting.PluginService)

```C#
await ServiceHost.Create(
            typeof(Program),
            args,
            hb => hb, // Configure the HostBuilder
            host => {}, // Configure the Host
            nameSpace: "ReactiveMarbles.Plugin").ConfigureAwait(false);
```

#### ReactiveMarbles.Extensions.Hosting.ReactiveUI.WinForms
![Nuget](https://img.shields.io/nuget/v/CP.Extensions.Hosting.ReactiveUI.WinForms) ![Nuget](https://img.shields.io/nuget/dt/CP.Extensions.Hosting.ReactiveUI.WinForms)

```C#
.ConfigureSplatForMicrosoftDependencyResolver()
.ConfigureWinForms<MainForm>()
.UseWinFormsLifetime()
```

#### ReactiveMarbles.Extensions.Hosting.ReactiveUI.WinUI
![Nuget](https://img.shields.io/nuget/v/CP.Extensions.Hosting.ReactiveUI.WinUI) ![Nuget](https://img.shields.io/nuget/dt/CP.Extensions.Hosting.ReactiveUI.WinUI)

```C#
.ConfigureSplatForMicrosoftDependencyResolver()
.ConfigureWinUI<MainWindow>()
.UseWpfLifetime()
```

#### ReactiveMarbles.Extensions.Hosting.ReactiveUI.Wpf
![Nuget](https://img.shields.io/nuget/v/CP.Extensions.Hosting.ReactiveUI.Wpf) ![Nuget](https://img.shields.io/nuget/dt/CP.Extensions.Hosting.ReactiveUI.Wpf)

```C#
.ConfigureSplatForMicrosoftDependencyResolver()
.ConfigureWpf<MainWindow>()
.UseWpfLifetime()
```

#### ReactiveMarbles.Extensions.Hosting.SingleInstance
![Nuget](https://img.shields.io/nuget/v/CP.Extensions.Hosting.SingleInstance) ![Nuget](https://img.shields.io/nuget/dt/CP.Extensions.Hosting.SingleInstance)

```C#
.ConfigureSingleInstance(builder =>
{
	builder.MutexId = "{ea031523-3a63-45e5-85f2-6fa75fbf37ed}";
	builder.WhenNotFirstInstance = (hostingEnvironment, logger) =>
	{
		// Application already started, this is another instance
		logger.LogWarning("Application {0} already running.", hostingEnvironment.ApplicationName);
	};
})
```

#### ReactiveMarbles.Extensions.Hosting.WinForms
![Nuget](https://img.shields.io/nuget/v/CP.Extensions.Hosting.WinForms) ![Nuget](https://img.shields.io/nuget/dt/CP.Extensions.Hosting.WinForms)

```C#
.ConfigureWinForms<MainForm>()
.UseWinFormsLifetime()
```

#### ReactiveMarbles.Extensions.Hosting.WinUI
![Nuget](https://img.shields.io/nuget/v/CP.Extensions.Hosting.WinUI) ![Nuget](https://img.shields.io/nuget/dt/CP.Extensions.Hosting.WinUI)

```C#
.ConfigureWinUI<MainWindow>()
.UseWpfLifetime()
```

#### ReactiveMarbles.Extensions.Hosting.Wpf
![Nuget](https://img.shields.io/nuget/v/CP.Extensions.Hosting.Wpf) ![Nuget](https://img.shields.io/nuget/dt/CP.Extensions.Hosting.Wpf)

```C#
.ConfigureWpf<MainWindow>()
.UseWpfLifetime()
```
