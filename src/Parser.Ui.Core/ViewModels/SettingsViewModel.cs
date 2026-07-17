using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Parser.Core.Interfaces;
using Parser.Core.Models;

namespace Parser.Ui.Core.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly ISettingsAppService _settingsService;

    [ObservableProperty]
    private AppTheme _theme = AppTheme.Dark;

    [ObservableProperty]
    private string _language = "en";

    [ObservableProperty]
    private bool _autosave = true;

    [ObservableProperty]
    private string _loggingLevel = "Info";

    [ObservableProperty]
    private bool _performanceMode;

    [ObservableProperty]
    private bool _isSaved;

    public IReadOnlyList<string> AvailableLanguages { get; } = new[] { "en", "es", "de", "fr" };

    public IReadOnlyList<string> AvailableLoggingLevels { get; } = new[] { "Debug", "Info", "Warning", "Error", "Critical" };

    public SettingsViewModel(ISettingsAppService settingsService)
    {
        _settingsService = settingsService;
    }

    public override async Task InitializeAsync()
    {
        var settings = await _settingsService.GetSettingsAsync();
        Theme = settings.Theme;
        Language = settings.Language;
        Autosave = settings.Autosave;
        LoggingLevel = settings.LoggingLevel;
        PerformanceMode = settings.PerformanceMode;
    }

    [RelayCommand]
    private void ToggleTheme()
    {
        Theme = Theme == AppTheme.Dark ? AppTheme.Light : AppTheme.Dark;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        await _settingsService.SaveSettingsAsync(new AppSettings
        {
            Theme = Theme,
            Language = Language,
            Autosave = Autosave,
            LoggingLevel = LoggingLevel,
            PerformanceMode = PerformanceMode
        });
        IsSaved = true;
    }
}
