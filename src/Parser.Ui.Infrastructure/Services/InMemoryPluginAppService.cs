using Parser.Core.Interfaces;
using Parser.Core.Models;

namespace Parser.Ui.Infrastructure.Services;

public class InMemoryPluginAppService : IPluginAppService
{
    private readonly List<PluginInfo> _plugins = new()
    {
        new PluginInfo { Name = "Modbus Input Adapter", Version = "1.2.0", Description = "Reads payloads from Modbus TCP devices.", IsEnabled = true },
        new PluginInfo { Name = "MQTT Output Publisher", Version = "1.0.3", Description = "Publishes transformed payloads to an MQTT broker.", IsEnabled = true },
        new PluginInfo { Name = "OPC-UA Bridge", Version = "0.9.1", Description = "Experimental OPC-UA connectivity.", IsEnabled = false, UpdateAvailable = true },
    };

    private readonly object _lock = new();

    public Task<IReadOnlyList<PluginInfo>> GetPluginsAsync()
    {
        lock (_lock)
        {
            return Task.FromResult<IReadOnlyList<PluginInfo>>(_plugins.Select(p => p.Clone()).ToList());
        }
    }

    public Task EnableAsync(string pluginId)
    {
        lock (_lock)
        {
            var plugin = _plugins.FirstOrDefault(p => p.Id == pluginId);
            if (plugin != null)
            {
                plugin.IsEnabled = true;
            }
            return Task.CompletedTask;
        }
    }

    public Task DisableAsync(string pluginId)
    {
        lock (_lock)
        {
            var plugin = _plugins.FirstOrDefault(p => p.Id == pluginId);
            if (plugin != null)
            {
                plugin.IsEnabled = false;
            }
            return Task.CompletedTask;
        }
    }
}
