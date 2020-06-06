using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Text;
using RacersLeaderboard.Models;
using CsvHelper;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;

namespace RacersLeaderboard.Services
{
	public class iRacingScraperService
	{
		public CookieCollection LoginAndGetCookies()
		{
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            var username = Environment.GetEnvironmentVariable("iracing.username");
			var password = Environment.GetEnvironmentVariable("iracing.username");
			string loginUrl = "https://members.iracing.com/membersite/Login";
			string formParams = $"username={username}&password={password}&utcoffset=-600&todaysdate=";
			
			var req = (HttpWebRequest)WebRequest.Create(loginUrl);

			req.Headers.Add("Cache-Control", "max-age=0");
			req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
			req.KeepAlive = true;
            req.CookieContainer = new CookieContainer();
			req.ContentType = "application/x-www-form-urlencoded";
            req.Method = "POST";
            var bytes = Encoding.ASCII.GetBytes(formParams);
			req.ContentLength = bytes.Length;
			try
			{
				using (Stream os = req.GetRequestStream())
				{
					os.Write(bytes, 0, bytes.Length);
				}

				var resp = (HttpWebResponse)req.GetResponse();
				CookieCollection cookieCollection = new CookieCollection
				{
					req.CookieContainer.GetCookies(new Uri("https://members.iracing.com"))
				};

				if (cookieCollection.Count == 0)
				{
					throw new Exception("Could not login to iRacing");
				}

				return cookieCollection;
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
				throw ex;
			}
		}


		public List<DriverStats> GetDriverStats(string csvFilename)
		{
			List<DriverStats> drivers = new List<DriverStats>();

			using (var csv = new CsvReader(File.OpenText(csvFilename)))
			{
				csv.Configuration.HasHeaderRecord = true;
                csv.Configuration.RegisterClassMap<DriverStatsMap>();
				drivers = csv.GetRecords<DriverStats>().ToList();
			}

			var asrDrivers = drivers.Where(driver => AsrDriverNames.Names.ContainsKey(driver.CustId.ToString())).ToList();
			// I'm in there twice, once with no position
			return asrDrivers.Where(driver => !driver.Pos.IsNullOrWhiteSpace()).ToList();
		}
		
		public List<SeasonStanding> GetSeasonStandings(string csvFilename)
		{
			List<SeasonStanding> standings = new List<SeasonStanding>();

			using (var csv = new CsvReader(File.OpenText(csvFilename)))
			{
				csv.Configuration.HasHeaderRecord = true;
				csv.Configuration.RegisterClassMap<SeasonStandingMap>();
				standings = csv.GetRecords<SeasonStanding>().ToList();
			}

			var whitelist = ConfigurationManager.AppSettings["custids"];
			var ids = whitelist.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

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

			return standings.Where(standing => ids.Contains(standing.CustId.ToString())).ToList();
		}

		public List<TimeTrialLeaderboard.TimeTrialItem> GetTimeTrialLeaderboard(string jsonFilename)
		{
			if (!File.Exists(jsonFilename))
			{
				throw new FileNotFoundException("Time Trial Leaderboard file not found", jsonFilename);
			}

			var jsonData = File.ReadAllText(jsonFilename);

			var leaderboard = JsonConvert.DeserializeObject<TimeTrialLeaderboard>(jsonData).Data.TimeTrials;

			var whitelist = ConfigurationManager.AppSettings["custids"];
			var ids = whitelist.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse);

			return leaderboard.Where(timetrial => ids.Any(id => id == timetrial.CustId)).ToList();

		}

		public void RebuildStatsFile(CookieCollection cookies, string csvFilename)
		{
			// need to watch all ASR drivers, even those who are friends. Or they won't show up.
			var url =
				"http://members.iracing.com/memberstats/member/DriverStatsData?search=null&friend=-1&watched=59619&recent=-1&country=null&category=2&classlow=-1&classhigh=-1&iratinglow=-1&iratinghigh=-1&ttratinglow=-1&ttratinghigh=-1&avgstartlow=-1&avgstarthigh=-1&avgfinishlow=-1&avgfinishhigh=-1&avgpointslow=-1&avgpointshigh=-1&avgincidentslow=-1&avgincidentshigh=-1&custid=59619&lowerbound=26&upperbound=37&sort=irating&order=desc&active=1";

			var standingsReq = (HttpWebRequest)WebRequest.Create(url);
			standingsReq.CookieContainer = new CookieContainer();
			standingsReq.CookieContainer.Add(cookies);

			var standingsResp = standingsReq.GetResponse();

			using (StreamReader sr = new StreamReader(standingsResp.GetResponseStream()))
			{
				var csvText = sr.ReadToEnd();
				File.WriteAllText(csvFilename, csvText);
			}
		}

		public void RebuildSeriesStandingFile(CookieCollection cookies, int seasonId, string csvFilename)
		{
			var url =
				$"http://members.iracing.com/memberstats/member/GetSeasonStandings?format=csv&seasonid={seasonId}&carclassid=-1&clubid=-1&raceweek=-1&division=-1&start=1&end=25&sort=points&order=desc";

			var standingsReq = (HttpWebRequest)WebRequest.Create(url);
			standingsReq.CookieContainer = new CookieContainer();
			standingsReq.CookieContainer.Add(cookies);

			var standingsResp = standingsReq.GetResponse();

			using (StreamReader sr = new StreamReader(standingsResp.GetResponseStream()))
			{
				var csvText = sr.ReadToEnd();
				File.WriteAllText(csvFilename, csvText);
			}
		}

		public bool IsFileOld(string csvFilename, double ageInHours)
		{
			var refreshFile = true;
			if (File.Exists(csvFilename))
			{
				var fi = new FileInfo(csvFilename);
				if (fi.LastWriteTime > DateTime.Now.AddHours(-ageInHours))
				{
					refreshFile = false;
				}
			}

			return refreshFile;
		}

		public void RebuildStatsFileIfOld(string csvFilename, double ageInHours, bool force)
		{
			if (IsFileOld(csvFilename, ageInHours) || force)
			{
				var cookies = LoginAndGetCookies();
				RebuildStatsFile(cookies, csvFilename);
			}
		}

		public void RebuildTTLeaderboardIfOld(string jsonFilename, int seasonid, double ageInHours, bool force)
		{
			if (IsFileOld(jsonFilename, ageInHours) || force)
			{
				var cookies = LoginAndGetCookies();
				GetTTSeasonLeaderboard(cookies, seasonid, jsonFilename);
			}
		}

		public void GetTTSeasonLeaderboard(CookieCollection cookies, int seasonId, string jsonFilename)
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
			postStream.Write(formContentBytes, 0, formContentBytes.Length);
			postStream.Flush();
			postStream.Close();

			var standingsResp = standingsReq.GetResponse();

			var jsonText = string.Empty;
			using (StreamReader sr = new StreamReader(standingsResp.GetResponseStream()))
			{
				jsonText = sr.ReadToEnd();
				File.WriteAllText(jsonFilename, jsonText);
			}
		}
	}
}