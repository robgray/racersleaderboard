using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RacersLeaderboard.Core.Configuration
{
    /// <summary>
    /// Marker class for Microsoft's Dependency Injection.
    /// </summary>
    public interface IModule
    {
        void Configure(IServiceCollection services, IConfiguration config);
    }
}
