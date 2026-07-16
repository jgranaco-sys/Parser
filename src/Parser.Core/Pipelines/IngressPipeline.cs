namespace Parser.Core.Pipelines;

public sealed class IngressPipeline(
    IEnumerable<IParserStrategy> parsers,
    IMappingEngine mappingEngine,
    ITransformationEngine transformationEngine,
    IValidationEngine validationEngine,
    IScadaVariableProvider scadaVariableProvider,
    IProfileRepository profileRepository,
    IDeadLetterSink deadLetterSink,
    EngineMetrics metrics,
    ILogger<IngressPipeline> logger) : IIngressPipeline
{
    private readonly Dictionary<string, IParserStrategy> _parsers = parsers.ToDictionary(parser => parser.Format, StringComparer.OrdinalIgnoreCase);

    public async ValueTask<IngressResult> ProcessAsync(string profileName, PayloadEnvelope envelope, CancellationToken ct = default)
    {
        using var scope = logger.BeginScope(new Dictionary<string, object?>
        {
            ["messageId"] = envelope.MessageId,
            ["correlationId"] = envelope.CorrelationId,
            ["topic"] = envelope.Topic,
            ["profile"] = profileName
        });

        var ingressProfile = await profileRepository.GetIngressProfileAsync(profileName, ct)
            ?? throw new InvalidOperationException($"Ingress profile '{profileName}' was not found.");
        var transformationProfile = await profileRepository.GetTransformationProfileAsync(ingressProfile.TransformationProfileName, ct)
            ?? throw new InvalidOperationException($"Transformation profile '{ingressProfile.TransformationProfileName}' was not found.");
        var validationProfile = await profileRepository.GetValidationProfileAsync(ingressProfile.ValidationProfileName, ct)
            ?? throw new InvalidOperationException($"Validation profile '{ingressProfile.ValidationProfileName}' was not found.");

        var parser = ResolveParser(envelope.FormatHint ?? ingressProfile.Format);
        var started = Stopwatch.GetTimestamp();
        CanonicalDocument document;
        try
        {
            document = await parser.ParseAsync(envelope, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to parse inbound payload.");
            metrics.RecordFailure();
            await deadLetterSink.WriteAsync(new DeadLetterMessage("ParseFailure", envelope, new[] { new ValidationIssue(ValidationSeverity.Error, string.Empty, ex.Message) }, DateTimeOffset.UtcNow), ct);
            throw;
        }

        metrics.RecordParseLatency(Stopwatch.GetElapsedTime(started));
        var mapped = mappingEngine.Map(document, ingressProfile.Mapping);
        var transformed = transformationEngine.Transform(mapped, transformationProfile);
        var validation = validationEngine.Validate(transformed, validationProfile, DateTimeOffset.UtcNow);

        var rejectedNames = validation.Points.Where(point => !point.Accepted).Select(point => point.VariableName).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var accepted = transformed.Where(point => !rejectedNames.Contains(point.Name)).ToArray();
        var rejected = transformed.Where(point => rejectedNames.Contains(point.Name)).ToArray();

        if (validation.HasErrors && validationProfile.PartialUpdateMode == PartialUpdateMode.RejectAll)
        {
            rejected = transformed.ToArray();
            accepted = Array.Empty<ScadaWriteRequest>();
        }

        if (accepted.Length > 0)
        {
            var bulkPayload = accepted.ToDictionary(item => item.Name, item => item.Value, StringComparer.OrdinalIgnoreCase);
            await RetryPolicy.ExecuteAsync(
                async token => await scadaVariableProvider.WriteMultipleAsync(bulkPayload, token),
                ingressProfile.RetryCount,
                ct);
        }

        if (rejected.Length > 0)
        {
            metrics.RecordFailure();
            await deadLetterSink.WriteAsync(new DeadLetterMessage("ValidationFailure", envelope, validation.Issues, DateTimeOffset.UtcNow), ct);
        }
        else
        {
            metrics.RecordSuccess(accepted.Length);
        }

        if (ingressProfile.TracePayloads)
        {
            logger.LogInformation("Payload trace: {payload}", Encoding.UTF8.GetString(envelope.Payload));
        }

        logger.LogInformation("Ingress processed {ParsedFieldCount} fields and wrote {WrittenCount} values.", document.Values.Count, accepted.Length);
        return new IngressResult(rejected.Length == 0, document.Values.Count, accepted.Length, validation, accepted, rejected);
    }

    private IParserStrategy ResolveParser(string format)
    {
        if (_parsers.TryGetValue(format, out var parser))
        {
            return parser;
        }

        throw new InvalidOperationException($"Parser for format '{format}' is not registered.");
    }
}
