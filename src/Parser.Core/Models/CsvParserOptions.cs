namespace Parser.Core.Models;

public class CsvParserOptions
{
    public string Delimiter { get; set; } = ",";
    public bool HasHeader { get; set; } = true;
    public string Encoding { get; set; } = "UTF-8";
    public char QuoteChar { get; set; } = '"';
    public char EscapeChar { get; set; } = '\\';
    public string DecimalSeparator { get; set; } = ".";
    public string DateFormat { get; set; } = "yyyy-MM-dd";
}
