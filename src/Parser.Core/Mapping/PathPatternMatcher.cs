namespace Parser.Core.Mapping;

internal static partial class PathPatternMatcher
{
    [GeneratedRegex(@"\[(\d+)\]", RegexOptions.Compiled)]
    private static partial Regex IndexRegex();

    public static bool TryMatch(string pattern, string candidate, out string[] captures)
    {
        captures = Array.Empty<string>();
        if (string.Equals(pattern, candidate, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var regexPattern = BuildRegexPattern(pattern, out var captureCount);
        var match = Regex.Match(candidate, regexPattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (!match.Success)
        {
            return false;
        }

        captures = Enumerable.Range(1, captureCount)
            .Select(index => match.Groups[index].Value)
            .ToArray();
        return true;
    }

    public static string ApplyTargetTemplate(string targetTemplate, IReadOnlyList<string> captures)
    {
        var result = targetTemplate;
        for (var index = 0; index < captures.Count; index++)
        {
            result = result.Replace($"{{{index}}}", captures[index], StringComparison.Ordinal);
        }

        return result;
    }

    private static string BuildRegexPattern(string pattern, out int captureCount)
    {
        var builder = new StringBuilder("^");
        captureCount = 0;
        for (var index = 0; index < pattern.Length; index++)
        {
            if (index + 3 <= pattern.Length && pattern.AsSpan(index, 3).SequenceEqual("[*]"))
            {
                builder.Append(@"\[(\d+)\]");
                captureCount++;
                index += 2;
                continue;
            }

            var current = pattern[index];
            if (current == '*')
            {
                builder.Append(@"([^.\[]+)");
                captureCount++;
                continue;
            }

            builder.Append(Regex.Escape(current.ToString()));
        }

        builder.Append('$');
        return builder.ToString();
    }
}
