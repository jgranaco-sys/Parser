using Parser.Core.Interfaces;
using Parser.Core.Models;

namespace Parser.Ui.Infrastructure.Services;

public class InMemoryValidationAppService : IValidationAppService
{
    private readonly List<ValidationRule> _rules = new();
    private readonly object _lock = new();

    public Task<IReadOnlyList<ValidationRule>> GetRulesAsync(string profileId)
    {
        lock (_lock)
        {
            return Task.FromResult<IReadOnlyList<ValidationRule>>(
                _rules.Where(r => r.ProfileId == profileId).Select(r => r.Clone()).ToList());
        }
    }

    public Task<ValidationRule> AddRuleAsync(string profileId, ValidationRule rule)
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

    public Task<ValidationRule> UpdateRuleAsync(string profileId, ValidationRule rule)
    {
        lock (_lock)
        {
            var existing = _rules.FirstOrDefault(r => r.Id == rule.Id && r.ProfileId == profileId);
            if (existing == null)
            {
                throw new InvalidOperationException($"Rule '{rule.Id}' not found.");
            }

            existing.FieldName = rule.FieldName;
            existing.RuleType = rule.RuleType;
            existing.Parameters = rule.Parameters;
            existing.MissingBehavior = rule.MissingBehavior;
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
