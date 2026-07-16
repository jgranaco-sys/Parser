namespace Parser.Plugins.BuiltIn;

/// <summary>
/// Registers built-in parser, mapping, transformation, validation, and template services.
/// </summary>
public sealed class BuiltInPluginModule : IPluginModule
{
    public void Register(IServiceCollection services)
    {
        services.AddSingleton<IParserStrategy, JsonPayloadParser>();
        services.AddSingleton<IParserStrategy, CsvPayloadParser>();
        services.AddSingleton<IParserStrategy, TextPayloadParser>();
        services.AddSingleton<IMappingEngine, MappingEngine>();
        services.AddSingleton<ITransformationEngine, TransformationEngine>();
        services.AddSingleton<IValidationEngine, ValidationEngine>();
        services.AddSingleton<ITemplateRenderer, TemplateRenderer>();
    }
}
