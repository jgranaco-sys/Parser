using Parser.Core.Models;

namespace Parser.Core.Interfaces;

public interface ITemplateAppService
{
    Task<IReadOnlyList<OutputTemplate>> GetTemplatesAsync(string profileId);
    Task<OutputTemplate> SaveTemplateAsync(string profileId, OutputTemplate template);
    Task DeleteTemplateAsync(string profileId, string templateId);
    Task<string> RenderPreviewAsync(string profileId, string templateId, Dictionary<string, object> variables);
}
