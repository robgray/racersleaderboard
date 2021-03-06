﻿using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RacersLeaderboard.Api.ActionResults;
using RacersLeaderboard.Core;
using RacersLeaderboard.Core.Services;
using RacersLeaderboard.Core.Services.iRacing;
using RacersLeaderboard.Core.Services.iRacing.Models;
using RacersLeaderboard.Core.TableBuilders;

namespace RacersLeaderboard.Api.Controllers
{
    [Route("[controller]")]
    public class ChartsController : ControllerBase
    {
        private readonly IScraperService _scraperService;

        public ChartsController(IScraperService scraperService)
        {
            _scraperService = scraperService;
        }

        /// <summary>
        /// Retrieve a detailed table of stats of all drivers, ranked by highest to lowest iRating
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> Stats()
        {
            await _scraperService.RebuildStatsFileIfOld(DataFiles.DriverStats, 6.0, false);
            var drivers = await _scraperService.GetDriverStats();

            foreach (var driver in drivers)
            {
                driver.Driver = AsrDriverNames.Names.ContainsKey($"{driver.CustId}")
                    ? AsrDriverNames.Names[$"{driver.CustId}"]
                    : driver.Driver;
            }

            var image = new FluentTableCreator()
                .WithDriverStats(drivers)
                .ForStatsTable()
                .Create();
				
			return new ImageResult(image);
        }

        /// <summary>
        /// Retrieve  a list of all drivers showing just iRating and SR, ordered by highest to lowest iRating
        /// </summary>
        [HttpGet("leaderboard")]
        public async Task<IActionResult> Leaderboard()
        {
            await _scraperService.RebuildStatsFileIfOld(DataFiles.DriverStats, 6.0, false);

            // Get Info from cache only.
            var drivers = await _scraperService.GetDriverStats();
            var rankedDrivers = drivers
				.OrderByDescending(d => d.iRating)
				.ThenByDescending(d => d.AvgPointsPerRace)
				.ToList();

		    foreach (var driver in rankedDrivers)
		    {
		        driver.Driver = AsrDriverNames.Names.ContainsKey($"{driver.CustId}")
		            ? AsrDriverNames.Names[$"{driver.CustId}"]
		            : driver.Driver;
		    }

            var leaderboard = new FluentTableCreator()
                .WithDriverStats(rankedDrivers)
                .ForLeaderboard()
                .Create();
            
			return new ImageResult(leaderboard);
		}

        [HttpGet("timetrials/{id}")]
        public async Task<IActionResult> TimeTrials(int id)
        {

            var jsonFilename = string.Format(DataFiles.TimeTrialFormat, id);

            await _scraperService.RebuildTimeTrialLeaderboardIfOld(jsonFilename, id, 6.0, true);

            var timeTrials = await _scraperService.GetTimeTrialLeaderboard(id);

            var image = new FluentTableCreator()
                .WithTimeTrials(timeTrials)
                .ForTimeTrials()
                .Create();
            
            return new ImageResult(image);
        }

        [HttpGet("season/{id}/{f?}")]
        public async Task<IActionResult> Season(int id, string f = null)
		{
			bool forceRefresh = !string.IsNullOrWhiteSpace(f);

            var csvFilename = string.Format(DataFiles.SeasonFormat, id);
			
			if ((await _scraperService.IsFileOld(csvFilename, 6.0)) || forceRefresh)
			{
                await _scraperService.GetSeriesStandingFile(id, csvFilename);
			}

		    var standings = await _scraperService.GetSeasonStandings(id);
            var standingsTable = new FluentTableCreator()
                .WithSeasonStandings(standings)
                .ForStandings()
                .Create();
            
			return new ImageResult(standingsTable);
	    }
	}
}