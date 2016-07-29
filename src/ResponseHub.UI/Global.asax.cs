using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;

using Enivate.ResponseHub.Common;

namespace Enivate.ResponseHub.UI
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
		{
			
			// Unity configuration loader
			UnityConfiguration.Container = new UnityContainer().LoadConfiguration();

			// Code that runs on application startup
			AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }
    }
}
