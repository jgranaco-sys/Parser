namespace Parser.Core.Models;

public class PluginInfo
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0.0";
    public string Description { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public bool IsInstalled { get; set; } = true;
    public bool UpdateAvailable { get; set; }

    /// <summary>Creates a shallow copy so callers cannot mutate shared/stored state.</summary>
    public PluginInfo Clone() => (PluginInfo)MemberwiseClone();
}
