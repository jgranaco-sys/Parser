namespace Parser.Abstractions;

public sealed record MappingRule(
    string SourcePath,
    string TargetVariable,
    bool Optional = false,
    string? DefaultValue = null,
    IReadOnlyDictionary<string, string>? Metadata = null);

public sealed record MappingProfile(
    string Name,
    string Format,
    IReadOnlyList<MappingRule> Rules,
    int Version = 1,
    DateTimeOffset? UpdatedUtc = null);

public sealed record TransformationStep(
    string Kind,
    string? Argument = null,
    IReadOnlyDictionary<string, string>? Map = null);

public sealed record TransformationRule(
    string TargetVariable,
    IReadOnlyList<TransformationStep> Steps);

public sealed record TransformationProfile(
    string Name,
    IReadOnlyList<TransformationRule> Rules,
    int Version = 1,
    DateTimeOffset? UpdatedUtc = null);

public sealed record ValidationRule(
    string TargetVariable,
    bool Required = false,
    string? ExpectedType = null,
    double? Minimum = null,
    double? Maximum = null,
    int? TimestampWindowSeconds = null);

public sealed record ValidationProfile(
    string Name,
    PartialUpdateMode PartialUpdateMode,
    IReadOnlyList<ValidationRule> Rules,
    int Version = 1,
    DateTimeOffset? UpdatedUtc = null);

public sealed record IngressProfile(
    string Name,
    string Format,
    MappingProfile Mapping,
    string TransformationProfileName,
    string ValidationProfileName,
    bool TracePayloads = false,
    string? DeadLetterProfile = null,
    int DegreeOfParallelism = 2,
    int ChannelCapacity = 128,
    int RetryCount = 2,
    int Version = 1,
    DateTimeOffset? UpdatedUtc = null);

public sealed record TemplateDefinition(
    string Name,
    string Format,
    string Content,
    int Version = 1,
    DateTimeOffset? UpdatedUtc = null);

public sealed record ConfigurationExport(
    IReadOnlyList<IngressProfile> IngressProfiles,
    IReadOnlyList<TransformationProfile> TransformationProfiles,
    IReadOnlyList<ValidationProfile> ValidationProfiles,
    IReadOnlyList<TemplateDefinition> Templates);
