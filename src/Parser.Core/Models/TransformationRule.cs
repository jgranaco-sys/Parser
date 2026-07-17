namespace Parser.Core.Models;

public enum TransformationType
{
    Math,
    Enum,
    String,
    Unit,
    Date,
    Boolean
}

public class TransformationRule
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ProfileId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Expression { get; set; } = string.Empty;
    public string SourceField { get; set; } = string.Empty;
    public string TargetField { get; set; } = string.Empty;
    public TransformationType Type { get; set; } = TransformationType.String;

    /// <summary>Creates a shallow copy so callers cannot mutate shared/stored state.</summary>
    public TransformationRule Clone() => (TransformationRule)MemberwiseClone();
}
