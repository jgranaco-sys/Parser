namespace Parser.Core.Models;

public class TestResult
{
    public bool Success { get; set; }
    public double ParseTimeMs { get; set; }
    public Dictionary<string, object> MappedValues { get; set; } = new();
    public string GeneratedOutput { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
}
