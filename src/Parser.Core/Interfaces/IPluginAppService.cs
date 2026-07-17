using Parser.Core.Models;

namespace Parser.Core.Interfaces;

public interface IPluginAppService
{
    Task<IReadOnlyList<PluginInfo>> GetPluginsAsync();
    Task EnableAsync(string pluginId);
    Task DisableAsync(string pluginId);
}
