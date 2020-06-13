<Query Kind="Program">
  <NuGetReference>CsvHelper</NuGetReference>
  <NuGetReference>Flurl</NuGetReference>
  <NuGetReference>Newtonsoft.Json</NuGetReference>
  <NuGetReference>System.Net.Http</NuGetReference>
  <Namespace>Newtonsoft.Json</Namespace>
  <Namespace>Newtonsoft.Json.Bson</Namespace>
  <Namespace>Newtonsoft.Json.Converters</Namespace>
  <Namespace>Newtonsoft.Json.Linq</Namespace>
  <Namespace>Newtonsoft.Json.Schema</Namespace>
  <Namespace>Newtonsoft.Json.Serialization</Namespace>
  <Namespace>System.Net</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Net.Http.Headers</Namespace>
  <Namespace>Flurl</Namespace>
</Query>

async Task Main()
{
	var data = new RacingDataService();
	var cookies = await data.LoginAndGetCookies();
	//var cookies = data.LoginAndGetCookiesOriginal();
	await data.GetDriverStatsData(cookies);	
}

public class RacingDataService
{
	private readonly string _username;
	private readonly string _password;

	public RacingDataService()
	{
		_username = Environment.GetEnvironmentVariable("iracing.username");
		_password = Environment.GetEnvironmentVariable("iracing.password");
	}

	public CookieCollection LoginAndGetCookiesOriginal()
	{		
        var username = Environment.GetEnvironmentVariable("iracing.username");
		var password = Environment.GetEnvironmentVariable("iracing.password");
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

	// Define other methods and classes here
	public async Task<CookieCollection> LoginAndGetCookies()
	{
		var loginUri = new Uri("https://members.iracing.com/membersite/Login");
        var cookieContainer = new CookieContainer();
		using (var httpClientHandler = new HttpClientHandler { CookieContainer = cookieContainer })
		using (var client = new HttpClient(httpClientHandler))
		{
			client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue() { MaxAge = TimeSpan.Zero };
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

				if (responseMessage.StatusCode == HttpStatusCode.Found && responseMessage.Headers.Location. == "http://members.iracing.com/membersite/member/Home.do")
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

	public async Task GetDriverStatsData(CookieCollection cookies)
	{		
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
				}, Flurl.NullValueHandling.NameOnly);

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
			Console.WriteLine(csvText);
		}
	}
}

public class iRacingScraperService
{
	public CookieCollection LoginAndGetCookies()
	{
		ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

		var username = Environment.GetEnvironmentVariable("iracing.username");
		var password = Environment.GetEnvironmentVariable("iracing.password");
		string loginUrl = "https://members.iracing.com/membersite/Login";
		string formParams = $"username={username}&password={password}&utcoffset=-600&todaysdate=";

		var req = (HttpWebRequest)WebRequest.Create(loginUrl);
		
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
}