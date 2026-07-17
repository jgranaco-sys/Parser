using Parser.Core.Models;

namespace Parser.Core.Interfaces;

public interface IProfileAppService
{
    Task<IReadOnlyList<ParserProfile>> GetAllAsync();
    Task<ParserProfile?> GetByIdAsync(string id);
    Task<ParserProfile> CreateAsync(ParserProfile profile);
    Task<ParserProfile> UpdateAsync(ParserProfile profile);
    Task DeleteAsync(string id);
    Task<ParserProfile> DuplicateAsync(string id);
    Task SetActiveAsync(string id);
    Task<ParserProfile?> GetActiveAsync();
}
