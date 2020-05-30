using System.Linq;
using System.Web.Hosting;
using System.Web.Mvc;
using RacersLeaderboard.Models;
using RacersLeaderboard.Services;

namespace RacersLeaderboard.Controllers
{    
	[RoutePrefix("tables")]
    public class TablesController : Controller
    {
        [Route("stats")]
		[OutputCache(Duration = 43200)]
		public ActionResult Stats()
        {
			var service = new iRacingScraperService();
			var csvFilename = HostingEnvironment.MapPath("~/driverStats.csv");
            
			service.RebuildStatsFileIfOld(csvFilename, 6.0, false);

			var drivers = service.GetDriverStats(csvFilename);

            foreach (var driver in drivers)
            {
                driver.Driver = AsrDriverNames.Names.ContainsKey($"{driver.CustId}")
                    ? AsrDriverNames.Names[$"{driver.CustId}"]
                    : driver.Driver;
            }

            var leaderboardService = new LeaderboardImageCreator(Server);			
			var statsTable = leaderboardService.CreateStatsTable(drivers);
				
			return new ImageResult(statsTable);
        }

        [Route("leaderboard")]
        public ActionResult Leaderboard()
		{
            var service = new iRacingScraperService();
            var csvFilename = HostingEnvironment.MapPath("~/driverStats.csv");  

            service.RebuildStatsFileIfOld(csvFilename, 6.0, false);

            // Get Info from cache only.
            var drivers = service.GetDriverStats(csvFilename);
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

			var leaderboardService = new LeaderboardImageCreator(Server);
			var leaderboard = leaderboardService.CreateLeaderboad(rankedDrivers);

			return new ImageResult(leaderboard);
		}

        [Route("timetrials/{id}")]
        [OutputCache(Duration = 3600)]
        public ActionResult TimeTrials(int id)
        {
            var service = new iRacingScraperService();
            var jsonFilename = HostingEnvironment.MapPath($"~/{id}-tt.json");
            
            service.RebuildTTLeaderboardIfOld(jsonFilename, id, 6.0, true);

            var timeTrials = service.GetTimeTrialLeaderboard(jsonFilename);

            var ledaerboardService = new LeaderboardImageCreator(Server);
            var leaderboard = ledaerboardService.CreateTimeTrialLeaderboad(timeTrials);

            return new ImageResult(leaderboard);
        }

        [Route("season/{id}/{f?}")]
        [OutputCache(Duration = 3600, VaryByParam = "id;f")]	
		public ActionResult Season(int id, string f = null)
		{
			bool forceRefresh = !string.IsNullOrWhiteSpace(f);

			var csvFilename = HostingEnvironment.MapPath($"~/season-{id}.csv");
			var service = new iRacingScraperService();

			if (service.IsFileOld(csvFilename, 6.0) || forceRefresh)
			{
				var cookies = service.LoginAndGetCookies();
				service.RebuildSeriesStandingFile(cookies, id, csvFilename);
			}

		    var standings = service.GetSeasonStandings(csvFilename);
			var leaderboardService = new LeaderboardImageCreator(Server);
			var standingsTable = leaderboardService.CreateSeasonTable(standings);

			return new ImageResult(standingsTable);
	    }
	}
}