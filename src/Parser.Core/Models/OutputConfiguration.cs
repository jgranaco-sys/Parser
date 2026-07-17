namespace Parser.Core.Models;

public class OutputConfiguration
{
    public string Topic { get; set; } = string.Empty;
    public int QoS { get; set; } = 1;
    public bool Retain { get; set; }
    public int PublishInterval { get; set; } = 1000;
    public bool Compression { get; set; }
    public string Encoding { get; set; } = "UTF-8";
}
