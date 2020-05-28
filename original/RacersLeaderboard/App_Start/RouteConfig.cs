using System.Web.Mvc;
using System.Web.Routing;

namespace RacersLeaderboard
{
	public class RouteConfig
	{
		public static void RegisterRoutes(RouteCollection routes)
		{
		    routes.LowercaseUrls = true;

			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapMvcAttributeRoutes();
			
            /* Leave default route */
            routes.MapRoute(
				name: "Default",
				url: "{controller}/{action}/{id}",
				defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional, },
				namespaces: new[] { "RacersLeaderboard.Controllers" }
            );            
		}
	}	
}
