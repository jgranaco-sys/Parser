namespace Parser.SamplePlugin;

/// <summary>
/// Sample plugin parser for semicolon-separated key/value text payloads.
/// </summary>
public sealed class SemicolonTextParser : IParserStrategy
{
    public string Format => "kvp-semi";

    public ValueTask<CanonicalDocument> ParseAsync(PayloadEnvelope envelope, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var text = Encoding.UTF8.GetString(envelope.Payload);
        var values = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var segment in text.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var separatorIndex = segment.IndexOf('=');
            if (separatorIndex <= 0)
            {
                continue;
            }

            values[segment[..separatorIndex].Trim()] = segment[(separatorIndex + 1)..].Trim();
        }

        return ValueTask.FromResult(new CanonicalDocument(values));
    }
}
