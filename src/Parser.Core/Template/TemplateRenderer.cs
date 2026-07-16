namespace Parser.Core.Template;

public sealed partial class TemplateRenderer : ITemplateRenderer
{
    [GeneratedRegex(@"\$\{(?<expr>[^}]+)\}", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex PlaceholderRegex();

    public IReadOnlyCollection<string> DiscoverVariables(string templateContent)
    {
        var variables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (Match match in PlaceholderRegex().Matches(templateContent))
        {
            var expression = match.Groups["expr"].Value.Trim();
            var variable = ExtractVariableName(expression);
            if (!string.IsNullOrWhiteSpace(variable))
            {
                variables.Add(variable);
            }
        }

        return variables.ToArray();
    }

    public string Render(string templateContent, IReadOnlyDictionary<string, ScadaValue> values)
    {
        return PlaceholderRegex().Replace(templateContent, match =>
        {
            var expression = match.Groups["expr"].Value.Trim();
            return Evaluate(expression, values);
        });
    }

    private static string Evaluate(string expression, IReadOnlyDictionary<string, ScadaValue> values)
    {
        if (expression.Contains('?', StringComparison.Ordinal) && (expression.Contains("==", StringComparison.Ordinal) || expression.Contains("!=", StringComparison.Ordinal)))
        {
            return EvaluateConditional(expression, values);
        }

        var parts = expression.Split(':', 2, StringSplitOptions.TrimEntries);
        var name = parts[0];
        var format = parts.Length > 1 ? parts[1] : null;
        return TryFormat(values, name, format);
    }

    private static string EvaluateConditional(string expression, IReadOnlyDictionary<string, ScadaValue> values)
    {
        var parts = expression.Split('?', 2, StringSplitOptions.TrimEntries);
        var condition = parts[0];
        var branches = parts[1].Split(':', 2, StringSplitOptions.TrimEntries);
        var conditionResult = EvaluateEquality(condition, values);
        return NormalizeLiteral(conditionResult ? branches[0] : branches[1]);
    }

    private static bool EvaluateEquality(string condition, IReadOnlyDictionary<string, ScadaValue> values)
    {
        var op = condition.Contains("!=", StringComparison.Ordinal) ? "!=" : "==";
        var pieces = condition.Split(op, 2, StringSplitOptions.TrimEntries);
        var actual = TryFormat(values, pieces[0], null);
        var expected = NormalizeLiteral(pieces[1]);
        return op == "=="
            ? string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase)
            : !string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase);
    }

    private static string TryFormat(IReadOnlyDictionary<string, ScadaValue> values, string name, string? format)
    {
        if (!values.TryGetValue(name, out var value) || value.Value is null)
        {
            return string.Empty;
        }

        if (string.IsNullOrWhiteSpace(format))
        {
            return Convert.ToString(value.Value, CultureInfo.InvariantCulture) ?? string.Empty;
        }

        return value.Value switch
        {
            IFormattable formattable => formattable.ToString(format, CultureInfo.InvariantCulture),
            _ => Convert.ToString(value.Value, CultureInfo.InvariantCulture) ?? string.Empty
        };
    }

    private static string ExtractVariableName(string expression)
    {
        if (expression.Contains('?', StringComparison.Ordinal))
        {
            var condition = expression.Split('?', 2, StringSplitOptions.TrimEntries)[0];
            var op = condition.Contains("!=", StringComparison.Ordinal) ? "!=" : "==";
            return condition.Split(op, 2, StringSplitOptions.TrimEntries)[0];
        }

        return expression.Split(':', 2, StringSplitOptions.TrimEntries)[0];
    }

    private static string NormalizeLiteral(string text)
    {
        var trimmed = text.Trim();
        if ((trimmed.StartsWith('"') && trimmed.EndsWith('"')) || (trimmed.StartsWith("'") && trimmed.EndsWith("'")))
        {
            return trimmed[1..^1];
        }

        return trimmed;
    }
}
