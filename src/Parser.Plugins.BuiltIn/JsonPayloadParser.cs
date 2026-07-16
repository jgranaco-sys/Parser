namespace Parser.Plugins.BuiltIn;

/// <summary>
/// Parses JSON payloads using a streaming Utf8JsonReader.
/// </summary>
public sealed class JsonPayloadParser : IParserStrategy
{
    public string Format => "json";

    public ValueTask<CanonicalDocument> ParseAsync(PayloadEnvelope envelope, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var values = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        var reader = new Utf8JsonReader(envelope.Payload, isFinalBlock: true, state: default);
        var contexts = new List<ContainerContext>();
        string? pendingProperty = null;

        while (reader.Read())
        {
            ct.ThrowIfCancellationRequested();
            switch (reader.TokenType)
            {
                case JsonTokenType.PropertyName:
                    pendingProperty = reader.GetString();
                    break;
                case JsonTokenType.StartObject:
                    contexts.Add(new ContainerContext(ContainerType.Object, BuildPath(contexts, pendingProperty), 0));
                    pendingProperty = null;
                    break;
                case JsonTokenType.EndObject:
                    if (contexts.Count > 0)
                    {
                        contexts.RemoveAt(contexts.Count - 1);
                    }
                    break;
                case JsonTokenType.StartArray:
                    contexts.Add(new ContainerContext(ContainerType.Array, BuildPath(contexts, pendingProperty), 0));
                    pendingProperty = null;
                    break;
                case JsonTokenType.EndArray:
                    if (contexts.Count > 0)
                    {
                        contexts.RemoveAt(contexts.Count - 1);
                    }
                    break;
                case JsonTokenType.String:
                    WriteScalar(values, contexts, pendingProperty, reader.GetString());
                    pendingProperty = null;
                    break;
                case JsonTokenType.Number:
                    WriteScalar(values, contexts, pendingProperty, reader.TryGetInt64(out var integer) ? integer : reader.GetDouble());
                    pendingProperty = null;
                    break;
                case JsonTokenType.True:
                    WriteScalar(values, contexts, pendingProperty, true);
                    pendingProperty = null;
                    break;
                case JsonTokenType.False:
                    WriteScalar(values, contexts, pendingProperty, false);
                    pendingProperty = null;
                    break;
                case JsonTokenType.Null:
                    WriteScalar(values, contexts, pendingProperty, null);
                    pendingProperty = null;
                    break;
            }
        }

        return ValueTask.FromResult(new CanonicalDocument(values));
    }

    private static void WriteScalar(IDictionary<string, object?> values, List<ContainerContext> contexts, string? pendingProperty, object? value)
    {
        var path = BuildPath(contexts, pendingProperty);
        values[path] = value;
    }

    private static string BuildPath(List<ContainerContext> contexts, string? pendingProperty)
    {
        if (contexts.Count == 0)
        {
            return pendingProperty ?? string.Empty;
        }

        var parent = contexts[^1];
        if (parent.Type == ContainerType.Array)
        {
            var arrayPath = string.IsNullOrWhiteSpace(parent.Prefix)
                ? $"[{parent.NextIndex}]"
                : $"{parent.Prefix}[{parent.NextIndex}]";
            contexts[^1] = parent with { NextIndex = parent.NextIndex + 1 };
            return string.IsNullOrWhiteSpace(pendingProperty) ? arrayPath : $"{arrayPath}.{pendingProperty}";
        }

        if (string.IsNullOrWhiteSpace(parent.Prefix))
        {
            return pendingProperty ?? string.Empty;
        }

        return string.IsNullOrWhiteSpace(pendingProperty)
            ? parent.Prefix
            : $"{parent.Prefix}.{pendingProperty}";
    }

    private enum ContainerType
    {
        Object,
        Array
    }

    private readonly record struct ContainerContext(ContainerType Type, string Prefix, int NextIndex);
}
