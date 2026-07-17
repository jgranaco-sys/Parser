namespace Parser.Core.Models;

public enum AppTheme
{
    Light,
    Dark
}

public class AppSettings
{
    public AppTheme Theme { get; set; } = AppTheme.Dark;
    public string Language { get; set; } = "en";
    public bool Autosave { get; set; } = true;
    public string LoggingLevel { get; set; } = "Info";
    public bool PerformanceMode { get; set; }
}
