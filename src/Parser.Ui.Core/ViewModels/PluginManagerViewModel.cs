using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Parser.Core.Interfaces;
using Parser.Core.Models;

namespace Parser.Ui.Core.ViewModels;

public partial class PluginManagerViewModel : ViewModelBase
{
    private readonly IPluginAppService _pluginService;

    [ObservableProperty]
    private ObservableCollection<PluginInfo> _plugins = new();

    [ObservableProperty]
    private PluginInfo? _selectedPlugin;

    public PluginManagerViewModel(IPluginAppService pluginService)
    {
        _pluginService = pluginService;
    }

    public override async Task InitializeAsync()
    {
        var plugins = await _pluginService.GetPluginsAsync();
        Plugins = new ObservableCollection<PluginInfo>(plugins);
    }

    [RelayCommand]
    private async Task EnableAsync(PluginInfo? plugin)
    {
        plugin ??= SelectedPlugin;
        if (plugin == null)
        {
            return;
        }

        await _pluginService.EnableAsync(plugin.Id);
        plugin.IsEnabled = true;
    }

    [RelayCommand]
    private async Task DisableAsync(PluginInfo? plugin)
    {
        plugin ??= SelectedPlugin;
        if (plugin == null)
        {
            return;
        }

        await _pluginService.DisableAsync(plugin.Id);
        plugin.IsEnabled = false;
    }
}
