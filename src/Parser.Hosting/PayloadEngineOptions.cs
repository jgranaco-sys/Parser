namespace Parser.Hosting;

/// <summary>
/// Core hosting settings for the payload engine.
/// </summary>
public sealed class PayloadEngineOptions
{
    public string SqliteConnectionString { get; set; } = "Data Source=parser.db";
    public string PluginDirectory { get; set; } = "plugins";
    public int ChannelCapacity { get; set; } = 128;
    public int DegreeOfParallelism { get; set; } = 2;
}
