using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Parser.Core.Interfaces;
using Parser.Core.Models;

namespace Parser.Ui.Core.ViewModels;

public partial class LogsViewModel : ViewModelBase
{
    private readonly IDiagnosticsAppService _diagnosticsService;
    private List<LogEntry> _allEntries = new();

    [ObservableProperty]
    private ObservableCollection<LogEntry> _entries = new();

    [ObservableProperty]
    private string _filterText = string.Empty;

    [ObservableProperty]
    private LogLevel? _levelFilter;

    [ObservableProperty]
    private string _exportedLogs = string.Empty;

    public LogsViewModel(IDiagnosticsAppService diagnosticsService)
    {
        _diagnosticsService = diagnosticsService;
    }

    public override async Task InitializeAsync()
    {
        var logs = await _diagnosticsService.GetLogsAsync();
        _allEntries = logs.ToList();
        ApplyFilter();
    }

    partial void OnFilterTextChanged(string value) => ApplyFilter();

    partial void OnLevelFilterChanged(LogLevel? value) => ApplyFilter();

    private void ApplyFilter()
    {
        IEnumerable<LogEntry> query = _allEntries;

        if (LevelFilter.HasValue)
        {
            query = query.Where(e => e.Level == LevelFilter.Value);
        }

        if (!string.IsNullOrWhiteSpace(FilterText))
        {
            query = query.Where(e =>
                e.Message.Contains(FilterText, StringComparison.OrdinalIgnoreCase) ||
                e.Source.Contains(FilterText, StringComparison.OrdinalIgnoreCase));
        }

        Entries = new ObservableCollection<LogEntry>(query);
    }

    [RelayCommand]
    private async Task ExportAsync()
    {
        ExportedLogs = await _diagnosticsService.ExportLogsAsync();
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await InitializeAsync();
    }
}
