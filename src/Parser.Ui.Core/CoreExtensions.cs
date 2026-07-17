using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Parser.Ui.Core.Services;
using Parser.Ui.Core.ViewModels;

namespace Parser.Ui.Core;

public static class CoreExtensions
{
    public static IServiceCollection AddParserUiCore(this IServiceCollection services)
    {
        services.AddSingleton<INavigationService>(provider =>
            new NavigationService(
                type => (ViewModelBase)provider.GetRequiredService(type),
                provider.GetService<ILogger<NavigationService>>()));

        services.AddSingleton<MainWindowViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<InputConfigViewModel>();
        services.AddTransient<MappingEditorViewModel>();
        services.AddTransient<TransformationsViewModel>();
        services.AddTransient<ValidationViewModel>();
        services.AddTransient<TemplateEditorViewModel>();
        services.AddTransient<OutputConfigViewModel>();
        services.AddTransient<TestingViewModel>();
        services.AddTransient<DiagnosticsViewModel>();
        services.AddTransient<LogsViewModel>();
        services.AddTransient<ProfilesViewModel>();
        services.AddTransient<PluginManagerViewModel>();
        services.AddTransient<SettingsViewModel>();

        return services;
    }
}
