namespace Parser.Abstractions.Contracts;

/// <summary>
/// Processes inbound payloads.
/// </summary>
public interface IIngressPipeline
{
    ValueTask<IngressResult> ProcessAsync(string profileName, PayloadEnvelope envelope, CancellationToken ct = default);
}

/// <summary>
/// Generates outbound payloads.
/// </summary>
public interface IEgressPipeline
{
    ValueTask<EgressResult> GenerateAsync(string templateName, CancellationToken ct = default);
}
