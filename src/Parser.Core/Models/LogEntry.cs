namespace Parser.Core.Models;

public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error,
    Critical
}

public class LogEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    public LogLevel Level { get; set; } = LogLevel.Info;
    public string Message { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;

    /// <summary>Creates a shallow copy so callers cannot mutate shared/stored state.</summary>
    public LogEntry Clone() => (LogEntry)MemberwiseClone();
}
