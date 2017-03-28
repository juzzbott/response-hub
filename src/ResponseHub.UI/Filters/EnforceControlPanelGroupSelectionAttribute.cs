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
	public class EnforceControlPanelGroupSelectionAttribute : ActionFilterAttribute, IActionFilter
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

					// Determine if we are a Groupo admin, if we are not, then don't do any further checking
					bool isGroupAdmin = userIdentity.Claims.Any(i => i.Type == ClaimTypes.Role && i.Value == RoleTypes.GroupAdministrator);
					if (!isGroupAdmin)
					{
						return;
					}

					// If the session key for the control panel selected group is set, then just return
					if (filterContext.HttpContext.Session[SessionConstants.ControlPanelContextGroupId] != null)
					{
						return;
					}

					// If we are not in the control panel area or the control panel Home/Index controller, then exit, as we don't need to check this
					bool isControlPanelArea = (filterContext.RouteData.DataTokens["area"] != null && filterContext.RouteData.DataTokens["area"].ToString().ToLower() == "controlpanel");
					bool isControlPanelHome = (filterContext.RouteData.DataTokens["area"] != null && filterContext.RouteData.DataTokens["area"].ToString().ToLower() == "controlpanel" &&
						filterContext.RouteData.Values["controller"] != null && filterContext.RouteData.Values["controller"].ToString().ToLower() == "groupselection" &&
						filterContext.RouteData.Values["action"] != null && filterContext.RouteData.Values["action"].ToString().ToLower() == "index");

					if (!isControlPanelArea || isControlPanelHome)
					{
						return;
					}

					// Otherwise, return to control panel home to set the group id
					filterContext.HttpContext.Response.Redirect("/control-panel/group-selection?show_info_message=1");

				}

			}

		}

	}
}