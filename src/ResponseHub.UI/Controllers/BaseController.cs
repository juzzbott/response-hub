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
using Enivate.ResponseHub.Model.Units.Interface;
using Enivate.ResponseHub.Common.Constants;

namespace Enivate.ResponseHub.UI.Controllers
{
    public class BaseController : Controller
    {
		protected ILogger Log = ServiceLocator.Get<ILogger>();
		protected IUserService UserService = ServiceLocator.Get<IUserService>();
		protected readonly ICapcodeService CapcodeService = ServiceLocator.Get<ICapcodeService>();
		protected readonly IUnitService UnitService = ServiceLocator.Get<IUnitService>();

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

		/// <summary>
		/// Gets the current context unit id for the control panel.
		/// </summary>
		/// <returns></returns>
		protected Guid GetContextUnitId()
		{
			if (Session[SessionConstants.ContextUnitId] != null)
			{
				return (Guid)Session[SessionConstants.ContextUnitId];
			}
			else
			{
				return Guid.Empty;
			}
		}
	}
}