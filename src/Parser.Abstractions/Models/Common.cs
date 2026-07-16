namespace Parser.Abstractions;

public enum PartialUpdateMode
{
    RejectAll,
    AcceptValidOnly
}

public enum ValidationSeverity
{
    Warning,
    Error
}

public enum ScadaQuality
{
    Good,
    Uncertain,
    Bad
}

public sealed record PayloadEnvelope(
    string Topic,
    IReadOnlyDictionary<string, string>? Headers,
    byte[] Payload,
    string? FormatHint = null,
    string? ProfileName = null,
    string? MessageId = null,
    string? CorrelationId = null);

public sealed record CanonicalDocument(IReadOnlyDictionary<string, object?> Values)
{
    public bool TryGetValue(string path, out object? value) => Values.TryGetValue(path, out value);
}

public sealed record ScadaValue(
    object? Value,
    string DataType,
    DateTimeOffset TimestampUtc,
    ScadaQuality Quality = ScadaQuality.Good,
    IReadOnlyDictionary<string, string>? Metadata = null);

public sealed record MappedPoint(
    string SourcePath,
    string TargetVariable,
    object? Value,
    bool UsedDefault,
    IReadOnlyDictionary<string, string>? Metadata = null);

public sealed record ScadaWriteRequest(
    string Name,
    ScadaValue Value,
    string SourcePath,
    IReadOnlyDictionary<string, string>? Metadata = null);

public sealed record ValidationIssue(ValidationSeverity Severity, string VariableName, string Message);

public sealed record ValidationPointResult(string VariableName, bool Accepted, string? Reason = null);

public sealed record ValidationResult(
    IReadOnlyList<ValidationIssue> Issues,
    IReadOnlyList<ValidationPointResult> Points)
{
    public bool HasErrors => Issues.Any(static issue => issue.Severity == ValidationSeverity.Error);
}

public sealed record IngressResult(
    bool Success,
    int ParsedFieldCount,
    int WrittenCount,
    ValidationResult Validation,
    IReadOnlyList<ScadaWriteRequest> AcceptedWrites,
    IReadOnlyList<ScadaWriteRequest> RejectedWrites);

public sealed record EgressResult(string TemplateName, string Format, string Content, byte[] PayloadBytes);

public sealed record DeadLetterMessage(
    string Reason,
    PayloadEnvelope Envelope,
    IReadOnlyList<ValidationIssue> Issues,
    DateTimeOffset CreatedUtc);
