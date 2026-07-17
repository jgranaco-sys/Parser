using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Parser.Core.Interfaces;
using Parser.Core.Models;

namespace Parser.Ui.Core.ViewModels;

public partial class InputConfigViewModel : ViewModelBase
{
    private readonly IProfileAppService _profileService;

    [ObservableProperty]
    private string _currentProfileId = string.Empty;

    [ObservableProperty]
    private string _selectedFormat = "JSON";

    [ObservableProperty]
    private JsonParserOptions _jsonOptions = new();

    [ObservableProperty]
    private CsvParserOptions _csvOptions = new();

    [ObservableProperty]
    private TxtParserOptions _txtOptions = new();

    public IReadOnlyList<string> AvailableFormats { get; } = new[] { "JSON", "CSV", "TXT" };

    public InputConfigViewModel(IProfileAppService profileService)
    {
        _profileService = profileService;
    }

    public override async Task InitializeAsync()
    {
        var profile = await _profileService.GetActiveAsync();
        if (profile != null)
        {
            CurrentProfileId = profile.Id;
            SelectedFormat = profile.InputFormat;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrEmpty(CurrentProfileId))
        {
            return;
        }

        var profile = await _profileService.GetByIdAsync(CurrentProfileId);
        if (profile == null)
        {
            return;
        }

        profile.InputFormat = SelectedFormat;
        await _profileService.UpdateAsync(profile);
    }
}
