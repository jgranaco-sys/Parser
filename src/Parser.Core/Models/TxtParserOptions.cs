namespace Parser.Core.Models;

public enum TxtParseMode
{
    FixedWidth,
    Delimited,
    Regex,
    KeyValue
}

public class TxtParserOptions
{
    public TxtParseMode Mode { get; set; } = TxtParseMode.Delimited;
    public string Encoding { get; set; } = "UTF-8";
    public string Pattern { get; set; } = string.Empty;
}
