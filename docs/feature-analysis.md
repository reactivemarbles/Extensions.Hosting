# Package feature analysis

The solution already covers desktop host lifetimes for WPF, WinForms, WinUI,
Avalonia, and MAUI; ReactiveUI/Splat integration; single-instance enforcement;
plug-in discovery and Windows service hosting; EF Core/Identity registration; and
Log4Net integration. Existing tests exercise platform startup, shutdown, multiple
shells, discovery ordering, external assembly loading, and normal/Reactive variants.

The following additions address concrete gaps in those existing responsibilities.

| Gap | Addition | Design and compatibility |
| --- | --- | --- |
| Built-in plug-ins must use assembly discovery or manually invoke `ConfigureHost`. | `ConfigurePlugin(IPlugin)` on both host builders, in normal and Reactive packages. | Uses the supplied instance without reflection or loading assemblies. Preserves each builder's configuration timing and context. Explicit calls run in registration order; the caller owns the instance. No changes to `IPlugin` or `IPluginBuilder`. |
| WPF offers application types and existing instances, but lacks the application factories available in MAUI. | `ConfigureWpfApplication<TApplication>(Func<IServiceProvider, TApplication>)` on both host builders. | Composes existing WPF hosting registration with a deferred singleton factory. Concrete/base service aliases share the instance. No changes to `IWpfBuilder`, platform threading, or shutdown. |
| Log4Net formats message state but only enriches properties from scopes, losing queryable message-template fields. | Preserve structured message state in event properties. | Copies typed key/value state, including the original template. Message fields override ambient scope fields; `eventId` remains authoritative. Existing message and scope formatting remain intact. |

Factories and explicit plug-ins support applications that construct their own
dependencies. They avoid constructor discovery in these registration paths, but do
not make unrelated WPF, plug-in implementation, or third-party code AOT compatible.

Validation covers plug-in context, timing, order, repeated calls, argument failures,
and both dependency families; deferred WPF factory invocation and service aliases;
and typed logging fields reaching a real in-memory Log4Net appender, including
scope collisions, nulls, duplicates, and event identity. Tests use TUnit assertions.
The README contains public API entries, usage examples, and ownership/threading
requirements for each addition.

## Verification results

| Check | Result |
| --- | --- |
| Core TUnit suite (.NET 10 and .NET 11) | 222 passed per framework; 444 executions total. |
| Data/logging TUnit suite (.NET 10) | 41 passed. |
| WPF builder TUnit suite (.NET 10 and .NET 11 Windows) | 10 passed across both frameworks. |
| Release builds with warnings as errors | Plugins, Plugins.Reactive, Log4Net, WPF, ReactiveUI.Wpf, and ReactiveUI.Wpf.Reactive passed across their declared target frameworks. |
| MTP coverage inspection | New plugin and WPF factory methods and the changed Log4Net event factory have 100% line and branch coverage in the focused reports. This is not a solution-wide coverage claim. |
| Full solution restore/build | Blocked by the existing `Tmds.DBus.Protocol` 0.95.2 pin: configured feeds report 0.95.1 as the nearest available version (NU1102). The pin is unchanged pending the user's dependency choice. |

Tests ran with builds enabled. No warning suppressions were added.
