using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using Microsoft.AspNet.Identity;

using Microsoft.Practices.Unity;

using Enivate.ResponseHub.ApplicationServices;
using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.DataAccess.Interface;
using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Model.Identity;
using System.Security.Claims;

namespace Enivate.ResponseHub.UI.Filters
{
	public class ResponseHubPageAttribute : ActionFilterAttribute, IActionFilter
	{

		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{

			if (filterContext != null && filterContext.HttpContext != null)
			{

				ClaimsIdentity userIdentity = (ClaimsIdentity)filterContext.HttpContext.User.Identity; 

				// If the user is authenticated, then get the user object from the database and set the full name into the view bag.
				if (userIdentity.IsAuthenticated)
				{

					//// Determine if we need to skip authentication.
					//bool skipAuthorization = filterContext.ActionDescriptor.IsDefined(typeof(AllowAnonymousAttribute), inherit: true)
					//						 || filterContext.ActionDescriptor.ControllerDescriptor.IsDefined(typeof(AllowAnonymousAttribute), inherit: true);
					//
					//// If the user is a system admin, but they are not in the "Admin" area, redirect now
					//if (!skipAuthorization &&  userIdentity.Claims.Any(i => i.Type == ClaimTypes.Role && i.Value == RoleTypes.SystemAdministrator) &&
					//	filterContext.RouteData.DataTokens["area"] == null || filterContext.RouteData.DataTokens["area"].ToString().ToLower() != "admin")
					//{
					//	filterContext.HttpContext.Response.Redirect("/admin");
					//	return;
					//}

					// Create the user service
					IUserRepository userRepo = UnityConfiguration.Container.Resolve<IUserRepository>();
					ILogger logger = UnityConfiguration.Container.Resolve<ILogger>();
					UserService userServce = new UserService(userRepo, logger);

					// Get the user id as a guid
					Guid userId = new Guid(userIdentity.GetUserId());

					IdentityUser user = Task.Run(async () => await userServce.FindByIdAsync(userId)).Result;
					if (user != null)
					{
						filterContext.Controller.ViewBag.UserFirstName = user.FirstName;
						filterContext.Controller.ViewBag.UserFullName = user.FullName;
					}
				}
			}
		}

	}
}