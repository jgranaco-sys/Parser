namespace Parser.Hosting;

public sealed class DemoRunner(
    IIngressPipeline ingressPipeline,
    IEgressPipeline egressPipeline,
    IScadaVariableProvider scadaVariableProvider,
    IProfileRepository profileRepository,
    ILogger<DemoRunner> logger)
{
    public async Task RunAsync(CancellationToken ct = default)
    {
        await profileRepository.InitializeAsync(ct);
        await profileRepository.SeedSampleDataAsync(ct);

        var jsonPayload = """
            {
              "StationA": {
                "Voltage": 138.2,
                "Breaker": "OPEN",
                "Temperature": 25.3
              }
            }
            """;
        await ingressPipeline.ProcessAsync(
            "json-station",
            new PayloadEnvelope("demo/json", null, Encoding.UTF8.GetBytes(jsonPayload), "json", "json-station", Guid.NewGuid().ToString("N"), Guid.NewGuid().ToString("N")),
            ct);

        var csvPayload = """
            StationA.Voltage,StationA.Breaker,StationA.Temperature
            138.2,CLOSED,27.1
            """;
        await ingressPipeline.ProcessAsync(
            "csv-station",
            new PayloadEnvelope("demo/csv", null, Encoding.UTF8.GetBytes(csvPayload), "csv", "csv-station", Guid.NewGuid().ToString("N"), Guid.NewGuid().ToString("N")),
            ct);

        var txtPayload = """
            StationA.Voltage=139.4
            StationA.Breaker=TRIP
            StationA.Temperature=26.2
            """;
        await ingressPipeline.ProcessAsync(
            "txt-station",
            new PayloadEnvelope("demo/txt", null, Encoding.UTF8.GetBytes(txtPayload), "txt", "txt-station", Guid.NewGuid().ToString("N"), Guid.NewGuid().ToString("N")),
            ct);

        var jsonResult = await egressPipeline.GenerateAsync("json-out", ct);
        var csvResult = await egressPipeline.GenerateAsync("csv-out", ct);
        var txtResult = await egressPipeline.GenerateAsync("txt-out", ct);

        logger.LogInformation("Generated JSON payload:\n{Payload}", jsonResult.Content);
        logger.LogInformation("Generated CSV payload:\n{Payload}", csvResult.Content);
        logger.LogInformation("Generated TXT payload:\n{Payload}", txtResult.Content);

        var sampleValue = await scadaVariableProvider.ReadVariableAsync("BUS1_V", ct);
        logger.LogInformation("SCADA BUS1_V = {Value}", sampleValue?.Value);
    }
}
