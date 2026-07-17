using Parser.Core.Models;

namespace Parser.Core.Interfaces;

public interface IMappingAppService
{
    Task<IReadOnlyList<FieldMapping>> GetMappingsAsync(string profileId);
    Task<FieldMapping> AddMappingAsync(string profileId, FieldMapping mapping);
    Task<FieldMapping> UpdateMappingAsync(string profileId, FieldMapping mapping);
    Task DeleteMappingAsync(string profileId, string mappingId);
    Task<IReadOnlyList<FieldMapping>> ImportAsync(string profileId, string json);
    Task<string> ExportAsync(string profileId);
}
