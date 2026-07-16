namespace Parser.Core.Mapping;

public sealed class MappingEngine : IMappingEngine
{
    public IReadOnlyList<MappedPoint> Map(CanonicalDocument document, MappingProfile profile)
    {
        var results = new List<MappedPoint>();
        foreach (var rule in profile.Rules)
        {
            var matches = document.Values
                .Where(entry => PathPatternMatcher.TryMatch(rule.SourcePath, entry.Key, out _))
                .ToArray();

            if (matches.Length == 0)
            {
                if (rule.DefaultValue is not null)
                {
                    results.Add(new MappedPoint(rule.SourcePath, rule.TargetVariable, rule.DefaultValue, true, rule.Metadata));
                }
                else if (!rule.Optional)
                {
                    results.Add(new MappedPoint(rule.SourcePath, rule.TargetVariable, null, false, rule.Metadata));
                }

                continue;
            }

            foreach (var match in matches)
            {
                _ = PathPatternMatcher.TryMatch(rule.SourcePath, match.Key, out var captures);
                results.Add(new MappedPoint(
                    match.Key,
                    PathPatternMatcher.ApplyTargetTemplate(rule.TargetVariable, captures),
                    match.Value,
                    false,
                    rule.Metadata));
            }
        }

        return results;
    }
}
