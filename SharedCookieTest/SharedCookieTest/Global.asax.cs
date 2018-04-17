using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace SharedCookieTest
{
	public class MvcApplication : System.Web.HttpApplication
	{
		protected void Application_Start()
		{
			AreaRegistration.RegisterAllAreas();
			FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
			RouteConfig.RegisterRoutes(RouteTable.Routes);
			BundleConfig.RegisterBundles(BundleTable.Bundles);
		}

		protected void Application_Error(object sender, EventArgs e)
		{
			Exception ex = Server.GetLastError();
		}

		protected void Session_Start(object sender, EventArgs e)
		{
			if (Context?.Session?.IsNewSession ?? false)
				if (Request.Cookies?[".AspNet.SharedCookie"]?.Value == null)
					Response.Redirect("~/Account/Login");
			//Response.RedirectToRoute(new { Controller = "Account", Action = "Login" });
		}
	}
}
