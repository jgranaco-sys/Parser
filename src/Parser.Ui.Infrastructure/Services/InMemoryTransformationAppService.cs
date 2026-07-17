using Parser.Core.Interfaces;
using Parser.Core.Models;

namespace Parser.Ui.Infrastructure.Services;

public class InMemoryTransformationAppService : ITransformationAppService
{
    private readonly List<TransformationRule> _rules = new();
    private readonly object _lock = new();

    public Task<IReadOnlyList<TransformationRule>> GetRulesAsync(string profileId)
    {
        lock (_lock)
        {
            return Task.FromResult<IReadOnlyList<TransformationRule>>(
                _rules.Where(r => r.ProfileId == profileId).Select(r => r.Clone()).ToList());
        }
    }

    public Task<TransformationRule> AddRuleAsync(string profileId, TransformationRule rule)
    {
        lock (_lock)
        {
            var stored = rule.Clone();
            if (string.IsNullOrEmpty(stored.Id))
            {
                stored.Id = Guid.NewGuid().ToString();
            }
            stored.ProfileId = profileId;
            _rules.Add(stored);
            return Task.FromResult(stored.Clone());
        }
    }

    public Task<TransformationRule> UpdateRuleAsync(string profileId, TransformationRule rule)
    {
        lock (_lock)
        {
            var existing = _rules.FirstOrDefault(r => r.Id == rule.Id && r.ProfileId == profileId);
            if (existing == null)
            {
                throw new InvalidOperationException($"Rule '{rule.Id}' not found.");
            }

            existing.Name = rule.Name;
            existing.Expression = rule.Expression;
            existing.SourceField = rule.SourceField;
            existing.TargetField = rule.TargetField;
            existing.Type = rule.Type;
            return Task.FromResult(existing.Clone());
        }
    }

    public Task DeleteRuleAsync(string profileId, string ruleId)
    {
        lock (_lock)
        {
            _rules.RemoveAll(r => r.Id == ruleId && r.ProfileId == profileId);
            return Task.CompletedTask;
        }
    }
}
