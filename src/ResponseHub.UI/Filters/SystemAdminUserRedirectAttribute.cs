using Enivate.ResponseHub.Model.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace Enivate.ResponseHub.UI.Filters
{
	public class SystemAdminUserRedirectAttribute : ActionFilterAttribute, IActionFilter
	{

		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{

			if (filterContext != null && filterContext.HttpContext != null)
			{

				ClaimsIdentity userIdentity = (ClaimsIdentity)filterContext.HttpContext.User.Identity;

				// If the user is authenticated, then get the user object from the database and set the full name into the view bag.
				if (userIdentity.IsAuthenticated)
				{

					// Determine if we need to skip authentication.
					bool skipAuthorization = filterContext.ActionDescriptor.IsDefined(typeof(AllowAnonymousAttribute), inherit: true)
											 || filterContext.ActionDescriptor.ControllerDescriptor.IsDefined(typeof(AllowAnonymousAttribute), inherit: true);

					// Determine if we are a system admin
					bool isSystemAdmin = userIdentity.Claims.Any(i => i.Type == ClaimTypes.Role && i.Value == RoleTypes.SystemAdministrator);

					// Find out if we are in the admin area
					bool isAdminArea = (filterContext.RouteData.DataTokens["area"] != null && filterContext.RouteData.DataTokens["area"].ToString().ToLower() == "admin");

					string[] allowedControllerNames = { "myaccount" };
					bool isAllowedController = allowedControllerNames.Contains(filterContext.RouteData.Values["Controller"].ToString().ToLower());

					// If the user is a system admin, but they are not in the "Admin" area, redirect now
					if (!skipAuthorization && isSystemAdmin && !isAdminArea && !isAllowedController)
					{
						filterContext.HttpContext.Response.Redirect("/admin");
						return;
					}

				}
			}
		}

	}
}