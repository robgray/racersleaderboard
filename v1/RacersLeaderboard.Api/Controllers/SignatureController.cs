using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RacersLeaderboard.Api.ActionResults;
using RacersLeaderboard.Core;
using RacersLeaderboard.Core.Repositories;
using RacersLeaderboard.Core.Services;

namespace RacersLeaderboard.Api.Controllers
{
    [Route("[controller]")]
	public class SignatureController : ControllerBase
	{
		private IDriverInfoRepository _driverInfoRepository;
        private IScraperService _scraperService;
        private ISignatureImageCreator _signatureCreator;
        private IWhitelister _whitelister;
		
		public SignatureController(IDriverInfoRepository driverInfoRepository, IScraperService scraperService, ISignatureImageCreator signatureImageCreator, IWhitelister whitelister)
        {
            _driverInfoRepository = driverInfoRepository;
            _scraperService = scraperService;
            _signatureCreator = signatureImageCreator;
            _whitelister = whitelister;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
		{
			if (!Authorised(id))
				return Unauthorized();
			
            await _scraperService.RebuildStatsFileIfOld(DataFiles.DriverStats, 6.0, false);
			
		    var driverInfo = (await _scraperService.GetDriverStats()).First(d => d.CustId == id);
            var signature = await _signatureCreator.GetRoadSignature(driverInfo);
			
			return new ImageResult(signature);
		}

	    [HttpGet("badge/{type}/{customerId}")]
	    public async Task<IActionResult> Badge(string type, int customerId)
        {
            if (!Authorised(customerId))
                return Unauthorized();
			
	        var driverInfo = (await _scraperService.GetDriverStats()).First(d => d.CustId == customerId);

	        Image signature = null;
	        //if (type == "road")
	            signature = await _signatureCreator.GetRoadBadge(driverInfo);
            //else if (type == "oval")
	           // signature = signatureCreator.GetOvalBadge(driverInfo);
            //else if (type == "dirtoval")
	           // signature = signatureCreator.GetDirtOvalBadge(driverInfo);
            //else if (type == "dirtroad")
	           // signature = signatureCreator.GetDirtRoadBadge(driverInfo);

            return new ImageResult(signature);
        }

        [HttpGet("road/{id}", Name = "Road")]
        public async Task<IActionResult> Road(int id)
		{
			if (!Authorised(id))
				return Unauthorized();

            await _scraperService.RebuildStatsFileIfOld(DataFiles.DriverStats, 6.0, false);
			
		    var driverInfo = (await _scraperService.GetDriverStats()).First(d => d.CustId == id);
            var signature = await _signatureCreator.GetRoadSignature(driverInfo);

			return new ImageResult(signature);
		}
		
        [HttpGet("mini/{id}", Name = "Mini")]
        public async Task<IActionResult> Mini(int id)
		{
			if (!Authorised(id))
				return Unauthorized();

            await _scraperService.RebuildStatsFileIfOld(DataFiles.DriverStats, 6.0, false);
            
		    var driverInfo = (await _scraperService.GetDriverStats()).First(d => d.CustId == id);
            var signature = await _signatureCreator.GetRoadMiniSignature(driverInfo);
			
			return new ImageResult(signature);
		}

        [HttpGet("srr/{id}", Name="SimracingRocksMini")]
        public async Task<IActionResult> Srr(int id)
	    {
	        if (!Authorised(id))
	            return Unauthorized();
            
	        await _scraperService.RebuildStatsFileIfOld(DataFiles.DriverStats, 6.0, false);
	        
	        var driverInfo = (await _scraperService.GetDriverStats()).First(d => d.CustId == id);
	        var signature = await _signatureCreator.GetRoadMiniSrrSignature(driverInfo);

	        return new ImageResult(signature);
	    }

        private bool Authorised(int custId) => _whitelister.IsWhitelisted(custId);
    }
}