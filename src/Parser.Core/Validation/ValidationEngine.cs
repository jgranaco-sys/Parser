namespace Parser.Core.Validation;

public sealed class ValidationEngine : IValidationEngine
{
    public ValidationResult Validate(IReadOnlyList<ScadaWriteRequest> points, ValidationProfile profile, DateTimeOffset nowUtc)
    {
        var issues = new List<ValidationIssue>();
        var results = new List<ValidationPointResult>();
        var pointLookup = points.ToDictionary(point => point.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var rule in profile.Rules)
        {
            if (!pointLookup.TryGetValue(rule.TargetVariable, out var point))
            {
                if (rule.Required)
                {
                    issues.Add(new ValidationIssue(ValidationSeverity.Error, rule.TargetVariable, "Required value was not supplied."));
                    results.Add(new ValidationPointResult(rule.TargetVariable, false, "Required value missing."));
                }

                continue;
            }

            var accepted = true;
            if (rule.Required && point.Value.Value is null)
            {
                issues.Add(new ValidationIssue(ValidationSeverity.Error, rule.TargetVariable, "Required value was null."));
                accepted = false;
            }

            if (accepted && rule.ExpectedType is not null && !MatchesType(point.Value.Value, rule.ExpectedType))
            {
                issues.Add(new ValidationIssue(ValidationSeverity.Error, rule.TargetVariable, $"Expected type '{rule.ExpectedType}'."));
                accepted = false;
            }

            var numeric = 0d;
            if (accepted && (rule.Minimum.HasValue || rule.Maximum.HasValue) && !TryToDouble(point.Value.Value, out numeric))
            {
                issues.Add(new ValidationIssue(ValidationSeverity.Error, rule.TargetVariable, "Numeric range validation failed because the value is not numeric."));
                accepted = false;
            }
            else if (accepted && rule.Minimum.HasValue && numeric < rule.Minimum.Value)
            {
                issues.Add(new ValidationIssue(ValidationSeverity.Error, rule.TargetVariable, $"Value '{numeric}' is below minimum '{rule.Minimum.Value}'."));
                accepted = false;
            }
            else if (accepted && rule.Maximum.HasValue && numeric > rule.Maximum.Value)
            {
                issues.Add(new ValidationIssue(ValidationSeverity.Error, rule.TargetVariable, $"Value '{numeric}' is above maximum '{rule.Maximum.Value}'."));
                accepted = false;
            }

            if (accepted && rule.TimestampWindowSeconds.HasValue)
            {
                var age = nowUtc - point.Value.TimestampUtc;
                if (age.Duration().TotalSeconds > rule.TimestampWindowSeconds.Value)
                {
                    issues.Add(new ValidationIssue(ValidationSeverity.Error, rule.TargetVariable, $"Timestamp age '{age.TotalSeconds:F0}s' exceeded window '{rule.TimestampWindowSeconds.Value}s'."));
                    accepted = false;
                }
            }

            results.Add(new ValidationPointResult(rule.TargetVariable, accepted, accepted ? null : "Validation failed."));
        }

        foreach (var point in points.Where(point => results.All(result => !string.Equals(result.VariableName, point.Name, StringComparison.OrdinalIgnoreCase))))
        {
            results.Add(new ValidationPointResult(point.Name, true));
        }

        return new ValidationResult(issues, results);
    }

    private static bool MatchesType(object? value, string expectedType)
    {
        if (value is null)
        {
            return false;
        }

        return expectedType.ToLowerInvariant() switch
        {
            "string" => value is string,
            "bool" or "boolean" => value is bool,
            "int" or "int32" => value is int,
            "long" or "int64" => value is long or int,
            "double" => value is double or float or decimal or int or long,
            "float" or "single" => value is float,
            "datetime" => value is DateTime or DateTimeOffset,
            _ => true
        };
    }

    private static bool TryToDouble(object? value, out double numeric)
    {
        switch (value)
        {
            case null:
                numeric = 0;
                return false;
            case byte b:
                numeric = b;
                return true;
            case short s:
                numeric = s;
                return true;
            case int i:
                numeric = i;
                return true;
            case long l:
                numeric = l;
                return true;
            case float f:
                numeric = f;
                return true;
            case double d:
                numeric = d;
                return true;
            case decimal m:
                numeric = (double)m;
                return true;
            case string text when double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed):
                numeric = parsed;
                return true;
            default:
                numeric = 0;
                return false;
        }
    }
}
