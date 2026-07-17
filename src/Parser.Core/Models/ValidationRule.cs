namespace Parser.Core.Models;

public enum ValidationRuleType
{
    Required,
    MinValue,
    MaxValue,
    AllowedValues,
    Regex,
    Timestamp
}

public class ValidationRule
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ProfileId { get; set; } = string.Empty;
    public string FieldName { get; set; } = string.Empty;
    public ValidationRuleType RuleType { get; set; } = ValidationRuleType.Required;
    public string Parameters { get; set; } = string.Empty;
    public string MissingBehavior { get; set; } = "Reject";

    /// <summary>Creates a shallow copy so callers cannot mutate shared/stored state.</summary>
    public ValidationRule Clone() => (ValidationRule)MemberwiseClone();
}
