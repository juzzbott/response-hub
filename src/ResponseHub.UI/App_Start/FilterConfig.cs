using System.Web;
using System.Web.Mvc;

using Enivate.ResponseHub.UI.Filters;
using System.Security.Claims;
using Enivate.ResponseHub.Model.Identity;

namespace Enivate.ResponseHub.UI
{
	public class FilterConfig
	{
		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new HandleErrorAttribute());
			filters.Add(new ClaimsAuthorizeAttribute(ClaimTypes.Role, RoleTypes.GeneralUser));
			filters.Add(new SystemAdminUserRedirectAttribute());
			filters.Add(new EnforceControlPanelGroupSelectionAttribute());
			filters.Add(new ResponseHubPageAttribute());
			filters.Add(new ExceptionLogFilter());
		}
	}
}
