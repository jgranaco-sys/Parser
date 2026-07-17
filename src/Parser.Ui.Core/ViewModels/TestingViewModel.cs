using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Parser.Core.Interfaces;
using Parser.Core.Models;

namespace Parser.Ui.Core.ViewModels;

public partial class TestingViewModel : ViewModelBase
{
    private readonly ITestRunAppService _testRunService;
    private readonly IProfileAppService _profileService;

    [ObservableProperty]
    private string _currentProfileId = string.Empty;

    [ObservableProperty]
    private string _inputFormat = "JSON";

    [ObservableProperty]
    private string _payloadInput = "{\n  \"temperature\": 21.5\n}";

    [ObservableProperty]
    private TestResult? _result;

    [ObservableProperty]
    private bool _isRunning;

    public TestingViewModel(ITestRunAppService testRunService, IProfileAppService profileService)
    {
        _testRunService = testRunService;
        _profileService = profileService;
    }

    public override async Task InitializeAsync()
    {
        var profile = await _profileService.GetActiveAsync();
        if (profile != null)
        {
            CurrentProfileId = profile.Id;
            InputFormat = profile.InputFormat;
        }
    }

    [RelayCommand]
    private async Task RunTestAsync()
    {
        if (string.IsNullOrEmpty(CurrentProfileId))
        {
            return;
        }

        IsRunning = true;
        try
        {
            Result = await _testRunService.RunTestAsync(CurrentProfileId, PayloadInput, InputFormat);
        }
        finally
        {
            IsRunning = false;
        }
    }
}
