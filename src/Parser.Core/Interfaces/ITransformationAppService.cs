using Parser.Core.Models;

namespace Parser.Core.Interfaces;

public interface ITransformationAppService
{
    Task<IReadOnlyList<TransformationRule>> GetRulesAsync(string profileId);
    Task<TransformationRule> AddRuleAsync(string profileId, TransformationRule rule);
    Task<TransformationRule> UpdateRuleAsync(string profileId, TransformationRule rule);
    Task DeleteRuleAsync(string profileId, string ruleId);
}
