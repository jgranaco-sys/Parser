namespace Parser.Plugins.BuiltIn;

/// <summary>
/// Parses CSV payloads using CsvHelper.
/// </summary>
public sealed class CsvPayloadParser : IParserStrategy
{
    public string Format => "csv";

    public async ValueTask<CanonicalDocument> ParseAsync(PayloadEnvelope envelope, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        using var reader = new StringReader(Encoding.UTF8.GetString(envelope.Payload));
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        var records = new List<dynamic>();
        await foreach (var record in csv.GetRecordsAsync<dynamic>(ct))
        {
            records.Add(record);
        }

        var values = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        for (var rowIndex = 0; rowIndex < records.Count; rowIndex++)
        {
            foreach (var pair in (IDictionary<string, object>)records[rowIndex])
            {
                values[$"rows[{rowIndex}].{pair.Key}"] = pair.Value;
                if (records.Count == 1)
                {
                    values[pair.Key] = pair.Value;
                }
            }
        }

        return new CanonicalDocument(values);
    }
}
