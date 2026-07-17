namespace Parser.Core.Models;

public class InputConfiguration
{
    public string Format { get; set; } = "JSON";
    public JsonParserOptions JsonOptions { get; set; } = new();
    public CsvParserOptions CsvOptions { get; set; } = new();
    public TxtParserOptions TxtOptions { get; set; } = new();
}
