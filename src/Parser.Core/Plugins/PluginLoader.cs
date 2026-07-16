namespace Parser.Core.Plugins;

public static class PluginLoader
{
    public static IReadOnlyList<string> LoadFromDirectory(IServiceCollection services, string pluginDirectory)
    {
        if (!Directory.Exists(pluginDirectory))
        {
            return Array.Empty<string>();
        }

        var loaded = new List<string>();
        foreach (var assemblyPath in Directory.EnumerateFiles(pluginDirectory, "*.dll", SearchOption.TopDirectoryOnly))
        {
            var assembly = Assembly.LoadFrom(assemblyPath);
            foreach (var type in assembly.GetTypes().Where(static type => !type.IsAbstract && typeof(IPluginModule).IsAssignableFrom(type)))
            {
                if (Activator.CreateInstance(type) is IPluginModule module)
                {
                    module.Register(services);
                    loaded.Add(type.FullName ?? type.Name);
                }
            }
        }

        return loaded;
    }
}
