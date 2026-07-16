namespace Parser.Hosting;

public sealed class PayloadEngineInitializationService(
    IProfileRepository profileRepository,
    IngressChannelProcessor processor,
    ILogger<PayloadEngineInitializationService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await profileRepository.InitializeAsync(cancellationToken);
        await profileRepository.SeedSampleDataAsync(cancellationToken);
        processor.Start();
        logger.LogInformation("Payload engine initialized.");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
