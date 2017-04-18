using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Mvc;

using Enivate.ResponseHub.Common.Constants;
using System.Security.Claims;
using Enivate.ResponseHub.Model.Identity;

namespace Enivate.ResponseHub.UI.Filters
{
	public class EnforceControlPanelUnitSelectionAttribute : ActionFilterAttribute, IActionFilter
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
					if (skipAuthorization)
					{
						return;
					}

					// Determine if we are a Unito admin, if we are not, then don't do any further checking
					bool isUnitAdmin = userIdentity.Claims.Any(i => i.Type == ClaimTypes.Role && i.Value == RoleTypes.UnitAdministrator);
					if (!isUnitAdmin)
					{
						return;
					}

					// If the session key for the control panel selected unit is set, then just return
					if (filterContext.HttpContext.Session[SessionConstants.ControlPanelContextUnitId] != null)
					{
						return;
					}

					// If we are not in the control panel area or the control panel Home/Index controller, then exit, as we don't need to check this
					bool isControlPanelArea = (filterContext.RouteData.DataTokens["area"] != null && filterContext.RouteData.DataTokens["area"].ToString().ToLower() == "controlpanel");
					bool isControlPanelHome = (filterContext.RouteData.DataTokens["area"] != null && filterContext.RouteData.DataTokens["area"].ToString().ToLower() == "controlpanel" &&
						filterContext.RouteData.Values["controller"] != null && filterContext.RouteData.Values["controller"].ToString().ToLower() == "unitselection" &&
						filterContext.RouteData.Values["action"] != null && filterContext.RouteData.Values["action"].ToString().ToLower() == "index");

					if (!isControlPanelArea || isControlPanelHome)
					{
						return;
					}

					// Otherwise, return to control panel home to set the unit id
					filterContext.HttpContext.Response.Redirect(String.Format("/control-panel/unit-selection?show_info_message=1&return_url={0}", 
						filterContext.HttpContext.Server.UrlEncode(filterContext.HttpContext.Request.Url.PathAndQuery)));

				}

			}

		}

	}
}