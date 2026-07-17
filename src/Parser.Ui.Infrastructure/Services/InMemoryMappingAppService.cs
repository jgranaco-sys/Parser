using System.Text.Json;
using Parser.Core.Interfaces;
using Parser.Core.Models;

namespace Parser.Ui.Infrastructure.Services;

public class InMemoryMappingAppService : IMappingAppService
{
    private readonly List<FieldMapping> _mappings = new();
    private readonly object _lock = new();

    public Task<IReadOnlyList<FieldMapping>> GetMappingsAsync(string profileId)
    {
        lock (_lock)
        {
            return Task.FromResult<IReadOnlyList<FieldMapping>>(
                _mappings.Where(m => m.ProfileId == profileId).Select(m => m.Clone()).ToList());
        }
    }

    public Task<FieldMapping> AddMappingAsync(string profileId, FieldMapping mapping)
    {
        lock (_lock)
        {
            var stored = mapping.Clone();
            if (string.IsNullOrEmpty(stored.Id))
            {
                stored.Id = Guid.NewGuid().ToString();
            }
            stored.ProfileId = profileId;
            _mappings.Add(stored);
            return Task.FromResult(stored.Clone());
        }
    }

    public Task<FieldMapping> UpdateMappingAsync(string profileId, FieldMapping mapping)
    {
        lock (_lock)
        {
            var existing = _mappings.FirstOrDefault(m => m.Id == mapping.Id && m.ProfileId == profileId);
            if (existing == null)
            {
                throw new InvalidOperationException($"Mapping '{mapping.Id}' not found.");
            }

            existing.IncomingField = mapping.IncomingField;
            existing.ScadaVariable = mapping.ScadaVariable;
            existing.DataType = mapping.DataType;
            existing.Transform = mapping.Transform;
            existing.Status = mapping.Status;
            return Task.FromResult(existing.Clone());
        }
    }

    public Task DeleteMappingAsync(string profileId, string mappingId)
    {
        lock (_lock)
        {
            _mappings.RemoveAll(m => m.Id == mappingId && m.ProfileId == profileId);
            return Task.CompletedTask;
        }
    }

    public Task<IReadOnlyList<FieldMapping>> ImportAsync(string profileId, string json)
    {
        lock (_lock)
        {
            var imported = JsonSerializer.Deserialize<List<FieldMapping>>(json) ?? new List<FieldMapping>();
            var result = new List<FieldMapping>();
            foreach (var mapping in imported)
            {
                var stored = mapping.Clone();
                stored.ProfileId = profileId;
                if (string.IsNullOrEmpty(stored.Id))
                {
                    stored.Id = Guid.NewGuid().ToString();
                }
                _mappings.Add(stored);
                result.Add(stored.Clone());
            }
            return Task.FromResult<IReadOnlyList<FieldMapping>>(result);
        }
    }

    public Task<string> ExportAsync(string profileId)
    {
        lock (_lock)
        {
            var toExport = _mappings.Where(m => m.ProfileId == profileId).ToList();
            var json = JsonSerializer.Serialize(toExport, new JsonSerializerOptions { WriteIndented = true });
            return Task.FromResult(json);
        }
    }
}
