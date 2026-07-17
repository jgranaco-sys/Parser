using Parser.Core.Models;

namespace Parser.Core.Interfaces;

public interface IValidationAppService
{
    Task<IReadOnlyList<ValidationRule>> GetRulesAsync(string profileId);
    Task<ValidationRule> AddRuleAsync(string profileId, ValidationRule rule);
    Task<ValidationRule> UpdateRuleAsync(string profileId, ValidationRule rule);
    Task DeleteRuleAsync(string profileId, string ruleId);
}
