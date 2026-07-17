using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Parser.Core.Interfaces;
using Parser.Core.Models;

namespace Parser.Ui.Core.ViewModels;

public partial class MappingEditorViewModel : ViewModelBase
{
    private readonly IMappingAppService _mappingService;
    private readonly IProfileAppService _profileService;

    [ObservableProperty]
    private ObservableCollection<FieldMapping> _mappings = new();

    [ObservableProperty]
    private FieldMapping? _selectedMapping;

    [ObservableProperty]
    private string _filterText = string.Empty;

    [ObservableProperty]
    private string _currentProfileId = string.Empty;

    public MappingEditorViewModel(IMappingAppService mappingService, IProfileAppService profileService)
    {
        _mappingService = mappingService;
        _profileService = profileService;
    }

    public override async Task InitializeAsync()
    {
        var profile = await _profileService.GetActiveAsync();
        if (profile == null)
        {
            return;
        }

        CurrentProfileId = profile.Id;
        var mappings = await _mappingService.GetMappingsAsync(profile.Id);
        Mappings = new ObservableCollection<FieldMapping>(mappings);
    }

    [RelayCommand]
    private async Task AddMappingAsync()
    {
        if (string.IsNullOrEmpty(CurrentProfileId))
        {
            return;
        }

        var mapping = new FieldMapping
        {
            IncomingField = "newField",
            ScadaVariable = string.Empty,
            DataType = "string",
            Status = MappingStatus.Missing
        };
        var added = await _mappingService.AddMappingAsync(CurrentProfileId, mapping);
        Mappings.Add(added);
        SelectedMapping = added;
    }

    [RelayCommand]
    private async Task UpdateMappingAsync(FieldMapping? mapping)
    {
        mapping ??= SelectedMapping;
        if (mapping == null || string.IsNullOrEmpty(CurrentProfileId))
        {
            return;
        }

        await _mappingService.UpdateMappingAsync(CurrentProfileId, mapping);
    }

    [RelayCommand]
    private async Task DeleteMappingAsync()
    {
        if (SelectedMapping == null || string.IsNullOrEmpty(CurrentProfileId))
        {
            return;
        }

        await _mappingService.DeleteMappingAsync(CurrentProfileId, SelectedMapping.Id);
        Mappings.Remove(SelectedMapping);
        SelectedMapping = null;
    }
}
