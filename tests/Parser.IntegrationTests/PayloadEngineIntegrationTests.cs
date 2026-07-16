using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Parser.Abstractions;
using Parser.Abstractions.Contracts;
using Parser.Core.Mapping;
using Parser.Core.Observability;
using Parser.Core.Pipelines;
using Parser.Core.Plugins;
using Parser.Core.Providers;
using Parser.Core.Template;
using Parser.Core.Transformations;
using Parser.Core.Validation;
using Parser.Plugins.BuiltIn;
using Parser.Storage.Sqlite;
using Xunit;

namespace Parser.IntegrationTests;

public sealed class PayloadEngineIntegrationTests
{
    [Fact]
    public async Task IngressAndEgress_WorkEndToEndAgainstSqliteProfiles()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"parser-{Guid.NewGuid():N}.db");
        try
        {
            var connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = dbPath,
                Pooling = false
            }.ToString();
            var repository = new SqliteProfileRepository(connectionString, NullLogger<SqliteProfileRepository>.Instance);
            await repository.InitializeAsync();
            await repository.SeedSampleDataAsync();

            var scada = new InMemoryScadaVariableProvider();
            var ingress = new IngressPipeline(
                new IParserStrategy[] { new JsonPayloadParser(), new CsvPayloadParser(), new TextPayloadParser() },
                new MappingEngine(),
                new TransformationEngine(),
                new ValidationEngine(),
                scada,
                repository,
                new InMemoryDeadLetterSink(),
                new EngineMetrics(),
                NullLogger<IngressPipeline>.Instance);
            var egress = new EgressPipeline(new TemplateRenderer(), scada, repository, NullLogger<EgressPipeline>.Instance);

            var payload = """
                {
                  "StationA": {
                    "Voltage": 138.2,
                    "Breaker": "OPEN",
                    "Temperature": 25.3
                  }
                }
                """;

            var result = await ingress.ProcessAsync("json-station", new PayloadEnvelope("demo/json", null, System.Text.Encoding.UTF8.GetBytes(payload), "json"));
            var rendered = await egress.GenerateAsync("txt-out");
            var bus = await scada.ReadVariableAsync("BUS1_V");

            Assert.True(result.Success);
            Assert.Equal(138200d, bus!.Value);
            Assert.Contains("Voltage=138200.00", rendered.Content);
            Assert.Contains("BreakerAlarm=1", rendered.Content);
        }
        finally
        {
            await DeleteFileWhenReleasedAsync(dbPath);
        }
    }

    [Fact]
    public void PluginLoader_LoadsExternalPluginModuleFromPluginsFolder()
    {
        var pluginDirectory = Path.Combine(Path.GetTempPath(), $"parser-plugins-{Guid.NewGuid():N}");
        Directory.CreateDirectory(pluginDirectory);
        var pluginAssemblyPath = typeof(Parser.SamplePlugin.SamplePluginModule).Assembly.Location;
        File.Copy(pluginAssemblyPath, Path.Combine(pluginDirectory, Path.GetFileName(pluginAssemblyPath)), overwrite: true);

        var services = new ServiceCollection();
        var loaded = PluginLoader.LoadFromDirectory(services, pluginDirectory);
        using var provider = services.BuildServiceProvider();
        var parsers = provider.GetServices<IParserStrategy>().ToArray();

        Assert.NotEmpty(loaded);
        Assert.Contains(parsers, parser => parser.Format == "kvp-semi");
    }

    private static async Task DeleteFileWhenReleasedAsync(string path)
    {
        if (!File.Exists(path))
        {
            return;
        }

        for (var attempt = 0; attempt < 5; attempt++)
        {
            try
            {
                File.Delete(path);
                return;
            }
            catch (IOException) when (attempt < 4)
            {
                await Task.Delay(50);
            }
            catch (UnauthorizedAccessException) when (attempt < 4)
            {
                await Task.Delay(50);
            }
        }
    }
}
