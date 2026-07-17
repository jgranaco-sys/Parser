using System.Diagnostics;
using System.Text.Json;
using Parser.Core.Interfaces;
using Parser.Core.Models;

namespace Parser.Ui.Infrastructure.Services;

public class InMemoryTestRunAppService : ITestRunAppService
{
    private readonly IMappingAppService _mappingService;

    public InMemoryTestRunAppService(IMappingAppService mappingService)
    {
        _mappingService = mappingService;
    }

    public async Task<TestResult> RunTestAsync(string profileId, string payload, string inputFormat)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new TestResult();

        try
        {
            var mappings = await _mappingService.GetMappingsAsync(profileId);
            var values = new Dictionary<string, object>();

            if (inputFormat.Equals("JSON", StringComparison.OrdinalIgnoreCase))
            {
                using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(payload) ? "{}" : payload);
                foreach (var mapping in mappings)
                {
                    if (doc.RootElement.TryGetProperty(mapping.IncomingField, out var element))
                    {
                        values[mapping.ScadaVariable] = element.ToString() ?? string.Empty;
                    }
                    else
                    {
                        result.Errors.Add($"Field '{mapping.IncomingField}' not found in payload.");
                    }
                }
            }
            else
            {
                foreach (var mapping in mappings)
                {
                    values[mapping.ScadaVariable] = string.Empty;
                }
            }

            result.MappedValues = values;
            result.GeneratedOutput = JsonSerializer.Serialize(values, new JsonSerializerOptions { WriteIndented = true });
            result.Success = result.Errors.Count == 0;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Errors.Add(ex.Message);
        }
        finally
        {
            stopwatch.Stop();
            result.ParseTimeMs = stopwatch.Elapsed.TotalMilliseconds;
        }

        return result;
    }
}
