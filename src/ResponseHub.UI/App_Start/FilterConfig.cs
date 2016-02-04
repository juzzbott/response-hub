using System.Web;
using System.Web.Mvc;

using Enivate.ResponseHub.UI.Filters;

namespace Enivate.ResponseHub.UI
{
	public class FilterConfig
	{
		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new HandleErrorAttribute());
			filters.Add(new AuthorizeAttribute());
			filters.Add(new ResponseHubPageAttribute());
		}
	}
}
