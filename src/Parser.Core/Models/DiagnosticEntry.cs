namespace Parser.Core.Models;

public enum DiagnosticSeverity
{
    Info,
    Warning,
    Error
}

public class DiagnosticEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    public DiagnosticSeverity Severity { get; set; } = DiagnosticSeverity.Info;
    public string Category { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;

    /// <summary>Creates a shallow copy so callers cannot mutate shared/stored state.</summary>
    public DiagnosticEntry Clone() => (DiagnosticEntry)MemberwiseClone();
}
