using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

using Microsoft.AspNet.Identity;
using Microsoft.Practices.Unity;

using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Model.Identity.Interface;
using Enivate.ResponseHub.Model.Identity;

namespace Enivate.ResponseHub.UI.Controllers.Api
{
    public class BaseApiController : ApiController
    {

		private ILogger _log;
		protected ILogger Log
		{
			get
			{
				return _log ?? (_log = UnityConfiguration.Container.Resolve<ILogger>());
			}
		}

		private IUserService _userService;
		protected IUserService UserService
		{
			get
			{
				return _userService ?? (_userService = UnityConfiguration.Container.Resolve<IUserService>());
			}
		}

		public Guid UserId { get; set; }

		public IdentityUser CurrentUser { get; set; }

		public BaseApiController()
		{
			// If the user is authenticated, then get the details
			if (User.Identity.IsAuthenticated)
			{
				UserId = new Guid(User.Identity.GetUserId());
			}
		}

		public async Task<IdentityUser> GetCurrentUser()
		{
			return await UserService.FindByIdAsync(UserId);
		}

	}
}
