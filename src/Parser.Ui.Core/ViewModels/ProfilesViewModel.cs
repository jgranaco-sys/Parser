using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Parser.Core.Interfaces;
using Parser.Core.Models;

namespace Parser.Ui.Core.ViewModels;

public partial class ProfilesViewModel : ViewModelBase
{
    private readonly IProfileAppService _profileService;

    [ObservableProperty]
    private ObservableCollection<ParserProfile> _profiles = new();

    [ObservableProperty]
    private ParserProfile? _selectedProfile;

    [ObservableProperty]
    private string _newProfileName = string.Empty;

    public ProfilesViewModel(IProfileAppService profileService)
    {
        _profileService = profileService;
    }

    public override async Task InitializeAsync()
    {
        await RefreshAsync();
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        var profiles = await _profileService.GetAllAsync();
        Profiles = new ObservableCollection<ParserProfile>(profiles);
        SelectedProfile = Profiles.FirstOrDefault(p => p.IsActive) ?? Profiles.FirstOrDefault();
    }

    [RelayCommand]
    private async Task CreateAsync()
    {
        var name = string.IsNullOrWhiteSpace(NewProfileName) ? "New Profile" : NewProfileName;
        var created = await _profileService.CreateAsync(new ParserProfile { Name = name });
        Profiles.Add(created);
        SelectedProfile = created;
        NewProfileName = string.Empty;
    }

    [RelayCommand]
    private async Task UpdateAsync()
    {
        if (SelectedProfile == null)
        {
            return;
        }

        await _profileService.UpdateAsync(SelectedProfile);
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (SelectedProfile == null)
        {
            return;
        }

        await _profileService.DeleteAsync(SelectedProfile.Id);
        Profiles.Remove(SelectedProfile);
        SelectedProfile = Profiles.FirstOrDefault();
    }

    [RelayCommand]
    private async Task DuplicateAsync()
    {
        if (SelectedProfile == null)
        {
            return;
        }

        var duplicate = await _profileService.DuplicateAsync(SelectedProfile.Id);
        Profiles.Add(duplicate);
        SelectedProfile = duplicate;
    }

    [RelayCommand]
    private async Task SetActiveAsync()
    {
        if (SelectedProfile == null)
        {
            return;
        }

        await _profileService.SetActiveAsync(SelectedProfile.Id);
        foreach (var profile in Profiles)
        {
            profile.IsActive = profile.Id == SelectedProfile.Id;
        }
    }
}
