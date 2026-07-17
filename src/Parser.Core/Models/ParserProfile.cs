namespace Parser.Core.Models;

public class ParserProfile
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string InputFormat { get; set; } = "JSON";
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Creates a shallow copy so callers cannot mutate shared/stored state.</summary>
    public ParserProfile Clone() => (ParserProfile)MemberwiseClone();
}
