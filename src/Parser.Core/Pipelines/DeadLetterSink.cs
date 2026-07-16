namespace Parser.Core.Pipelines;

public sealed class InMemoryDeadLetterSink : IDeadLetterSink
{
    private readonly ConcurrentQueue<DeadLetterMessage> _messages = new();

    public IReadOnlyCollection<DeadLetterMessage> Messages => _messages.ToArray();

    public ValueTask WriteAsync(DeadLetterMessage message, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        _messages.Enqueue(message);
        return ValueTask.CompletedTask;
    }
}
