namespace Parser.Plugins.BuiltIn;

/// <summary>
/// Parses text payloads containing key/value pairs or raw text.
/// </summary>
public sealed class TextPayloadParser : IParserStrategy
{
    public string Format => "txt";

    public ValueTask<CanonicalDocument> ParseAsync(PayloadEnvelope envelope, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var text = Encoding.UTF8.GetString(envelope.Payload);
        var values = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        var normalized = text.Replace("\r", string.Empty, StringComparison.Ordinal);
        var lines = normalized.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var line in lines)
        {
            var separatorIndex = line.IndexOf('=');
            if (separatorIndex <= 0)
            {
                values["text"] = text;
                continue;
            }

            var key = line[..separatorIndex].Trim();
            var value = line[(separatorIndex + 1)..].Trim();
            values[key] = value;
        }

        if (values.Count == 0)
        {
            values["text"] = text;
        }

        return ValueTask.FromResult(new CanonicalDocument(values));
    }
}
