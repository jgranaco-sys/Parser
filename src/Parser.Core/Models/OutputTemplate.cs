namespace Parser.Core.Models;

public enum OutputFormat
{
    JSON,
    CSV,
    TXT
}

public class OutputTemplate
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ProfileId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public OutputFormat Format { get; set; } = OutputFormat.JSON;
    public string Content { get; set; } = string.Empty;

    /// <summary>Creates a shallow copy so callers cannot mutate shared/stored state.</summary>
    public OutputTemplate Clone() => (OutputTemplate)MemberwiseClone();
}
