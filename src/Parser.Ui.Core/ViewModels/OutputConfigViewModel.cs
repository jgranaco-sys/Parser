using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Parser.Core.Interfaces;
using Parser.Core.Models;

namespace Parser.Ui.Core.ViewModels;

public partial class OutputConfigViewModel : ViewModelBase
{
    private readonly IProfileAppService _profileService;

    [ObservableProperty]
    private string _currentProfileId = string.Empty;

    [ObservableProperty]
    private OutputConfiguration _configuration = new();

    public OutputConfigViewModel(IProfileAppService profileService)
    {
        _profileService = profileService;
    }

    public override async Task InitializeAsync()
    {
        var profile = await _profileService.GetActiveAsync();
        if (profile != null)
        {
            CurrentProfileId = profile.Id;
        }
    }

    [RelayCommand]
    private Task SaveAsync()
    {
        // Output configuration is held in-memory for now; persisted per-profile
        // storage can be layered in once a durable store is introduced.
        return Task.CompletedTask;
    }
}
