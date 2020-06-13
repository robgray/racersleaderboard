using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CsvHelper;
using Flurl;
using Newtonsoft.Json;
using RacersLeaderboard.Core.Configuration;
using RacersLeaderboard.Core.Services.iRacing.Models;
using RacersLeaderboard.Core.Storage;
using NullValueHandling = Flurl.NullValueHandling;

namespace RacersLeaderboard.Core.Services.iRacing
{
    public interface IScraperService
    {
        Task<List<DriverStats>> GetDriverStats();
        Task<List<SeasonStanding>> GetSeasonStandings(int seasonId);
        Task<List<TimeTrialLeaderboard.TimeTrialItem>> GetTimeTrialLeaderboard(int timeTrialId);
        Task GetSeasonStats(string csvFilename);
        Task GetSeriesStandingFile(int seasonId, string csvFilename);
        Task<bool> IsFileOld(string csvFilename, double ageInHours);
        Task RebuildStatsFileIfOld(string csvFilename, double ageInHours, bool force);
        Task RebuildTimeTrialLeaderboardIfOld(string jsonFilename, int seasonId, double ageInHours, bool force);
    }

	public class ScraperService : IScraperService
    {
        private readonly string _username;
        private readonly string _password;
        private readonly IWhitelister _whitelister;
        private readonly IBlobStore _blobStore;

        private CookieContainer _cookieContainer = null;

        public ScraperService(IBlobStore blobStore, IWhitelister whitelister)
        {
            _username = Environment.GetEnvironmentVariable("iracing.username");
            _password = Environment.GetEnvironmentVariable("iracing.password");
            _blobStore = blobStore;
            _whitelister = whitelister;
        }

        public async Task EnsureLoggedIn(bool forceRelogin = false)
        {
            if (_cookieContainer != null && !forceRelogin)
                return;

            var loginUri = new Uri(Constants.Urls.Login);
            _cookieContainer = new CookieContainer();
            using (var httpClientHandler = new HttpClientHandler {CookieContainer = _cookieContainer})
            {
                var client = new HttpClient(httpClientHandler);

                client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue() {MaxAge = TimeSpan.Zero};
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xhtml+xml"));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("image/apng"));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("image/webp"));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

                var responseMessage = await client.PostAsync(loginUri, new FormUrlEncodedContent(
                    new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("username", _username),
                        new KeyValuePair<string, string>("password", _password),
                        new KeyValuePair<string, string>("utcoffset", "-600"),
                        new KeyValuePair<string, string>("todaysdate", "")
                    }));

                if (responseMessage.StatusCode == HttpStatusCode.Found &&
                    responseMessage.Headers.Location.AbsoluteUri == Constants.Urls.MembersiteHome)
                {
                    var cookies = _cookieContainer.GetCookies(loginUri).Cast<Cookie>().ToList();
                    var cookieCollection = new CookieCollection();
                    foreach (var cookie in cookies)
                    {
                        cookieCollection.Add(cookie);
                    }
                }
                else
                {
                    _cookieContainer = null;
                    throw new Exception("Login to iRacing was unsuccessful");
                }
            }
        } 
	
		public async Task<List<DriverStats>> GetDriverStats()
		{
			var drivers = new List<DriverStats>();
            var driverStatsData = await _blobStore.GetBlobString(StorageContainers.CsvContainer, DataFiles.DriverStats);
            using (var csv = new CsvReader(new StringReader(driverStatsData), CultureInfo.InvariantCulture))
            {
                csv.Configuration.HasHeaderRecord = true;
                csv.Configuration.RegisterClassMap<DriverStatsMap>();
                drivers = csv.GetRecords<DriverStats>().ToList();
            }
			
            var asrDrivers = drivers.Where(driver => AsrDriverNames.Names.ContainsKey(driver.CustId.ToString())).ToList();
            
            // I'm in there twice, once with no position
		    return asrDrivers.Where(driver => !string.IsNullOrWhiteSpace(driver.Pos)).ToList();            
        }
		
		public async Task<List<SeasonStanding>> GetSeasonStandings(int seasonId)
		{
			var standings = new List<SeasonStanding>();
            var seasonStandingFileString = await _blobStore.GetBlobString(StorageContainers.CsvContainer, string.Format(DataFiles.SeasonFormat, seasonId));
            using (var csv = new CsvReader(new StringReader(seasonStandingFileString), CultureInfo.InvariantCulture))
			{
				csv.Configuration.HasHeaderRecord = true;
				csv.Configuration.RegisterClassMap<SeasonStandingMap>();
				standings = csv.GetRecords<SeasonStanding>().ToList();
			}

			var total = standings.Count;

		    var divisionTotals = standings.GroupBy(s => s.Division)
		        .Select(group => new
		        {
		            Division = group.Key,
		            DriverCount = group.Count()
		        }).ToDictionary(d => d.Division, d => d.DriverCount);

		    var driverDivisionPlace = from standing in standings
		        orderby standing.Points
		        group standing by standing.Division
		        into div
		        select new
		        {
		            Division = div.Key,
		            Elements = div.OrderByDescending(stat => stat.Points)
		        };
		    var ddp = driverDivisionPlace.ToDictionary(d => d.Division, d => d.Elements);

            standings.ForEach(s =>
            {
                s.TotalDrivers = total;
                s.DriversInDivision = divisionTotals[s.Division];
                var divisionDrivers = ddp[s.Division].ToList();

                s.DivisionPosition = divisionDrivers.FindIndex(t => t.CustId == s.CustId) + 1;
            });

            return standings.Where(standing => _whitelister.IsWhitelisted(standing.CustId.ToString())).ToList();
        }

	    public async Task<List<TimeTrialLeaderboard.TimeTrialItem>> GetTimeTrialLeaderboard(int timeTrialId)
        {
            var jsonFilename = string.Format(DataFiles.TimeTrialFormat, timeTrialId);
            var fileExists = await _blobStore.BlobExists(StorageContainers.CsvContainer, jsonFilename);
            if (!fileExists)
	        {
	            throw new FileNotFoundException("Time Trial Leaderboard file not found", jsonFilename);
	        }

            var jsonData = await _blobStore.GetBlobString(StorageContainers.CsvContainer, jsonFilename);
            var leaderboard = JsonConvert.DeserializeObject<TimeTrialLeaderboard>(jsonData).Data.TimeTrials;

            return leaderboard.Where(l => _whitelister.IsWhitelisted(l.CustId)).ToList();
        }

        public async Task GetSeasonStats(string csvFilename)
        {
            var url = Constants.Urls.DriverStatsData
                .SetQueryParams(new
                {
                    //search = "null",
                    //custid = 59619, 
                    friend = -1,    // Use this to get my friends
                    watched = 59619, // Use this to get my watched.  Together with Friends gets watched and friends. (AND not OR)
                    category = 2,   // 1 = oval, 2 = road, 3 = oval dirt, 4 = road dirt
                    //recent = -1,
                    //country = "null",
                    //classlow = -1,
                    //classhigh = -1,
                    //iratinglow = -1,
                    //iratinghigh = -1,
                    //ttratinglow = 1,
                    //ttratinghigh = -1,
                    //avgstartlow = -1,
                    //avgstarthigh = -1,
                    //avgfinishlow = -1,
                    //avgfinishhigh = -1,
                    //avgpointslow = -1,
                    //avgpointshigh = -1,
                    //avgincidentslow = -1,
                    //avgincidentshigh = -1,
                    //lowerbound = 26,
                    //upperbound = 37,
                    //sort = "irating",
                    //order = "desc",
                    //active = 1  // -1 = any, 0 = not active, 1 = active
                }, NullValueHandling.NameOnly);

            // When I move to get everyone's data, don't send through: friend, watched, custid
            // Getting all the data at once causes a 504 Gateway Timeout. 
            // To work around this, batch requests based on iRating range. 

            await GetGenericFileDataAndSaveToBlobStorage(url, csvFilename);
        }

        public async Task GetSeriesStandingFile(int seasonId, string csvFilename)
        {
            var url = Constants.Urls.SeasonStandings
                .SetQueryParams(new
                {
                    format = "csv",
                    seasonId,
                    carclassid = -1,
                    clubid = -1,
                    raceweek = -1,
                    division = -1,
                    start = 1,
                    end = 25,
                    sort = "points",
                    order = "desc"
                });

            await GetGenericFileDataAndSaveToBlobStorage(url, csvFilename);
        }

        public async Task<bool> IsFileOld(string csvFilename, double ageInHours)
        {
            var refreshFile = true;
			var exists = await _blobStore.BlobExists(StorageContainers.CsvContainer, csvFilename);
			if (exists)
			{
                var properties = await _blobStore.GetBlobProperties(StorageContainers.CsvContainer, csvFilename);
                if (properties.Created > DateTimeOffset.UtcNow.AddHours(-ageInHours))
				{
					refreshFile = false;
				}
			}

			return refreshFile;
		}

	    public async Task RebuildStatsFileIfOld(string csvFilename, double ageInHours, bool force)
	    {
            if ((await IsFileOld(csvFilename, ageInHours)) || force)
            {
                await GetSeasonStats(csvFilename);
            }
        }

	    public async Task RebuildTimeTrialLeaderboardIfOld(string jsonFilename, int seasonId, double ageInHours, bool force)
	    {
	        if ((await IsFileOld(jsonFilename, ageInHours)) || force)
	        {
                await GetTimeTrialSeasonLeaderboard(seasonId, jsonFilename);
	        }
        }

	    public async Task GetTimeTrialSeasonLeaderboard(int seasonId, string jsonFilename)
	    {
            var standingsUri = new Uri(Constants.Urls.SeasonTimeTrialStandings);
            var cookieContainer = new CookieContainer();
            using (var httpClientHandler = new HttpClientHandler { CookieContainer = cookieContainer })
            {
                var client = new HttpClient(httpClientHandler);

                client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue() { MaxAge = TimeSpan.Zero };
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
                
                var responseMessage = await client.PostAsync(standingsUri, new FormUrlEncodedContent(
                    new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("seasonid", seasonId.ToString()),
                        new KeyValuePair<string, string>("club", "-1"),
                        new KeyValuePair<string, string>("carclassid", "4"),
                        new KeyValuePair<string, string>("raceweek", "-1"),
                        new KeyValuePair<string, string>("division", "-1"),
                        new KeyValuePair<string, string>("sort", "points"),
                        new KeyValuePair<string, string>("order", "desc"),
                        new KeyValuePair<string, string>("start", "1"),
                        new KeyValuePair<string, string>("end", "100000"),
                    }));
                
                using (var sr = new StreamReader(await responseMessage.Content.ReadAsStreamAsync()))
                {
                    var jsonText = await sr.ReadToEndAsync();
                    await _blobStore.UploadBlobString(StorageContainers.CsvContainer, jsonFilename, jsonText);
                }
            }
        }
        
        private async Task GetGenericFileDataAndSaveToBlobStorage(string url, string destinationFile)
        {
            await EnsureLoggedIn();

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.CookieContainer = _cookieContainer;
            
            var standingsResp = await request.GetResponseAsync();
            using (StreamReader sr = new StreamReader(standingsResp.GetResponseStream() ?? throw new InvalidOperationException()))
            {
                var csvText = sr.ReadToEnd();
                await _blobStore.UploadBlobString(StorageContainers.CsvContainer, destinationFile, csvText);
            }
        }

        private async Task PostGenericFileDataAndSaveToBlobStorage(string url, string destinationFile)
        {
            await EnsureLoggedIn();

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.CookieContainer = _cookieContainer;
            
            var standingsResp = await request.GetResponseAsync();
            using (var sr = new StreamReader(standingsResp.GetResponseStream() ?? throw new InvalidOperationException()))
            {
                var csvText = await sr.ReadToEndAsync();
                await _blobStore.UploadBlobString(StorageContainers.CsvContainer, destinationFile, csvText);
            }
        }
    }
}