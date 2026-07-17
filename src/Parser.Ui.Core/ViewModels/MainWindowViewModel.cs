using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Parser.Core.Interfaces;
using Parser.Core.Models;
using Parser.Ui.Core.Services;

namespace Parser.Ui.Core.ViewModels;

public class NavigationItem
{
    public string Label { get; init; } = string.Empty;
    public Type ViewModelType { get; init; } = typeof(DashboardViewModel);
}

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly IProfileAppService _profileService;

    [ObservableProperty]
    private ViewModelBase? _currentPage;

    [ObservableProperty]
    private string _breadcrumb = "Dashboard";

    [ObservableProperty]
    private bool _hasUnsavedChanges;

    [ObservableProperty]
    private AppTheme _currentTheme = AppTheme.Dark;

    [ObservableProperty]
    private ObservableCollection<ParserProfile> _profiles = new();

    [ObservableProperty]
    private ParserProfile? _selectedProfile;

    [ObservableProperty]
    private NavigationItem? _selectedNavigationItem;

    public ObservableCollection<NavigationItem> NavigationItems { get; } = new()
    {
        new NavigationItem { Label = "Dashboard", ViewModelType = typeof(DashboardViewModel) },
        new NavigationItem { Label = "Input Configuration", ViewModelType = typeof(InputConfigViewModel) },
        new NavigationItem { Label = "Mapping Editor", ViewModelType = typeof(MappingEditorViewModel) },
        new NavigationItem { Label = "Transformations", ViewModelType = typeof(TransformationsViewModel) },
        new NavigationItem { Label = "Validation", ViewModelType = typeof(ValidationViewModel) },
        new NavigationItem { Label = "Template Editor", ViewModelType = typeof(TemplateEditorViewModel) },
        new NavigationItem { Label = "Output Configuration", ViewModelType = typeof(OutputConfigViewModel) },
        new NavigationItem { Label = "Testing", ViewModelType = typeof(TestingViewModel) },
        new NavigationItem { Label = "Diagnostics", ViewModelType = typeof(DiagnosticsViewModel) },
        new NavigationItem { Label = "Logs", ViewModelType = typeof(LogsViewModel) },
        new NavigationItem { Label = "Profiles", ViewModelType = typeof(ProfilesViewModel) },
        new NavigationItem { Label = "Plugin Manager", ViewModelType = typeof(PluginManagerViewModel) },
        new NavigationItem { Label = "Settings", ViewModelType = typeof(SettingsViewModel) },
    };

    public MainWindowViewModel(INavigationService navigationService, IProfileAppService profileService)
    {
        _navigationService = navigationService;
        _profileService = profileService;
        _navigationService.NavigationChanged += (_, vm) =>
        {
            CurrentPage = vm;
            Breadcrumb = vm?.Title ?? string.Empty;
        };
    }

    public override async Task InitializeAsync()
    {
        var profiles = await _profileService.GetAllAsync();
        Profiles = new ObservableCollection<ParserProfile>(profiles);
        SelectedProfile = Profiles.FirstOrDefault(p => p.IsActive) ?? Profiles.FirstOrDefault();

        _navigationService.NavigateTo<DashboardViewModel>();
        SelectedNavigationItem = NavigationItems.FirstOrDefault();
    }

    partial void OnSelectedNavigationItemChanged(NavigationItem? value)
    {
        if (value != null)
        {
            _navigationService.NavigateTo(value.ViewModelType);
        }
    }

    [RelayCommand]
    private async Task SwitchProfileAsync()
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

    [RelayCommand]
    private void ToggleTheme()
    {
        CurrentTheme = CurrentTheme == AppTheme.Dark ? AppTheme.Light : AppTheme.Dark;
    }
}
