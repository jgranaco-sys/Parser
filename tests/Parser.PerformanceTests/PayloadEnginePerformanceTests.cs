using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using Parser.Abstractions;
using Parser.Abstractions.Contracts;
using Parser.Core.Mapping;
using Parser.Core.Observability;
using Parser.Core.Pipelines;
using Parser.Core.Providers;
using Parser.Core.Template;
using Parser.Core.Transformations;
using Parser.Core.Validation;
using Parser.Plugins.BuiltIn;
using Parser.Storage.Sqlite;
using Xunit;

namespace Parser.PerformanceTests;

public sealed class PayloadEnginePerformanceTests
{
    [Fact]
    public async Task IngressPipeline_ProcessesHighVolumeSyntheticPayloads()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"parser-perf-{Guid.NewGuid():N}.db");
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
            var metrics = new EngineMetrics();
            var ingress = new IngressPipeline(
                new IParserStrategy[] { new JsonPayloadParser(), new CsvPayloadParser(), new TextPayloadParser() },
                new MappingEngine(),
                new TransformationEngine(),
                new ValidationEngine(),
                scada,
                repository,
                new InMemoryDeadLetterSink(),
                metrics,
                NullLogger<IngressPipeline>.Instance);

            var payload = System.Text.Encoding.UTF8.GetBytes("{" + "\"StationA\":{\"Voltage\":138.2,\"Breaker\":\"OPEN\",\"Temperature\":25.3}}");
            for (var index = 0; index < 250; index++)
            {
                var result = await ingress.ProcessAsync("json-station", new PayloadEnvelope("perf/json", null, payload, "json", MessageId: index.ToString(), CorrelationId: "perf"));
                Assert.True(result.Success);
            }

            Assert.Equal(250, metrics.ProcessedCount);
            Assert.Equal(0, metrics.FailedCount);
        }
        finally
        {
            await DeleteFileWhenReleasedAsync(dbPath);
        }
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
