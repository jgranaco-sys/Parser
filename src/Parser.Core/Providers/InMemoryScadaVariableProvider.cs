namespace Parser.Core.Providers;

public sealed class InMemoryScadaVariableProvider : IScadaVariableProvider
{
    private readonly ConcurrentDictionary<string, ScadaValue> _values = new(StringComparer.OrdinalIgnoreCase);

    public ValueTask<ScadaValue?> ReadVariableAsync(string name, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return ValueTask.FromResult(_values.TryGetValue(name, out var value) ? value : null);
    }

    public ValueTask WriteVariableAsync(string name, ScadaValue value, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        _values[name] = value;
        return ValueTask.CompletedTask;
    }

    public ValueTask<IReadOnlyDictionary<string, ScadaValue>> ReadMultipleAsync(IReadOnlyCollection<string> names, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        IReadOnlyDictionary<string, ScadaValue> result = names
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Where(name => _values.ContainsKey(name))
            .ToDictionary(name => name, name => _values[name], StringComparer.OrdinalIgnoreCase);
        return ValueTask.FromResult(result);
    }

    public ValueTask WriteMultipleAsync(IReadOnlyDictionary<string, ScadaValue> values, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        foreach (var pair in values)
        {
            _values[pair.Key] = pair.Value;
        }

        return ValueTask.CompletedTask;
    }
}
