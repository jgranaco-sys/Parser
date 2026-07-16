namespace Parser.Core.Pipelines;

public sealed class EgressPipeline(
    ITemplateRenderer templateRenderer,
    IScadaVariableProvider scadaVariableProvider,
    IProfileRepository profileRepository,
    ILogger<EgressPipeline> logger) : IEgressPipeline
{
    public async ValueTask<EgressResult> GenerateAsync(string templateName, CancellationToken ct = default)
    {
        var template = await profileRepository.GetTemplateDefinitionAsync(templateName, ct)
            ?? throw new InvalidOperationException($"Template '{templateName}' was not found.");
        var variables = templateRenderer.DiscoverVariables(template.Content);
        var values = await scadaVariableProvider.ReadMultipleAsync(variables, ct);
        var rendered = templateRenderer.Render(template.Content, values);
        logger.LogInformation("Rendered template {TemplateName} with {VariableCount} variables.", templateName, variables.Count);
        return new EgressResult(template.Name, template.Format, rendered, Encoding.UTF8.GetBytes(rendered));
    }
}
