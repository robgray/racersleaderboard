using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Schema;
using CsvHelper;
using Flurl;
using Newtonsoft.Json;
using RacersLeaderboard.Core.Configuration;
using RacersLeaderboard.Core.Models;
using RacersLeaderboard.Core.Storage;
using NullValueHandling = Flurl.NullValueHandling;

namespace RacersLeaderboard.Core.Services
{
    public interface IScraperService
    {
        Task<CookieCollection> LoginAndGetCookies();
        Task<List<DriverStats>> GetDriverStats();
        Task<List<SeasonStanding>> GetSeasonStandings(int seasonId);
        Task<List<TimeTrialLeaderboard.TimeTrialItem>> GetTimeTrialLeaderboard(int timeTrialId);
        Task GetStatsFile(CookieCollection cookies, string csvFilename);
        Task GetSeriesStandingFile(CookieCollection cookies, int seasonId, string csvFilename);
        Task<bool> IsFileOld(string csvFilename, double ageInHours);
        Task RebuildStatsFileIfOld(string csvFilename, double ageInHours, bool force);
        Task RebuildTTLeaderboardIfOld(string jsonFilename, int seasonid, double ageInHours, bool force);
    }

	public class ScraperService : IScraperService
    {
        private readonly string _username;
        private readonly string _password;
        private readonly IWhitelister _whitelister;
        private readonly IBlobStore _blobStore;

        public ScraperService(IBlobStore blobStore, IWhitelister whitelister)
        {
            _username = Environment.GetEnvironmentVariable("iracing.username");
            _password = Environment.GetEnvironmentVariable("iracing.password");
            _blobStore = blobStore;
            _whitelister = whitelister;
        }

        public async Task<CookieCollection> LoginAndGetCookies()
        {
            var loginUri = new Uri("https://members.iracing.com/membersite/Login");
            var cookieContainer = new CookieContainer();
            using (var httpClientHandler = new HttpClientHandler { CookieContainer = cookieContainer })
            using (var client = new HttpClient(httpClientHandler))
            {
                client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue() {MaxAge = TimeSpan.Zero};
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xhtml+xml"));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("image/apng"));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("image/webp"));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

                try
                {
                    var responseMessage = await client.PostAsync(loginUri, new FormUrlEncodedContent(
                        new List<KeyValuePair<string, string>>
                        {
                            new KeyValuePair<string, string>("username", _username),
                            new KeyValuePair<string, string>("password", _password),
                            new KeyValuePair<string, string>("utcoffset", "-600"),
                            new KeyValuePair<string, string>("todaysdate", "")
                        }));

                    if (responseMessage.StatusCode == HttpStatusCode.Found)
                    {
                        var cookies = cookieContainer.GetCookies(loginUri).Cast<Cookie>().ToList();
                        var cookieCollection = new CookieCollection();
                        foreach (var cookie in cookies)
                        {
                            cookieCollection.Add(cookie);
                        }

                        return cookieCollection;
                    }

                    throw new Exception("Login to iRacing was unsuccessful");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    throw ex;
                }
            }
        } 
	
		public async Task<List<DriverStats>> GetDriverStats()
		{
			List<DriverStats> drivers = new List<DriverStats>();
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
			List<SeasonStanding> standings = new List<SeasonStanding>();
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

		public async Task GetStatsFile(CookieCollection cookies, string csvFilename)
		{
            // need to have all ASR drivers as "Studied", even those who are friends. Or they won't show up.
            var url = @"http://members.iracing.com/memberstats/member/DriverStatsData"
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

            var standingsReq = (HttpWebRequest)WebRequest.Create(url);
			standingsReq.CookieContainer = new CookieContainer();
			standingsReq.CookieContainer.Add(cookies);

			var standingsResp = standingsReq.GetResponse();
            using (StreamReader sr = new StreamReader(standingsResp.GetResponseStream() ?? throw new InvalidOperationException()))
			{
			    var csvText = sr.ReadToEnd();
                if (await _blobStore.BlobExists(StorageContainers.CsvContainer, csvFilename))
                {
                    await _blobStore.DeleteBlob(StorageContainers.CsvContainer, csvFilename);
                }

                await _blobStore.UploadBlobString(StorageContainers.CsvContainer, csvFilename, csvText);
            }
		}

		public async Task GetSeriesStandingFile(CookieCollection cookies, int seasonId, string csvFilename)
        {
            var url = "http://members.iracing.com/memberstats/member/GetSeasonStandings"
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

            var standingsReq = (HttpWebRequest)WebRequest.Create(url);
			standingsReq.CookieContainer = new CookieContainer();
			standingsReq.CookieContainer.Add(cookies);

			var standingsResp = await standingsReq.GetResponseAsync();
            using (StreamReader sr = new StreamReader(standingsResp.GetResponseStream() ?? throw new InvalidOperationException()))
			{
			    var csvText = sr.ReadToEnd();
                await _blobStore.UploadBlobString(StorageContainers.CsvContainer, csvFilename, csvText);
            }
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
                var cookies = await LoginAndGetCookies();
                await GetStatsFile(cookies, csvFilename);
            }
        }

	    public async Task RebuildTTLeaderboardIfOld(string jsonFilename, int seasonid, double ageInHours, bool force)
	    {
	        if ((await IsFileOld(jsonFilename, ageInHours)) || force)
	        {
	            var cookies = await LoginAndGetCookies();
	            await GetTTSeasonLeaderboard(cookies, seasonid, jsonFilename);
	        }
        }

	    public async Task GetTTSeasonLeaderboard(CookieCollection cookies, int seasonId, string jsonFilename)
	    {	        
	        string ttUrl = "http://members.iracing.com/memberstats/member/GetSeasonTTStandings";

	        var standingsReq = (HttpWebRequest)WebRequest.Create(ttUrl);            
	        standingsReq.CookieContainer = new CookieContainer();
	        standingsReq.CookieContainer.Add(cookies);
	        
	        standingsReq.Headers["Pragma"] = "no-cache";
	        standingsReq.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
	        standingsReq.Accept = "*/*";
	        standingsReq.ContentType = "application/x-www-form-urlencoded";            
	        standingsReq.Method = "POST";


	        var content = $"seasonid={seasonId}&clubid=-1&carclassid=4&raceweek=-1&division=-1&sort=points&order=desc&start=1&end=100000";
	        var postStream = standingsReq.GetRequestStream();
            
	        var formContentBytes = new ASCIIEncoding().GetBytes(content);
            postStream.Write(formContentBytes,0, formContentBytes.Length);
	        postStream.Flush();
	        postStream.Close();
            
            var standingsResp = standingsReq.GetResponse();

            using (var sr = new StreamReader(standingsResp.GetResponseStream() ?? throw new InvalidOperationException()))
            {
                var jsonText = sr.ReadToEnd();
                await _blobStore.UploadBlobString(StorageContainers.CsvContainer, jsonFilename, jsonText);
            }
        }
	}
}