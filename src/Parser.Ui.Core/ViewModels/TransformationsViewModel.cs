using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Parser.Core.Interfaces;
using Parser.Core.Models;

namespace Parser.Ui.Core.ViewModels;

public partial class TransformationsViewModel : ViewModelBase
{
    private readonly ITransformationAppService _transformationService;
    private readonly IProfileAppService _profileService;

    [ObservableProperty]
    private ObservableCollection<TransformationRule> _rules = new();

    [ObservableProperty]
    private TransformationRule? _selectedRule;

    [ObservableProperty]
    private string _currentProfileId = string.Empty;

    public TransformationsViewModel(ITransformationAppService transformationService, IProfileAppService profileService)
    {
        _transformationService = transformationService;
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
        var rules = await _transformationService.GetRulesAsync(profile.Id);
        Rules = new ObservableCollection<TransformationRule>(rules);
    }

    [RelayCommand]
    private async Task AddRuleAsync()
    {
        if (string.IsNullOrEmpty(CurrentProfileId))
        {
            return;
        }

        var rule = new TransformationRule
        {
            Name = "New Rule",
            Type = TransformationType.String
        };
        var added = await _transformationService.AddRuleAsync(CurrentProfileId, rule);
        Rules.Add(added);
        SelectedRule = added;
    }

    [RelayCommand]
    private async Task DeleteRuleAsync()
    {
        if (SelectedRule == null || string.IsNullOrEmpty(CurrentProfileId))
        {
            return;
        }

        await _transformationService.DeleteRuleAsync(CurrentProfileId, SelectedRule.Id);
        Rules.Remove(SelectedRule);
        SelectedRule = null;
    }
}
