using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Parser.Core.Interfaces;
using Parser.Core.Models;

namespace Parser.Ui.Core.ViewModels;

public partial class ValidationViewModel : ViewModelBase
{
    private readonly IValidationAppService _validationService;
    private readonly IProfileAppService _profileService;

    [ObservableProperty]
    private ObservableCollection<ValidationRule> _rules = new();

    [ObservableProperty]
    private ValidationRule? _selectedRule;

    [ObservableProperty]
    private string _currentProfileId = string.Empty;

    public ValidationViewModel(IValidationAppService validationService, IProfileAppService profileService)
    {
        _validationService = validationService;
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
        var rules = await _validationService.GetRulesAsync(profile.Id);
        Rules = new ObservableCollection<ValidationRule>(rules);
    }

    [RelayCommand]
    private async Task AddRuleAsync()
    {
        if (string.IsNullOrEmpty(CurrentProfileId))
        {
            return;
        }

        var rule = new ValidationRule
        {
            FieldName = "newField",
            RuleType = ValidationRuleType.Required
        };
        var added = await _validationService.AddRuleAsync(CurrentProfileId, rule);
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

        await _validationService.DeleteRuleAsync(CurrentProfileId, SelectedRule.Id);
        Rules.Remove(SelectedRule);
        SelectedRule = null;
    }
}
