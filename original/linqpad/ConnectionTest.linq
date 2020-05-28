<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\System.Configuration.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Net.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Web.dll</Reference>
  <Namespace>System.Configuration</Namespace>
  <Namespace>System.Net</Namespace>
  <Namespace>System.Web</Namespace>
</Query>

void Main()
{	
	var cookies = iRacingScraperService.LoginAndGetCookies();

	var url = "http://members.iracing.com/memberstats/member/DriverStatsData?search=null&friend=-1&watched=59619&recent=-1&country=null&category=2&classlow=-1&classhigh=-1&iratinglow=-1&iratinghigh=-1&ttratinglow=-1&ttratinghigh=-1&avgstartlow=-1&avgstarthigh=-1&avgfinishlow=-1&avgfinishhigh=-1&avgpointslow=-1&avgpointshigh=-1&avgincidentslow=-1&avgincidentshigh=-1&custid=59619&lowerbound=1&upperbound=25&sort=irating&order=desc&active=1";

	var standingsReq = (HttpWebRequest)WebRequest.Create(url);
	standingsReq.CookieContainer = new CookieContainer();
	standingsReq.CookieContainer.Add(cookies);

	var standingsResp = standingsReq.GetResponse();

	var csvText = "";
	using (StreamReader sr = new StreamReader(standingsResp.GetResponseStream()))
	{
		csvText = sr.ReadToEnd();
	}

	Console.WriteLine(csvText);
}

public static class iRacingScraperService
{
	// Define other methods and classes here
	public static CookieCollection LoginAndGetCookies()
	{
		var username = Environment.GetEnvironmentVariable("iracing.username");
		var password = Environment.GetEnvironmentVariable("iracing.password");
		string loginUrl = "https://members.iracing.com/membersite/Login";
		string formParams = $"username={username}&password={password}&utcoffset=-600&todaysdate=";

		var req = (HttpWebRequest)WebRequest.Create(loginUrl);

		req.Headers.Add("Cache-Control", "max-age=0");
		req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";

		req.KeepAlive = true;
		//req.Referer = "https://members.iracing.com/membersite/login.jsp";
		req.CookieContainer = new CookieContainer();
		req.ContentType = "application/x-www-form-urlencoded";
		//req.Headers.Add("Accept-Language", "en-AU,en-US;q=0.9,en;q=0.8");
		req.Method = "POST";
		//req.Host = "members.iracing.com";	
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

			// Do I need other cookies here?

			return cookieCollection;
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
			throw ex;
		}
	}
}