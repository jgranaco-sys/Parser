using CommunityToolkit.Mvvm.ComponentModel;
using Parser.Core.Interfaces;
using Parser.Core.Models;

namespace Parser.Ui.Core.ViewModels;

public partial class DashboardViewModel : ViewModelBase
{
    private readonly IDiagnosticsAppService _diagnosticsService;
    private readonly IProfileAppService _profileService;

    [ObservableProperty]
    private string _status = "Idle";

    [ObservableProperty]
    private string _activeProfileName = "None";

    [ObservableProperty]
    private int _mappedVariableCount;

    [ObservableProperty]
    private int _errorCount;

    [ObservableProperty]
    private double _messagesPerSecond;

    public DashboardViewModel(IDiagnosticsAppService diagnosticsService, IProfileAppService profileService)
    {
        _diagnosticsService = diagnosticsService;
        _profileService = profileService;
    }

    public override async Task InitializeAsync()
    {
        var profile = await _profileService.GetActiveAsync();
        ActiveProfileName = profile?.Name ?? "None";

        var diagnostics = await _diagnosticsService.GetDiagnosticsAsync();
        ErrorCount = diagnostics.Count(d => d.Severity == DiagnosticSeverity.Error);

        Status = ErrorCount > 0 ? "Degraded" : "Running";
        MessagesPerSecond = 12.5;
        MappedVariableCount = 8;
    }
}
