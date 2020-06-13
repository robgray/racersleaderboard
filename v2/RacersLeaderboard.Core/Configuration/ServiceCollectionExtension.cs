using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RacersLeaderboard.Core.Configuration
{

    // Load the various configuration modules
    public static class ServiceCollectionExtensions
    {
        public static void ConfigureSharedModules(this IServiceCollection services, Assembly[] assemblies, IConfiguration config)
        {
            var sharedModules = assemblies
                .SelectMany(ass => ass.GetExportedTypes())
                .Where(t => t.IsClass && typeof(IModule).IsAssignableFrom(t));

            foreach (var module in sharedModules)
            {
                var moduleInstance = (IModule)Activator.CreateInstance(module);

                moduleInstance.Configure(services, config);
            }
        }
    }
}
