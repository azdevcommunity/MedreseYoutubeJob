using System.Reflection;
using YoutubeApiSynchronize.Attributes;

namespace YoutubeApiSynchronize.Configuration;

public static class AutoConfigurationLoader
{
    public static void LoadConfiguration(this WebApplicationBuilder builder, List<Assembly> assemblies)
    {
        var services = builder.Services;
       
        var configClasses = assemblies
            .SelectMany(assembly => assembly.DefinedTypes)
            .Where(type => type.GetCustomAttribute<ConfigurationAttribute>() != null)
            .ToArray(); 

        foreach (var type in configClasses)
        {
            services.AddScoped(type);
        }

        using var scope = builder.Services.BuildServiceProvider().CreateScope();
        var serviceProvider = scope.ServiceProvider;

        foreach (var type in configClasses)
        {
            var instance = serviceProvider.GetService(type);
            if (instance == null) continue;
       
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => m.GetParameters().Length == 1 &&
                            m.GetParameters()[0].ParameterType == typeof(WebApplicationBuilder))
                .ToArray(); 

            foreach (var method in methods)
            {
                method.Invoke(instance, new object[] { builder });
            }
        }
    }
}