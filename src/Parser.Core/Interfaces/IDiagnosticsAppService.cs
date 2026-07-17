using Parser.Core.Models;

namespace Parser.Core.Interfaces;

public interface IDiagnosticsAppService
{
    Task<IReadOnlyList<DiagnosticEntry>> GetDiagnosticsAsync(int maxCount = 500);
    Task<IReadOnlyList<LogEntry>> GetLogsAsync(int maxCount = 1000);
    Task ClearDiagnosticsAsync();
    Task<string> ExportLogsAsync();
}
