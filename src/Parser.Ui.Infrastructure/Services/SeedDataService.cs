using Parser.Core.Interfaces;
using Parser.Core.Models;

namespace Parser.Ui.Infrastructure.Services;

/// <summary>
/// Seeds the in-memory services with sample profiles and mappings so the UI has
/// realistic data to display on first run.
/// </summary>
public class SeedDataService
{
    private readonly IProfileAppService _profileService;
    private readonly IMappingAppService _mappingService;

    public SeedDataService(IProfileAppService profileService, IMappingAppService mappingService)
    {
        _profileService = profileService;
        _mappingService = mappingService;
    }

    public async Task SeedAsync()
    {
        var weather = await _profileService.CreateAsync(new ParserProfile
        {
            Name = "Weather Provider",
            Description = "Ingests weather station telemetry as JSON.",
            InputFormat = "JSON"
        });
        await AddMappingsAsync(weather.Id, new (string, string, string, MappingStatus)[]
        {
            ("temperature", "Weather.Temperature", "double", MappingStatus.Mapped),
            ("humidity", "Weather.Humidity", "double", MappingStatus.Mapped),
            ("windSpeed", "Weather.WindSpeed", "double", MappingStatus.Partial),
            ("pressure", "Weather.Pressure", "double", MappingStatus.Missing),
        });

        var energy = await _profileService.CreateAsync(new ParserProfile
        {
            Name = "Energy Meter",
            Description = "Ingests smart meter readings as JSON.",
            InputFormat = "JSON"
        });
        await AddMappingsAsync(energy.Id, new (string, string, string, MappingStatus)[]
        {
            ("voltage", "Meter.Voltage", "double", MappingStatus.Mapped),
            ("current", "Meter.Current", "double", MappingStatus.Mapped),
            ("power", "Meter.Power", "double", MappingStatus.Mapped),
            ("energy", "Meter.Energy", "double", MappingStatus.Partial),
        });

        var der = await _profileService.CreateAsync(new ParserProfile
        {
            Name = "DER Gateway",
            Description = "Ingests distributed energy resource telemetry as JSON.",
            InputFormat = "JSON"
        });
        await AddMappingsAsync(der.Id, new (string, string, string, MappingStatus)[]
        {
            ("activePower", "DER.ActivePower", "double", MappingStatus.Mapped),
            ("reactivePower", "DER.ReactivePower", "double", MappingStatus.Mapped),
            ("frequency", "DER.Frequency", "double", MappingStatus.Mapped),
            ("soc", "DER.SOC", "double", MappingStatus.Missing),
        });

        await _profileService.SetActiveAsync(weather.Id);
    }

    private async Task AddMappingsAsync(string profileId, (string incoming, string scada, string type, MappingStatus status)[] fields)
    {
        foreach (var field in fields)
        {
            await _mappingService.AddMappingAsync(profileId, new FieldMapping
            {
                IncomingField = field.incoming,
                ScadaVariable = field.scada,
                DataType = field.type,
                Status = field.status
            });
        }
    }
}
