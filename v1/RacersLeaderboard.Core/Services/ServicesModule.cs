using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RacersLeaderboard.Core.Configuration;

namespace RacersLeaderboard.Core.Services
{
    public class ServicesModule : IModule
    {
        public void Configure(IServiceCollection services, IConfiguration config)
        {
            List<string> ids = new List<string>(config.GetValue<string>("CustomerIds").Split(new []{ ","}, StringSplitOptions.RemoveEmptyEntries));
            
            services.AddSingleton<IWhitelister>(new Whitelister(ids));
            services.AddScoped<ISignatureImageCreator, SignatureImageCreator>();
            services.AddScoped<IScraperService, ScraperService>();
        }
    }
}
