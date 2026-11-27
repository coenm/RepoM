# Demo mode

Start the normal RepoM UI with sample repositories:

```powershell
dotnet run --project src\RepoM.App -- --startup-plugin "src\RepoM.App\bin\Debug\net10.0-windows\RepoM.StartupPlugin.Demo.dll"
```

The separate worktree prototype is selected with `RepoM.StartupPlugin.WorktreePreview.dll`
instead.
For a published build, pass the startup DLL next to `RepoM.exe`.

## Behavior

- The demo opens the real main window, repository cards and YAML context menus.
- It stays open when it loses focus, has a taskbar entry, and closes normally. It does not register the tray icon or global hotkey.
- Five fixed examples cover clean and dirty repositories, ahead/behind counts, missing upstream and a missing repository.
- Searching, expanding cards, favorites and monitoring toggles use the normal UI. Favorites are kept in memory.
- Scanning restores the examples, including after clearing the repository list.
- Repository commands are explicitly logged as simulated: no Git command, terminal or editor is launched.
- Other plugins are not loaded in demo mode, so their modules and template integrations cannot start background work.

The profile lives at `%LOCALAPPDATA%\RepoM\Demo`, independently of the normal profile and command-line profile overrides. Its sample action menu, sorting and filtering YAML files are reset on every demo startup. Appearance and quick-filter settings remain within this separate profile.

This is an isolated data mode, not an OS security sandbox. Ordinary application links (help, project website, and opening the settings directory) still work normally.

## Implementation

All fixtures, profile setup and demo decorators live in [RepoM.Plugin.Demo](../src/RepoM.Plugin.Demo).

The host loads an `IStartupPlugin` from the supplied DLL without referencing demo types.
The startup plugin decorates the path-provider factory and plugin finder in a separate
startup container. The factory prepares the isolated profile; the finder selects only
the demo application package. That package is required even when a profile contains
old disabled-plugin settings, so demo startup cannot accidentally start production services.
The application package registers its decorators after the host's application defaults.
The startup DLLs use the `RepoM.StartupPlugin.*` prefix, so normal `RepoM.Plugin.*`
discovery does not select them.

The plugin decorates:

- `IRepositoryMonitorService`: provides in-memory data for both UI refreshes and module lifecycle. The production monitor factory is never called.
- `ICommandExecutor<T>`: simulates commands without constructing or calling the original executors.
- `IFavoriteStore`: seeds a favorite and retains changes in memory.
- `IApplicationPresentation`: selects standalone-window behavior.

The normal repository store, monitoring-state service, view models, controls and action-menu renderer are reused.

## Host and startup contract

- `RepoM.App` remains the executable (`RepoM.exe`) and owns both containers,
  logging, plugin loading, argument handling and application lifetime.
- `RepoM.UI` is the WPF library: windows, resources, view models and UI services.
  It does not reference the host. The host activates its `IApplicationWindow`.
- `RepoM.Core.StartupPlugin` contains `IStartupPlugin` and
  `IConfigBasedAppDataPathProviderFactory`.
- Core first registers `IFileSystem`, `IHmacService`, `IPluginFinder` and the
  path-provider factory in the startup container. An optional startup plugin then
  replaces or decorates those registrations through
  `Task RegisterStartupDependenciesAsync(Container container)`.
- Startup plugins must not resolve services during registration. After the awaited
  registration completes, the host verifies the startup container and resolves the
  final instances. The application container receives those same instances through
  `RegisterInstance`; it does not own their disposal.
- The path factory's `CreateAsync` allows profile preparation before logging and
  normal application registrations. Ordinary configuration arguments still reach
  the default factory; the startup-plugin argument is consumed by the host.
- Shutdown stops modules, disposes the application container, then disposes the
  startup container. Containers own the singleton services they construct; plugins
  supplying externally created instances remain responsible for their lifetime.

A startup DLL must export exactly one public, concrete `IStartupPlugin` with a
public parameterless constructor. Relative paths resolve against the current working
directory. Both `--startup-plugin <path>` and `--startup-plugin=<path>` are supported;
duplicates, missing paths, incompatible assemblies and registration failures are
reported as startup errors, without falling back to normal mode.

Startup plugins execute trusted .NET code with the application's permissions; only
load plugins you trust. The loader deliberately does not use `IPluginFinder`, since
that is one of the services a startup plugin can replace.

## Tests

```powershell
dotnet test tests\RepoM.App.Tests\RepoM.App.Tests.csproj --filter "FullyQualifiedName~StartupTests|FullyQualifiedName~DemoPackageTests|FullyQualifiedName~BootstrapperTests|FullyQualifiedName~ConfigBasedAppDataPathProviderFactoryTest"
```

The tests cover lifecycle/refresh isolation, clearing and reloading the data, cancelled scans, command suppression, in-memory favorites, plugin discovery and the real YAML/Scriban menu pipeline including deferred branch menus.

On Windows, run `dotnet test tests\RepoM.StartupPlugin.Demo.Tests\RepoM.StartupPlugin.Demo.Tests.csproj`
to generate and verify the WPF screenshots and demo GIFs. The repository-filter GIF
draws its own 1920×1080 desktop. The simulated `Ctrl + Alt + R` shortcut appears
before RepoM opens in the bottom-right above the taskbar, followed by overlays for
the click and typed keys. No wallpaper file or desktop recording is needed.
Tests that open interactive WPF windows take turns across test processes because they share
the foreground window and mouse capture.
