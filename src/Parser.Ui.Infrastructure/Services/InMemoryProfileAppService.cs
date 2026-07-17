using Parser.Core.Interfaces;
using Parser.Core.Models;

namespace Parser.Ui.Infrastructure.Services;

public class InMemoryProfileAppService : IProfileAppService
{
    private readonly List<ParserProfile> _profiles = new();
    private readonly object _lock = new();

    public Task<IReadOnlyList<ParserProfile>> GetAllAsync()
    {
        lock (_lock)
        {
            return Task.FromResult<IReadOnlyList<ParserProfile>>(_profiles.Select(p => p.Clone()).ToList());
        }
    }

    public Task<ParserProfile?> GetByIdAsync(string id)
    {
        lock (_lock)
        {
            return Task.FromResult(_profiles.FirstOrDefault(p => p.Id == id)?.Clone());
        }
    }

    public Task<ParserProfile> CreateAsync(ParserProfile profile)
    {
        lock (_lock)
        {
            var stored = profile.Clone();
            if (string.IsNullOrEmpty(stored.Id))
            {
                stored.Id = Guid.NewGuid().ToString();
            }
            stored.CreatedAt = DateTimeOffset.UtcNow;
            stored.UpdatedAt = DateTimeOffset.UtcNow;
            _profiles.Add(stored);
            return Task.FromResult(stored.Clone());
        }
    }

    public Task<ParserProfile> UpdateAsync(ParserProfile profile)
    {
        lock (_lock)
        {
            var existing = _profiles.FirstOrDefault(p => p.Id == profile.Id);
            if (existing == null)
            {
                throw new InvalidOperationException($"Profile '{profile.Id}' not found.");
            }

            existing.Name = profile.Name;
            existing.Description = profile.Description;
            existing.InputFormat = profile.InputFormat;
            existing.IsActive = profile.IsActive;
            existing.UpdatedAt = DateTimeOffset.UtcNow;
            return Task.FromResult(existing.Clone());
        }
    }

    public Task DeleteAsync(string id)
    {
        lock (_lock)
        {
            _profiles.RemoveAll(p => p.Id == id);
            return Task.CompletedTask;
        }
    }

    public Task<ParserProfile> DuplicateAsync(string id)
    {
        lock (_lock)
        {
            var source = _profiles.FirstOrDefault(p => p.Id == id);
            if (source == null)
            {
                throw new InvalidOperationException($"Profile '{id}' not found.");
            }

            var copy = new ParserProfile
            {
                Id = Guid.NewGuid().ToString(),
                Name = source.Name + " (Copy)",
                Description = source.Description,
                InputFormat = source.InputFormat,
                IsActive = false,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            _profiles.Add(copy);
            return Task.FromResult(copy.Clone());
        }
    }

    public Task SetActiveAsync(string id)
    {
        lock (_lock)
        {
            foreach (var p in _profiles)
            {
                p.IsActive = p.Id == id;
            }
            return Task.CompletedTask;
        }
    }

    public Task<ParserProfile?> GetActiveAsync()
    {
        lock (_lock)
        {
            return Task.FromResult((_profiles.FirstOrDefault(p => p.IsActive) ?? _profiles.FirstOrDefault())?.Clone());
        }
    }
}
