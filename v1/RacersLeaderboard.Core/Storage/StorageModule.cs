using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RacersLeaderboard.Core.Configuration;

namespace RacersLeaderboard.Core.Storage
{
    public class StorageModule : IModule
    {
        public void Configure(IServiceCollection services, IConfiguration config)
        {
            services.AddSingleton<IBlobContainerFactory>(new BlobContainerFactory(config.GetValue<string>("AzureWebJobsStorage")));
            services.AddScoped<IBlobStore, BlobStore>();
            services.AddSingleton<IQueueSender>(new QueueSender(config.GetValue<string>("AzureWebJobsStorage")));
        }
    }
}
