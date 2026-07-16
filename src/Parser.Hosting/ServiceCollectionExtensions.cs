namespace Parser.Hosting;

/// <summary>
/// Configures the parser engine dependency graph.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPayloadEngine(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddLogging(builder => builder.AddSimpleConsole(options =>
        {
            options.SingleLine = true;
            options.TimestampFormat = "HH:mm:ss ";
        }));

        services.Configure<PayloadEngineOptions>(configuration.GetSection("PayloadEngine"));
        services.Configure<MqttOptions>(configuration.GetSection("Mqtt"));

        services.AddSingleton<EngineMetrics>();
        services.AddSingleton<IScadaVariableProvider, InMemoryScadaVariableProvider>();
        services.AddSingleton<IDeadLetterSink, InMemoryDeadLetterSink>();
        services.AddSingleton<IProfileRepository>(serviceProvider =>
        {
            var options = configuration.GetSection("PayloadEngine").Get<PayloadEngineOptions>() ?? new PayloadEngineOptions();
            var logger = serviceProvider.GetRequiredService<ILogger<SqliteProfileRepository>>();
            return new SqliteProfileRepository(options.SqliteConnectionString, logger);
        });

        new BuiltInPluginModule().Register(services);
        var pluginDirectory = ResolvePluginDirectory(configuration);
        PluginLoader.LoadFromDirectory(services, pluginDirectory);

        services.AddSingleton<IIngressPipeline, IngressPipeline>();
        services.AddSingleton<IEgressPipeline, EgressPipeline>();
        services.AddSingleton(serviceProvider =>
        {
            var options = configuration.GetSection("PayloadEngine").Get<PayloadEngineOptions>() ?? new PayloadEngineOptions();
            return new IngressChannelProcessor(
                serviceProvider.GetRequiredService<IIngressPipeline>(),
                serviceProvider.GetRequiredService<ILogger<IngressChannelProcessor>>(),
                options.ChannelCapacity,
                options.DegreeOfParallelism);
        });
        services.AddHostedService<MqttAdapterHostedService>();
        services.AddHostedService<PayloadEngineInitializationService>();
        return services;
    }

    public static string ResolvePluginDirectory(IConfiguration configuration)
    {
        var configured = configuration.GetSection("PayloadEngine").Get<PayloadEngineOptions>()?.PluginDirectory ?? "plugins";
        return Path.IsPathRooted(configured)
            ? configured
            : Path.Combine(AppContext.BaseDirectory, configured);
    }
}
