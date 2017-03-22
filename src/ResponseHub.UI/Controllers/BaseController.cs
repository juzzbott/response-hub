using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using Microsoft.AspNet.Identity;

using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Model.Identity;
using Enivate.ResponseHub.Model.Identity.Interface;

namespace Enivate.ResponseHub.UI.Controllers
{
    public class BaseController : Controller
    {
		protected ILogger Log
		{
			get
			{
				return ServiceLocator.Get<ILogger>();
			}
		}

		protected IUserService UserService
		{
			get
			{
				return ServiceLocator.Get<IUserService>();
			}
		}

		public Guid UserId
		{
			get
			{
				// If the user is authenticated, then get the details
				if (User != null && User.Identity.IsAuthenticated)
				{
					return new Guid(User.Identity.GetUserId());
				}
				else
				{
					return Guid.Empty;
				}
			}
		}

		public BaseController()
		{ 
		}

		public async Task<IdentityUser> GetCurrentUser()
		{
			return await UserService.FindByIdAsync(UserId);
		}
	}
}