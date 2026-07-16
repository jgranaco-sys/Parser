using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Parser.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(builder =>
    {
        builder.SetBasePath(AppContext.BaseDirectory);
        builder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
    })
    .ConfigureServices((context, services) =>
    {
        services.AddPayloadEngine(context.Configuration);
        services.AddSingleton<DemoRunner>();
    })
    .Build();

using (host)
{
    await host.StartAsync();
    var runner = host.Services.GetRequiredService<DemoRunner>();
    await runner.RunAsync();
    await host.StopAsync();
}
