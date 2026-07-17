namespace Parser.Core.Models;

public class JsonParserOptions
{
    public bool Indented { get; set; }
    public bool IgnoreCase { get; set; } = true;
    public bool IgnoreNulls { get; set; }
    public string ArrayHandling { get; set; } = "Flatten";
    public string NestedObjectHandling { get; set; } = "Flatten";
}
