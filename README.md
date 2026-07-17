# Parser — Payload Transformation Studio

A .NET 8 / Avalonia UI desktop application for building, testing, and managing
payload-transformation profiles: configuring input formats (JSON/CSV/TXT),
mapping incoming fields to SCADA variables, applying transformation and
validation rules, authoring output templates, and running end-to-end tests —
all backed by a pluggable application-service layer.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (version pinned via `global.json`, currently `8.0.422`)
- Linux, macOS, or Windows (Avalonia is cross-platform)

## Solution layout

```
Parser.slnx                    Solution file (XML "slnx" format)
src/
  Parser.Core/                 Domain models + application-service interfaces (no UI deps)
  Parser.Ui.Infrastructure/     In-memory implementations of the application services + seed data
  Parser.Ui.Core/              MVVM view models + navigation service (CommunityToolkit.Mvvm, no Avalonia deps)
  Parser.Ui.Controls/          Reusable Avalonia UserControls (grids, pickers, editors)
  Parser.Ui.App/               Avalonia desktop shell: Program.cs, App/MainWindow, Views, Themes
tests/
  Parser.Ui.Tests/              xUnit tests for the view models
```

### Architecture

- **Parser.Core** defines the domain models (`ParserProfile`, `FieldMapping`,
  `TransformationRule`, `ValidationRule`, `OutputTemplate`, `PluginInfo`,
  diagnostics/log entries, configuration POCOs) and the application-service
  contracts (`IProfileAppService`, `IMappingAppService`, `ITemplateAppService`,
  `ITestRunAppService`, `IDiagnosticsAppService`, `ITransformationAppService`,
  `IValidationAppService`, `IPluginAppService`, `ISettingsAppService`).
- **Parser.Ui.Infrastructure** provides thread-safe in-memory implementations
  of every application service, plus a `SeedDataService` that pre-populates
  three sample profiles (Weather Provider, Energy Meter, DER Gateway) with
  representative field mappings. `InfrastructureExtensions.AddParserInfrastructure`
  wires everything into `IServiceCollection` and seeds data before the services
  are exposed, so the same instances are shared across the app.
- **Parser.Ui.Core** contains the MVVM layer: `ViewModelBase`, `INavigationService`
  / `NavigationService`, and one view model per screen (Dashboard, Input
  Configuration, Mapping Editor, Transformations, Validation, Template Editor,
  Output Configuration, Testing, Diagnostics, Logs, Profiles, Plugin Manager,
  Settings), plus `MainWindowViewModel` for the shell (navigation, breadcrumbs,
  profile selector, theme toggle). This project has **no Avalonia dependency**
  so it can be unit tested quickly and reused by other front-ends.
- **Parser.Ui.Controls** holds small reusable Avalonia `UserControl`s
  (`MappingGridControl`, `VariablePickerControl`, `DiagnosticsPanelControl`,
  `TemplateEditorControl`, `PayloadViewerControl`) that can be composed into
  richer views as the UI evolves.
- **Parser.Ui.App** is the Avalonia Desktop entry point. `App.axaml.cs` builds
  a `ServiceCollection`, registers infrastructure + core services, and resolves
  `MainWindowViewModel` for the main window. `MainWindow.axaml` hosts the menu,
  breadcrumb/profile header, left navigation list, content area (driven by
  per-view-model `DataTemplate`s), and a status bar. Each screen has its own
  `View` under `Views/`.

### Extension points

- **New input formats**: add an options POCO under `Parser.Core.Models`
  (mirroring `JsonParserOptions`/`CsvParserOptions`/`TxtParserOptions`), extend
  `InputConfiguration`, and update `InputConfigViewModel`/`InputConfigView`.
- **New application services**: define the contract in `Parser.Core.Interfaces`,
  implement it in `Parser.Ui.Infrastructure.Services` (or swap in a real
  persistence-backed implementation), and register it in
  `InfrastructureExtensions.AddParserInfrastructure`.
- **New screens**: add a view model to `Parser.Ui.Core.ViewModels`, register it
  in `CoreExtensions.AddParserUiCore`, add a matching `View` under
  `Parser.Ui.App/Views`, register it in `MainWindow.axaml`'s `Window.DataTemplates`,
  and add an entry to `MainWindowViewModel.NavigationItems`.
- **Plugins**: `IPluginAppService`/`PluginInfo` model plugin metadata
  (enabled/installed/update-available); a real implementation could scan a
  plugins directory and load assemblies dynamically.

## Building and running

```bash
# Restore & build every project
dotnet build src/Parser.Core/Parser.Core.csproj
dotnet build src/Parser.Ui.Infrastructure/Parser.Ui.Infrastructure.csproj
dotnet build src/Parser.Ui.Core/Parser.Ui.Core.csproj
dotnet build src/Parser.Ui.Controls/Parser.Ui.Controls.csproj
dotnet build src/Parser.Ui.App/Parser.Ui.App.csproj
dotnet build tests/Parser.Ui.Tests/Parser.Ui.Tests.csproj

# Run the desktop app
dotnet run --project src/Parser.Ui.App/Parser.Ui.App.csproj

# Run the tests
dotnet test tests/Parser.Ui.Tests/Parser.Ui.Tests.csproj
```

> **Note on `Parser.slnx`**: the repository includes a solution file in the
> newer XML `.slnx` format. Building/testing directly against `Parser.slnx`
> (e.g. `dotnet build Parser.slnx`) requires **.NET SDK 9.0.100+** (with
> `DOTNET_ENABLE_SOLUTION_SNLX=1` set, since `.slnx` CLI support is preview
> there) or a recent Visual Studio (17.10+). The `.NET 8` SDK pinned by
> `global.json` in this repository does not parse `.slnx` files, so use the
> per-project `dotnet build`/`dotnet test` commands above (or open
> `Parser.slnx` in a compatible IDE) until the toolchain is upgraded.

## Keyboard shortcuts

| Shortcut         | Action                          |
|------------------|---------------------------------|
| `Ctrl+N`         | New profile (planned)           |
| `Ctrl+S`         | Save current screen (planned)   |
| `Ctrl+Tab`       | Cycle navigation items (planned)|
| `F5`             | Run test payload (Testing view) |

> Shortcuts are defined as extension points for future `KeyBinding` wiring in
> `MainWindow.axaml`; the current build focuses on menu/button driven actions.
