using Parser.Core.Interfaces;
using Parser.Core.Models;

namespace Parser.Ui.Infrastructure.Services;

public class InMemoryTemplateAppService : ITemplateAppService
{
    private readonly List<OutputTemplate> _templates = new();
    private readonly object _lock = new();

    public Task<IReadOnlyList<OutputTemplate>> GetTemplatesAsync(string profileId)
    {
        lock (_lock)
        {
            return Task.FromResult<IReadOnlyList<OutputTemplate>>(
                _templates.Where(t => t.ProfileId == profileId).Select(t => t.Clone()).ToList());
        }
    }

    public Task<OutputTemplate> SaveTemplateAsync(string profileId, OutputTemplate template)
    {
        lock (_lock)
        {
            var existing = _templates.FirstOrDefault(t => t.Id == template.Id && t.ProfileId == profileId);
            if (existing != null)
            {
                existing.Name = template.Name;
                existing.Format = template.Format;
                existing.Content = template.Content;
                return Task.FromResult(existing.Clone());
            }

            var stored = template.Clone();
            stored.ProfileId = profileId;
            if (string.IsNullOrEmpty(stored.Id))
            {
                stored.Id = Guid.NewGuid().ToString();
            }
            _templates.Add(stored);
            return Task.FromResult(stored.Clone());
        }
    }

    public Task DeleteTemplateAsync(string profileId, string templateId)
    {
        lock (_lock)
        {
            _templates.RemoveAll(t => t.Id == templateId && t.ProfileId == profileId);
            return Task.CompletedTask;
        }
    }

    public Task<string> RenderPreviewAsync(string profileId, string templateId, Dictionary<string, object> variables)
    {
        lock (_lock)
        {
            var template = _templates.FirstOrDefault(t => t.Id == templateId && t.ProfileId == profileId);
            var content = template?.Content ?? string.Empty;

            foreach (var kvp in variables)
            {
                content = content.Replace("{{" + kvp.Key + "}}", kvp.Value?.ToString() ?? string.Empty);
            }

            return Task.FromResult(content);
        }
    }
}
