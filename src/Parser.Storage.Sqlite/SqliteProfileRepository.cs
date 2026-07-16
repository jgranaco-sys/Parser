namespace Parser.Storage.Sqlite;

/// <summary>
/// Stores runtime profiles in SQLite using JSON documents for portability and versioning.
/// </summary>
public sealed class SqliteProfileRepository(string connectionString, ILogger<SqliteProfileRepository> logger) : IProfileRepository
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public async ValueTask InitializeAsync(CancellationToken ct = default)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(ct);
        var commandText = """
            CREATE TABLE IF NOT EXISTS ingress_profiles (
                name TEXT PRIMARY KEY,
                version INTEGER NOT NULL,
                updated_utc TEXT NOT NULL,
                json TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS transformation_profiles (
                name TEXT PRIMARY KEY,
                version INTEGER NOT NULL,
                updated_utc TEXT NOT NULL,
                json TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS validation_profiles (
                name TEXT PRIMARY KEY,
                version INTEGER NOT NULL,
                updated_utc TEXT NOT NULL,
                json TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS templates (
                name TEXT PRIMARY KEY,
                version INTEGER NOT NULL,
                updated_utc TEXT NOT NULL,
                json TEXT NOT NULL
            );
            """;
        await using var command = connection.CreateCommand();
        command.CommandText = commandText;
        await command.ExecuteNonQueryAsync(ct);
    }

    public async ValueTask SeedSampleDataAsync(CancellationToken ct = default)
    {
        await InitializeAsync(ct);
        var transformation = new TransformationProfile(
            "default-transform",
            new[]
            {
                new TransformationRule("BUS1_V", new[]
                {
                    new TransformationStep("CONVERT", "double"),
                    new TransformationStep("EXPRESSION", "IncomingValue * 1000")
                }),
                new TransformationRule("BRK1_ST", new[]
                {
                    new TransformationStep("ENUM", Map: new Dictionary<string, string>
                    {
                        ["OPEN"] = "1",
                        ["CLOSED"] = "2",
                        ["TRIP"] = "3"
                    }),
                    new TransformationStep("CONVERT", "int")
                }),
                new TransformationRule("TEMP1", new[]
                {
                    new TransformationStep("CONVERT", "double")
                })
            });

        var validation = new ValidationProfile(
            "default-validation",
            PartialUpdateMode.AcceptValidOnly,
            new[]
            {
                new ValidationRule("BUS1_V", Required: true, ExpectedType: "double", Minimum: 0, Maximum: 500000),
                new ValidationRule("BRK1_ST", Required: true, ExpectedType: "int", Minimum: 1, Maximum: 3),
                new ValidationRule("TEMP1", Required: true, ExpectedType: "double", Minimum: -50, Maximum: 200)
            });

        var jsonIngress = new IngressProfile(
            "json-station",
            "json",
            new MappingProfile(
                "json-mapping",
                "json",
                new[]
                {
                    new MappingRule("StationA.Voltage", "BUS1_V", Metadata: new Dictionary<string, string> { ["unit"] = "kV" }),
                    new MappingRule("StationA.Breaker", "BRK1_ST"),
                    new MappingRule("StationA.Temperature", "TEMP1")
                }),
            transformation.Name,
            validation.Name,
            TracePayloads: true);

        var csvIngress = new IngressProfile(
            "csv-station",
            "csv",
            new MappingProfile(
                "csv-mapping",
                "csv",
                new[]
                {
                    new MappingRule("StationA.Voltage", "BUS1_V"),
                    new MappingRule("StationA.Breaker", "BRK1_ST"),
                    new MappingRule("StationA.Temperature", "TEMP1")
                }),
            transformation.Name,
            validation.Name);

        var txtIngress = new IngressProfile(
            "txt-station",
            "txt",
            new MappingProfile(
                "txt-mapping",
                "txt",
                new[]
                {
                    new MappingRule("StationA.Voltage", "BUS1_V"),
                    new MappingRule("StationA.Breaker", "BRK1_ST"),
                    new MappingRule("StationA.Temperature", "TEMP1")
                }),
            transformation.Name,
            validation.Name);

        var templates = new[]
        {
            new TemplateDefinition("json-out", "json", """
                {
                  "Substation": {
                    "Voltage": "${BUS1_V:F2}",
                    "BreakerCode": "${BRK1_ST}",
                    "Temperature": "${TEMP1:F1}"
                  }
                }
                """),
            new TemplateDefinition("csv-out", "csv", """
                Voltage,BreakerCode,Temperature
                ${BUS1_V:F2},${BRK1_ST},${TEMP1:F1}
                """),
            new TemplateDefinition("txt-out", "txt", """
                Voltage=${BUS1_V:F2}
                BreakerAlarm=${BRK1_ST=="1" ? 1 : 0}
                Temperature=${TEMP1:F1}
                """)
        };

        await UpsertAsync("ingress_profiles", jsonIngress.Name, jsonIngress.Version, jsonIngress.UpdatedUtc ?? DateTimeOffset.UtcNow, JsonSerializer.Serialize(jsonIngress, SerializerOptions), ct);
        await UpsertAsync("ingress_profiles", csvIngress.Name, csvIngress.Version, csvIngress.UpdatedUtc ?? DateTimeOffset.UtcNow, JsonSerializer.Serialize(csvIngress, SerializerOptions), ct);
        await UpsertAsync("ingress_profiles", txtIngress.Name, txtIngress.Version, txtIngress.UpdatedUtc ?? DateTimeOffset.UtcNow, JsonSerializer.Serialize(txtIngress, SerializerOptions), ct);
        await UpsertAsync("transformation_profiles", transformation.Name, transformation.Version, transformation.UpdatedUtc ?? DateTimeOffset.UtcNow, JsonSerializer.Serialize(transformation, SerializerOptions), ct);
        await UpsertAsync("validation_profiles", validation.Name, validation.Version, validation.UpdatedUtc ?? DateTimeOffset.UtcNow, JsonSerializer.Serialize(validation, SerializerOptions), ct);
        foreach (var template in templates)
        {
            await UpsertAsync("templates", template.Name, template.Version, template.UpdatedUtc ?? DateTimeOffset.UtcNow, JsonSerializer.Serialize(template, SerializerOptions), ct);
        }

        logger.LogInformation("Seeded SQLite configuration with sample profiles and templates.");
    }

    public ValueTask<IngressProfile?> GetIngressProfileAsync(string name, CancellationToken ct = default)
        => GetSingleAsync<IngressProfile>("ingress_profiles", name, ct);

    public ValueTask<TransformationProfile?> GetTransformationProfileAsync(string name, CancellationToken ct = default)
        => GetSingleAsync<TransformationProfile>("transformation_profiles", name, ct);

    public ValueTask<ValidationProfile?> GetValidationProfileAsync(string name, CancellationToken ct = default)
        => GetSingleAsync<ValidationProfile>("validation_profiles", name, ct);

    public ValueTask<TemplateDefinition?> GetTemplateDefinitionAsync(string name, CancellationToken ct = default)
        => GetSingleAsync<TemplateDefinition>("templates", name, ct);

    public async ValueTask<IReadOnlyCollection<TemplateDefinition>> GetTemplatesAsync(CancellationToken ct = default)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT json FROM templates ORDER BY name;";
        await using var reader = await command.ExecuteReaderAsync(ct);
        var items = new List<TemplateDefinition>();
        while (await reader.ReadAsync(ct))
        {
            items.Add(JsonSerializer.Deserialize<TemplateDefinition>(reader.GetString(0), SerializerOptions)!);
        }

        return items;
    }

    public async ValueTask ExportAsync(Stream destination, CancellationToken ct = default)
    {
        var export = new ConfigurationExport(
            await GetAllAsync<IngressProfile>("ingress_profiles", ct),
            await GetAllAsync<TransformationProfile>("transformation_profiles", ct),
            await GetAllAsync<ValidationProfile>("validation_profiles", ct),
            await GetAllAsync<TemplateDefinition>("templates", ct));
        await JsonSerializer.SerializeAsync(destination, export, SerializerOptions, ct);
    }

    public async ValueTask ImportAsync(Stream source, CancellationToken ct = default)
    {
        var export = await JsonSerializer.DeserializeAsync<ConfigurationExport>(source, SerializerOptions, ct)
            ?? throw new InvalidOperationException("Configuration import payload is invalid.");
        await InitializeAsync(ct);
        foreach (var item in export.IngressProfiles)
        {
            await UpsertAsync("ingress_profiles", item.Name, item.Version, item.UpdatedUtc ?? DateTimeOffset.UtcNow, JsonSerializer.Serialize(item, SerializerOptions), ct);
        }

        foreach (var item in export.TransformationProfiles)
        {
            await UpsertAsync("transformation_profiles", item.Name, item.Version, item.UpdatedUtc ?? DateTimeOffset.UtcNow, JsonSerializer.Serialize(item, SerializerOptions), ct);
        }

        foreach (var item in export.ValidationProfiles)
        {
            await UpsertAsync("validation_profiles", item.Name, item.Version, item.UpdatedUtc ?? DateTimeOffset.UtcNow, JsonSerializer.Serialize(item, SerializerOptions), ct);
        }

        foreach (var item in export.Templates)
        {
            await UpsertAsync("templates", item.Name, item.Version, item.UpdatedUtc ?? DateTimeOffset.UtcNow, JsonSerializer.Serialize(item, SerializerOptions), ct);
        }
    }

    private async ValueTask<T?> GetSingleAsync<T>(string table, string name, CancellationToken ct) where T : class
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = $"SELECT json FROM {table} WHERE name = $name;";
        command.Parameters.AddWithValue("$name", name);
        var result = await command.ExecuteScalarAsync(ct);
        return result is string json ? JsonSerializer.Deserialize<T>(json, SerializerOptions) : null;
    }

    private async ValueTask<IReadOnlyList<T>> GetAllAsync<T>(string table, CancellationToken ct)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = $"SELECT json FROM {table} ORDER BY name;";
        await using var reader = await command.ExecuteReaderAsync(ct);
        var items = new List<T>();
        while (await reader.ReadAsync(ct))
        {
            items.Add(JsonSerializer.Deserialize<T>(reader.GetString(0), SerializerOptions)!);
        }

        return items;
    }

    private async ValueTask UpsertAsync(string table, string name, int version, DateTimeOffset updatedUtc, string json, CancellationToken ct)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = $"INSERT INTO {table}(name, version, updated_utc, json) VALUES($name, $version, $updated, $json) ON CONFLICT(name) DO UPDATE SET version = excluded.version, updated_utc = excluded.updated_utc, json = excluded.json;";
        command.Parameters.AddWithValue("$name", name);
        command.Parameters.AddWithValue("$version", version);
        command.Parameters.AddWithValue("$updated", updatedUtc.ToString("O", System.Globalization.CultureInfo.InvariantCulture));
        command.Parameters.AddWithValue("$json", json);
        await command.ExecuteNonQueryAsync(ct);
    }
}
