using System;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RacersLeaderboard.Attributes
{
	public class AsrIRacingCustomerIdAuthorizeAttribute : AuthorizeAttribute
	{
		protected override bool AuthorizeCore(HttpContextBase httpContext)
		{
			var id = httpContext.Request.RequestContext.RouteData.Values["id"];
			var whitelist = ConfigurationManager.AppSettings["custids"];
			var ids = whitelist.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

			return ids.Contains(id);
		}				
	}
}