namespace Parser.Core.Observability;

public sealed class EngineMetrics
{
    private readonly Meter _meter = new("Parser.Engine", "1.0.0");
    private readonly Counter<long> _processedCounter;
    private readonly Counter<long> _failureCounter;
    private readonly Histogram<double> _parseLatencyMs;
    private long _processed;
    private long _failed;

    public EngineMetrics()
    {
        _processedCounter = _meter.CreateCounter<long>("parser.messages.processed");
        _failureCounter = _meter.CreateCounter<long>("parser.messages.failed");
        _parseLatencyMs = _meter.CreateHistogram<double>("parser.parse.duration.ms");
    }

    public long ProcessedCount => Interlocked.Read(ref _processed);
    public long FailedCount => Interlocked.Read(ref _failed);

    public void RecordSuccess(int points)
    {
        Interlocked.Increment(ref _processed);
        _processedCounter.Add(1, new KeyValuePair<string, object?>("points", points));
    }

    public void RecordFailure()
    {
        Interlocked.Increment(ref _failed);
        _failureCounter.Add(1);
    }

    public void RecordParseLatency(TimeSpan elapsed)
    {
        _parseLatencyMs.Record(elapsed.TotalMilliseconds);
    }
}
