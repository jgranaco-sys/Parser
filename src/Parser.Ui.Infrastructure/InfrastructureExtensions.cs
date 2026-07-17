using Microsoft.Extensions.DependencyInjection;
using Parser.Core.Interfaces;
using Parser.Ui.Infrastructure.Services;

namespace Parser.Ui.Infrastructure;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddParserInfrastructure(this IServiceCollection services)
    {
        // Instances are created up front (rather than via type-based DI registration) so that
        // seed data can be populated before the services are exposed through the container.
        var profileService = new InMemoryProfileAppService();
        var mappingService = new InMemoryMappingAppService();

        var seeder = new SeedDataService(profileService, mappingService);
        seeder.SeedAsync().GetAwaiter().GetResult();

        services.AddSingleton<IProfileAppService>(profileService);
        services.AddSingleton<IMappingAppService>(mappingService);
        services.AddSingleton<ITemplateAppService, InMemoryTemplateAppService>();
        services.AddSingleton<ITestRunAppService, InMemoryTestRunAppService>();
        services.AddSingleton<IDiagnosticsAppService, InMemoryDiagnosticsAppService>();
        services.AddSingleton<ITransformationAppService, InMemoryTransformationAppService>();
        services.AddSingleton<IValidationAppService, InMemoryValidationAppService>();
        services.AddSingleton<IPluginAppService, InMemoryPluginAppService>();
        services.AddSingleton<ISettingsAppService, InMemorySettingsAppService>();

        return services;
    }
}
