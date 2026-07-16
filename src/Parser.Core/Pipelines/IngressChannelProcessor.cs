namespace Parser.Core.Pipelines;

public sealed class IngressChannelProcessor : IAsyncDisposable
{
    private readonly Channel<(string Profile, PayloadEnvelope Envelope)> _channel;
    private readonly IIngressPipeline _pipeline;
    private readonly ILogger<IngressChannelProcessor> _logger;
    private readonly int _degreeOfParallelism;
    private readonly List<Task> _workers = new();
    private readonly CancellationTokenSource _cts = new();

    public IngressChannelProcessor(IIngressPipeline pipeline, ILogger<IngressChannelProcessor> logger, int capacity, int degreeOfParallelism)
    {
        _pipeline = pipeline;
        _logger = logger;
        _degreeOfParallelism = Math.Max(1, degreeOfParallelism);
        _channel = Channel.CreateBounded<(string Profile, PayloadEnvelope Envelope)>(new BoundedChannelOptions(Math.Max(1, capacity))
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = false,
            SingleWriter = false
        });
    }

    public void Start()
    {
        if (_workers.Count > 0)
        {
            return;
        }

        for (var index = 0; index < _degreeOfParallelism; index++)
        {
            _workers.Add(Task.Run(() => WorkerAsync(_cts.Token)));
        }
    }

    public ValueTask EnqueueAsync(string profileName, PayloadEnvelope envelope, CancellationToken ct = default)
        => _channel.Writer.WriteAsync((profileName, envelope), ct);

    private async Task WorkerAsync(CancellationToken ct)
    {
        await foreach (var item in _channel.Reader.ReadAllAsync(ct))
        {
            try
            {
                await _pipeline.ProcessAsync(item.Profile, item.Envelope, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ingress worker failed for profile {Profile}.", item.Profile);
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        _channel.Writer.TryComplete();
        _cts.Cancel();
        await Task.WhenAll(_workers);
        _cts.Dispose();
    }
}
