namespace Parser.SamplePlugin;

/// <summary>
/// Example external plugin loaded from the plugins folder.
/// </summary>
public sealed class SamplePluginModule : IPluginModule
{
    public void Register(IServiceCollection services)
    {
        services.AddSingleton<IParserStrategy, SemicolonTextParser>();
    }
}
