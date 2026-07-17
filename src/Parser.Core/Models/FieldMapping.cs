namespace Parser.Core.Models;

public enum MappingStatus
{
    Mapped,
    Partial,
    Missing
}

public class FieldMapping
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ProfileId { get; set; } = string.Empty;
    public string IncomingField { get; set; } = string.Empty;
    public string ScadaVariable { get; set; } = string.Empty;
    public string DataType { get; set; } = "string";
    public string Transform { get; set; } = string.Empty;
    public MappingStatus Status { get; set; } = MappingStatus.Missing;

    /// <summary>Creates a shallow copy so callers cannot mutate shared/stored state.</summary>
    public FieldMapping Clone() => (FieldMapping)MemberwiseClone();
}
