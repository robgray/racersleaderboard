using System;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Web.Hosting;
using System.Web.Mvc;
using RacersLeaderboard.Repositories;
using RacersLeaderboard.Services;

namespace RacersLeaderboard.Controllers
{
    [RoutePrefix("iracingsig")]
	public class IRacingSigController : Controller
	{
		private static DriverInfoRepository _driverInfoRepository;
		
		static IRacingSigController()
		{
			var filename = HostingEnvironment.MapPath("~/driverData.json");		
			_driverInfoRepository = new DriverInfoRepository(filename);
		}

        [Route("{id}")]
		[OutputCache(Duration=43200, VaryByParam = "id")]
		//[AsrIRacingCustomerIdAuthorize]
		public ActionResult Index(int id)
		{
			if (!Authorised(id))
				return new EmptyResult();

            var service = new iRacingScraperService();
            var csvFilename = HostingEnvironment.MapPath("~/driverStats.csv");

            service.RebuildStatsFileIfOld(csvFilename, 6.0, false);

            var signatureCreator = new SignatureImageCreator(Server);
						
		    var driverInfo = service.GetDriverStats(csvFilename).First(d => d.CustId == id);

            var signature = signatureCreator.GetRoadSignature(driverInfo);
			
			return new ImageResult(signature);
		}

	    [Route("badge/{type}/{customerId}")]
	    public ActionResult Badge(string type, int customerId)
	    {
	        if (!Authorised(customerId))
	            return new EmptyResult();

	        var service = new iRacingScraperService();
	        var csvFilename = HostingEnvironment.MapPath("~/driverStats.csv");

	        service.RebuildStatsFileIfOld(csvFilename, 6.0, false);

	        var signatureCreator = new SignatureImageCreator(Server);

	        var driverInfo = service.GetDriverStats(csvFilename).First(d => d.CustId == customerId);

	        Image signature = null;
	        //if (type == "road")
	            signature = signatureCreator.GetRoadBadge(driverInfo);
            //else if (type == "oval")
	           // signature = signatureCreator.GetOvalBadge(driverInfo);
            //else if (type == "dirtoval")
	           // signature = signatureCreator.GetDirtOvalBadge(driverInfo);
            //else if (type == "dirtroad")
	           // signature = signatureCreator.GetDirtRoadBadge(driverInfo);

            return new ImageResult(signature);
        }

        [Route("road/{id}", Name = "Road")]
		[OutputCache(Duration = 43200, VaryByParam = "id")]
		//[AsrIRacingCustomerIdAuthorize]
		public ActionResult Road(int id)
		{
			if (!Authorised(id))
				return new EmptyResult();

            var service = new iRacingScraperService();
            var csvFilename = HostingEnvironment.MapPath("~/driverStats.csv");

            service.RebuildStatsFileIfOld(csvFilename, 6.0, false);

            var signatureCreator = new SignatureImageCreator(Server);
			
		    var driverInfo = service.GetDriverStats(csvFilename).First(d => d.CustId == id);

            var signature = signatureCreator.GetRoadSignature(driverInfo);

			return new ImageResult(signature);
		}

        [Route("mini/{id}", Name = "Mini")]
		[OutputCache(Duration = 43200, VaryByParam = "id")]
		//[AsrIRacingCustomerIdAuthorize]
		public ActionResult Mini(int id)
		{
			if (!Authorised(id))
				return new EmptyResult();

            var service = new iRacingScraperService();
            var csvFilename = HostingEnvironment.MapPath("~/driverStats.csv");

            service.RebuildStatsFileIfOld(csvFilename, 6.0, false);
            var signatureCreator = new SignatureImageCreator(Server);
			
		    var driverInfo = service.GetDriverStats(csvFilename).First(d => d.CustId == id);
            var signature = signatureCreator.GetRoadMiniSignature(driverInfo);
			
			return new ImageResult(signature);
		}

        [Route("srr/{id}", Name="SimracingRocksMini")]
	    [OutputCache(Duration = 43200, VaryByParam = "id")]
	    //[AsrIRacingCustomerIdAuthorize]
	    public ActionResult Srr(int id)
	    {
	        if (!Authorised(id))
	            return new EmptyResult();

	        if (Request.UrlReferrer != null && Request.UrlReferrer.AbsolutePath.Contains("atomicracingracing.net/forum/"))
	            return Badge("road", id);
	        
	        var service = new iRacingScraperService();
	        var csvFilename = HostingEnvironment.MapPath("~/driverStats.csv");

	        service.RebuildStatsFileIfOld(csvFilename, 6.0, false);
	        var signatureCreator = new SignatureImageCreator(Server);

	        var driverInfo = service.GetDriverStats(csvFilename).First(d => d.CustId == id);
	        var signature = signatureCreator.GetRoadMiniSrrSignature(driverInfo);

	        return new ImageResult(signature);
	    }

        private bool Authorised(int id)
		{			
			var whitelist = ConfigurationManager.AppSettings["custids"];
			var ids = whitelist.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

			return ids.Contains(id.ToString());
		}		
	}
}