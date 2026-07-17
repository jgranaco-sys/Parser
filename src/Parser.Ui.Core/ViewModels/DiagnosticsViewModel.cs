using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Parser.Core.Interfaces;
using Parser.Core.Models;

namespace Parser.Ui.Core.ViewModels;

public partial class DiagnosticsViewModel : ViewModelBase
{
    private readonly IDiagnosticsAppService _diagnosticsService;
    private List<DiagnosticEntry> _allEntries = new();

    [ObservableProperty]
    private ObservableCollection<DiagnosticEntry> _entries = new();

    [ObservableProperty]
    private string _filterText = string.Empty;

    [ObservableProperty]
    private DiagnosticSeverity? _severityFilter;

    public DiagnosticsViewModel(IDiagnosticsAppService diagnosticsService)
    {
        _diagnosticsService = diagnosticsService;
    }

    public override async Task InitializeAsync()
    {
        var diagnostics = await _diagnosticsService.GetDiagnosticsAsync();
        _allEntries = diagnostics.ToList();
        ApplyFilter();
    }

    partial void OnFilterTextChanged(string value) => ApplyFilter();

    partial void OnSeverityFilterChanged(DiagnosticSeverity? value) => ApplyFilter();

    private void ApplyFilter()
    {
        IEnumerable<DiagnosticEntry> query = _allEntries;

        if (SeverityFilter.HasValue)
        {
            query = query.Where(e => e.Severity == SeverityFilter.Value);
        }

        if (!string.IsNullOrWhiteSpace(FilterText))
        {
            query = query.Where(e =>
                e.Message.Contains(FilterText, StringComparison.OrdinalIgnoreCase) ||
                e.Category.Contains(FilterText, StringComparison.OrdinalIgnoreCase));
        }

        Entries = new ObservableCollection<DiagnosticEntry>(query);
    }

    [RelayCommand]
    private async Task ClearAsync()
    {
        await _diagnosticsService.ClearDiagnosticsAsync();
        _allEntries.Clear();
        ApplyFilter();
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await InitializeAsync();
    }
}
