using Parser.Core.Interfaces;
using Parser.Core.Models;

namespace Parser.Ui.Infrastructure.Services;

public class InMemorySettingsAppService : ISettingsAppService
{
    private AppSettings _settings = new();
    private readonly object _lock = new();

    public Task<AppSettings> GetSettingsAsync()
    {
        lock (_lock)
        {
            // Return a copy so callers can't mutate shared state without
            // going through SaveSettingsAsync.
            var copy = new AppSettings
            {
                Theme = _settings.Theme,
                Language = _settings.Language,
                Autosave = _settings.Autosave,
                LoggingLevel = _settings.LoggingLevel,
                PerformanceMode = _settings.PerformanceMode
            };
            return Task.FromResult(copy);
        }
    }

    public Task SaveSettingsAsync(AppSettings settings)
    {
        lock (_lock)
        {
            _settings = new AppSettings
            {
                Theme = settings.Theme,
                Language = settings.Language,
                Autosave = settings.Autosave,
                LoggingLevel = settings.LoggingLevel,
                PerformanceMode = settings.PerformanceMode
            };
            return Task.CompletedTask;
        }
    }
}
