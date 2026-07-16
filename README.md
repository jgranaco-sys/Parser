# Generic Payload Transformation Engine

A runnable .NET 8 payload transformation engine for SCADA-oriented ingress and egress processing.

## Architecture overview

The solution uses a small Clean Architecture inspired split:

- **Parser.Abstractions**: public contracts, records, profiles, SCADA interface, plugin contract.
- **Parser.Core**: ingress/egress pipelines, mapping, transformation, validation, template rendering, plugin loading, in-memory SCADA provider, metrics, bounded-channel processor.
- **Parser.Plugins.BuiltIn**: built-in JSON/CSV/TXT parsers and default engine registrations.
- **Parser.Storage.Sqlite**: runtime SQLite-backed profile/template repository with sample seeding and JSON import/export.
- **Parser.ProtocolAdapters.Mqtt**: thin MQTT adapter entry point.
- **Parser.Hosting**: DI/bootstrap and runnable demo orchestration.
- **Parser.SamplePlugin**: sample external plugin assembly copied into `plugins/` at build output.
- **Parser.App**: runnable console host.
- **tests/**: unit, integration, and performance smoke tests.

## Key capabilities

- Async ingress pipeline: parse -> map -> transform -> validate -> bulk SCADA write.
- Async egress pipeline: discover required variables -> bulk read -> render template -> return UTF-8 payload.
- Built-in JSON/CSV/TXT support.
- Wildcard-capable mapping patterns with capture substitution via `{0}` placeholders.
- Built-in transformations:
  - type conversion
  - engineering conversion (`F_TO_C`, `KV_TO_V`, `MW_TO_W`, `KW_TO_W`)
  - arithmetic expressions like `IncomingValue * 1000`
  - enumeration mapping
- Validation rules:
  - required fields
  - type checks
  - numeric ranges
  - timestamp validation window
- SQLite runtime configuration storage.
- Plugin discovery from `plugins/` folder using `IPluginModule`.
- MQTT adapter kept thin and optional.
- In-memory SCADA provider for local execution and tests.

## Run instructions

```bash
dotnet build /home/runner/work/Parser/Parser/Parser.slnx
dotnet run --project /home/runner/work/Parser/Parser/src/Parser.App/Parser.App.csproj
```

The console host seeds SQLite data, processes sample JSON/CSV/TXT messages, and renders JSON/CSV/TXT output templates.

## Test instructions

```bash
dotnet test /home/runner/work/Parser/Parser/Parser.slnx
```

## Sample ingress mappings

Seeded JSON mapping:

- `StationA.Voltage -> BUS1_V`
- `StationA.Breaker -> BRK1_ST`
- `StationA.Temperature -> TEMP1`

Sample CSV uses the same logical field names as headers.

Sample TXT uses key/value lines:

```text
StationA.Voltage=139.4
StationA.Breaker=TRIP
StationA.Temperature=26.2
```

## Sample templates

### JSON

```json
{
  "Substation": {
    "Voltage": "${BUS1_V:F2}",
    "BreakerCode": "${BRK1_ST}",
    "Temperature": "${TEMP1:F1}"
  }
}
```

### CSV

```text
Voltage,BreakerCode,Temperature
${BUS1_V:F2},${BRK1_ST},${TEMP1:F1}
```

### TXT

```text
Voltage=${BUS1_V:F2}
BreakerAlarm=${BRK1_ST=="1" ? 1 : 0}
Temperature=${TEMP1:F1}
```

## Configuration storage

SQLite is used at runtime because it gives:

- transactional updates
- simple deployment
- version/timestamp metadata per profile/template
- import/export via JSON documents

## Plugin development guide

Create a class library that references `Parser.Abstractions` and implements:

```csharp
public sealed class MyPluginModule : IPluginModule
{
    public void Register(IServiceCollection services)
    {
        services.AddSingleton<IParserStrategy, MyParser>();
    }
}
```

Drop the compiled assembly into the host `plugins/` folder. The host scans `*.dll` files and registers all discovered `IPluginModule` implementations at startup.

The included sample plugin (`Parser.SamplePlugin`) adds a `kvp-semi` parser for payloads like:

```text
StationA.Voltage=138.2;StationA.Breaker=OPEN;StationA.Temperature=25.3
```

## Extension points

The current plugin contract supports adding future:

- XML parser plugins
- YAML parser plugins
- Binary parser plugins
- SparkplugB parser plugins

without changing the core engine.
