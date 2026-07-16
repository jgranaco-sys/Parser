namespace Parser.Abstractions.Contracts;

/// <summary>
/// Provides SCADA variable read and write operations.
/// </summary>
public interface IScadaVariableProvider
{
    ValueTask<ScadaValue?> ReadVariableAsync(string name, CancellationToken ct = default);
    ValueTask WriteVariableAsync(string name, ScadaValue value, CancellationToken ct = default);
    ValueTask<IReadOnlyDictionary<string, ScadaValue>> ReadMultipleAsync(IReadOnlyCollection<string> names, CancellationToken ct = default);
    ValueTask WriteMultipleAsync(IReadOnlyDictionary<string, ScadaValue> values, CancellationToken ct = default);
}

/// <summary>
/// Parses a payload into the canonical document model.
/// </summary>
public interface IParserStrategy
{
    string Format { get; }
    ValueTask<CanonicalDocument> ParseAsync(PayloadEnvelope envelope, CancellationToken ct = default);
}

/// <summary>
/// Applies mapping rules to parsed documents.
/// </summary>
public interface IMappingEngine
{
    IReadOnlyList<MappedPoint> Map(CanonicalDocument document, MappingProfile profile);
}

/// <summary>
/// Applies transformation rules to mapped points.
/// </summary>
public interface ITransformationEngine
{
    IReadOnlyList<ScadaWriteRequest> Transform(IReadOnlyList<MappedPoint> points, TransformationProfile profile);
}

/// <summary>
/// Validates transformed points before writes.
/// </summary>
public interface IValidationEngine
{
    ValidationResult Validate(IReadOnlyList<ScadaWriteRequest> points, ValidationProfile profile, DateTimeOffset nowUtc);
}

/// <summary>
/// Renders payload templates from SCADA variable values.
/// </summary>
public interface ITemplateRenderer
{
    IReadOnlyCollection<string> DiscoverVariables(string templateContent);
    string Render(string templateContent, IReadOnlyDictionary<string, ScadaValue> values);
}

/// <summary>
/// Provides runtime configuration profiles.
/// </summary>
public interface IProfileRepository
{
    ValueTask InitializeAsync(CancellationToken ct = default);
    ValueTask SeedSampleDataAsync(CancellationToken ct = default);
    ValueTask<IngressProfile?> GetIngressProfileAsync(string name, CancellationToken ct = default);
    ValueTask<TransformationProfile?> GetTransformationProfileAsync(string name, CancellationToken ct = default);
    ValueTask<ValidationProfile?> GetValidationProfileAsync(string name, CancellationToken ct = default);
    ValueTask<TemplateDefinition?> GetTemplateDefinitionAsync(string name, CancellationToken ct = default);
    ValueTask<IReadOnlyCollection<TemplateDefinition>> GetTemplatesAsync(CancellationToken ct = default);
    ValueTask ExportAsync(Stream destination, CancellationToken ct = default);
    ValueTask ImportAsync(Stream source, CancellationToken ct = default);
}

/// <summary>
/// Registers plugin services into dependency injection.
/// </summary>
public interface IPluginModule
{
    void Register(IServiceCollection services);
}

/// <summary>
/// Handles dead-letter payloads.
/// </summary>
public interface IDeadLetterSink
{
    ValueTask WriteAsync(DeadLetterMessage message, CancellationToken ct = default);
}
