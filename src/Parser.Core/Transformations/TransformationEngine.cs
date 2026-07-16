namespace Parser.Core.Transformations;

public sealed partial class TransformationEngine : ITransformationEngine
{
    [GeneratedRegex(@"^IncomingValue\s*(?<operator>[*/+-])\s*(?<operand>-?\d+(?:\.\d+)?)$", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex ArithmeticExpressionRegex();

    private static readonly IReadOnlyDictionary<string, Func<double, double>> EngineeringConversions =
        new Dictionary<string, Func<double, double>>(StringComparer.OrdinalIgnoreCase)
        {
            ["F_TO_C"] = value => (value - 32d) * 5d / 9d,
            ["KV_TO_V"] = value => value * 1000d,
            ["MW_TO_W"] = value => value * 1_000_000d,
            ["KW_TO_W"] = value => value * 1000d
        };

    public IReadOnlyList<ScadaWriteRequest> Transform(IReadOnlyList<MappedPoint> points, TransformationProfile profile)
    {
        var rules = profile.Rules.ToDictionary(rule => rule.TargetVariable, StringComparer.OrdinalIgnoreCase);
        var results = new List<ScadaWriteRequest>(points.Count);
        foreach (var point in points)
        {
            var value = point.Value;
            if (rules.TryGetValue(point.TargetVariable, out var rule))
            {
                foreach (var step in rule.Steps)
                {
                    value = ApplyStep(value, step);
                }
            }

            var dataType = InferTypeName(value);
            results.Add(new ScadaWriteRequest(
                point.TargetVariable,
                new ScadaValue(value, dataType, DateTimeOffset.UtcNow, ScadaQuality.Good, point.Metadata),
                point.SourcePath,
                point.Metadata));
        }

        return results;
    }

    private static object? ApplyStep(object? value, TransformationStep step)
    {
        return step.Kind.ToUpperInvariant() switch
        {
            "ENUM" => ApplyEnum(value, step.Map),
            "ENGINEERING" => ApplyEngineering(value, step.Argument),
            "EXPRESSION" => ApplyExpression(value, step.Argument),
            "CONVERT" => ConvertValue(value, step.Argument),
            _ => value
        };
    }

    private static object? ApplyEnum(object? value, IReadOnlyDictionary<string, string>? map)
    {
        if (value is null || map is null)
        {
            return value;
        }

        var key = Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
        return map.TryGetValue(key, out var mapped) ? mapped : value;
    }

    private static object? ApplyEngineering(object? value, string? conversion)
    {
        if (value is null || conversion is null)
        {
            return value;
        }

        if (!TryToDouble(value, out var number))
        {
            return value;
        }

        return EngineeringConversions.TryGetValue(conversion, out var func)
            ? func(number)
            : value;
    }

    private static object? ApplyExpression(object? value, string? expression)
    {
        if (value is null || string.IsNullOrWhiteSpace(expression) || !TryToDouble(value, out var number))
        {
            return value;
        }

        var match = ArithmeticExpressionRegex().Match(expression.Trim());
        if (!match.Success)
        {
            return value;
        }

        var operand = double.Parse(match.Groups["operand"].Value, CultureInfo.InvariantCulture);
        return match.Groups["operator"].Value switch
        {
            "*" => number * operand,
            "/" => number / operand,
            "+" => number + operand,
            "-" => number - operand,
            _ => number
        };
    }

    private static object? ConvertValue(object? value, string? targetType)
    {
        if (targetType is null)
        {
            return value;
        }

        return targetType.ToLowerInvariant() switch
        {
            "string" => Convert.ToString(value, CultureInfo.InvariantCulture),
            "bool" or "boolean" => Convert.ToBoolean(value, CultureInfo.InvariantCulture),
            "int" or "int32" => Convert.ToInt32(value, CultureInfo.InvariantCulture),
            "long" or "int64" => Convert.ToInt64(value, CultureInfo.InvariantCulture),
            "double" => Convert.ToDouble(value, CultureInfo.InvariantCulture),
            "float" or "single" => Convert.ToSingle(value, CultureInfo.InvariantCulture),
            "datetime" => value is DateTimeOffset dto ? dto : DateTimeOffset.Parse(Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty, CultureInfo.InvariantCulture),
            _ => value
        };
    }

    private static bool TryToDouble(object value, out double number)
    {
        switch (value)
        {
            case byte b:
                number = b;
                return true;
            case short s:
                number = s;
                return true;
            case int i:
                number = i;
                return true;
            case long l:
                number = l;
                return true;
            case float f:
                number = f;
                return true;
            case double d:
                number = d;
                return true;
            case decimal m:
                number = (double)m;
                return true;
            case string text when double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed):
                number = parsed;
                return true;
            default:
                number = 0;
                return false;
        }
    }

    private static string InferTypeName(object? value) => value switch
    {
        null => "null",
        bool => "bool",
        int => "int",
        long => "long",
        float => "float",
        double => "double",
        decimal => "decimal",
        DateTimeOffset => "datetimeoffset",
        DateTime => "datetime",
        _ => "string"
    };
}
