using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using Microsoft.AspNet.Identity;

using Microsoft.Practices.Unity;

using Enivate.ResponseHub.ApplicationServices.Identity;
using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Model.Identity;
using Enivate.ResponseHub.Model.Identity.Interface;

namespace Enivate.ResponseHub.UI.Filters
{
	public class ResponseHubPageAttribute : ActionFilterAttribute, IActionFilter
	{

		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{

			if (filterContext != null && filterContext.HttpContext != null)
			{

				IIdentity userIdentity = filterContext.HttpContext.User.Identity; 

				// If the user is authenticated, then get the user object from the database and set the full name into the view bag.
				if (userIdentity.IsAuthenticated)
				{

					// Create the user service
					IUserRepository userRepo = UnityConfiguration.Container.Resolve<IUserRepository>();
					ILogger logger = UnityConfiguration.Container.Resolve<ILogger>();
					UserService userServce = new UserService(userRepo, logger);

					// Get the user id as a guid
					Guid userId = new Guid(userIdentity.GetUserId());

					IdentityUser user = Task.Run(async () => await userServce.FindByIdAsync(userId)).Result;
					if (user != null)
					{
						filterContext.Controller.ViewBag.UserFullName = String.Format("{0} {1}", user.FirstName, user.Surname);
					}
				}
			}

			base.OnActionExecuting(filterContext);
		}

	}
}