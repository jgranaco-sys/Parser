using System.Text;
using Parser.Core.Interfaces;
using Parser.Core.Models;

namespace Parser.Ui.Infrastructure.Services;

public class InMemoryDiagnosticsAppService : IDiagnosticsAppService
{
    private readonly List<DiagnosticEntry> _diagnostics = new();
    private readonly List<LogEntry> _logs = new();
    private readonly object _lock = new();

    public InMemoryDiagnosticsAppService()
    {
        SeedSampleData();
    }

    private void SeedSampleData()
    {
        _diagnostics.Add(new DiagnosticEntry { Severity = DiagnosticSeverity.Info, Category = "System", Message = "Parser engine started." });
        _diagnostics.Add(new DiagnosticEntry { Severity = DiagnosticSeverity.Warning, Category = "Mapping", Message = "3 fields unmapped in active profile." });
        _logs.Add(new LogEntry { Level = LogLevel.Info, Source = "Startup", Message = "Application initialized." });
    }

    public Task<IReadOnlyList<DiagnosticEntry>> GetDiagnosticsAsync(int maxCount = 500)
    {
        lock (_lock)
        {
            return Task.FromResult<IReadOnlyList<DiagnosticEntry>>(
                _diagnostics.OrderByDescending(d => d.Timestamp).Take(maxCount).Select(d => d.Clone()).ToList());
        }
    }

    public Task<IReadOnlyList<LogEntry>> GetLogsAsync(int maxCount = 1000)
    {
        lock (_lock)
        {
            return Task.FromResult<IReadOnlyList<LogEntry>>(
                _logs.OrderByDescending(l => l.Timestamp).Take(maxCount).Select(l => l.Clone()).ToList());
        }
    }

    public Task ClearDiagnosticsAsync()
    {
        lock (_lock)
        {
            _diagnostics.Clear();
            return Task.CompletedTask;
        }
    }

    public Task<string> ExportLogsAsync()
    {
        lock (_lock)
        {
            var sb = new StringBuilder();
            foreach (var log in _logs.OrderBy(l => l.Timestamp))
            {
                sb.AppendLine($"[{log.Timestamp:O}] [{log.Level}] [{log.Source}] {log.Message}");
            }
            return Task.FromResult(sb.ToString());
        }
    }
}
