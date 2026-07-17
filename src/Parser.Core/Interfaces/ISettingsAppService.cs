using Parser.Core.Models;

namespace Parser.Core.Interfaces;

public interface ISettingsAppService
{
    Task<AppSettings> GetSettingsAsync();
    Task SaveSettingsAsync(AppSettings settings);
}
