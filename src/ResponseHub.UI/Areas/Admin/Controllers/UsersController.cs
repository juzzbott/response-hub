using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using Microsoft.Practices.Unity;

using Microsoft.AspNet.Identity;

using Enivate.ResponseHub.Model.Identity;
using Enivate.ResponseHub.UI.Filters;
using Enivate.ResponseHub.UI.Areas.Admin.Models.Users;
using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Model.Identity.Interface;
using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Model;

namespace Enivate.ResponseHub.UI.Areas.Admin.Controllers
{

	[RouteArea("admin")]
	[RoutePrefix("users")]
	[ClaimsAuthorize(Roles = RoleTypes.SystemAdministrator)]
	public class UsersController : Controller
	{

		private IUserService _userService;
		protected IUserService UserService
		{
			get
			{
				return _userService ?? (_userService = UnityConfiguration.Container.Resolve<IUserService>());
			}
		}

		private IMailService _mailService;
		protected IMailService MailService
		{
			get
			{
				return _mailService ?? (_mailService = UnityConfiguration.Container.Resolve<IMailService>());
			}
		}

		private ILogger _log;
		protected ILogger Log
		{
			get
			{
				return _log ?? (_log = UnityConfiguration.Container.Resolve<ILogger>());
			}
		}

		// GET: Admin/Users
		[Route]
		public ActionResult Index()
        {
            return View();
        }

		#region Create user

		[Route("create")]
		public ActionResult Create()
		{
			return View();
		}

		[Route("create")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Create(NewAdminUserViewModel model)
		{
			// If the model is not valid, return
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			// Create the administrator user
			IdentityUser newUser = await UserService.CreateAsync(model.EmailAddress, model.FirstName, model.Surname, new List<string>() { RoleTypes.SystemAdministrator });

			// Send the email
			await MailService.SendAccountActivationEmail(newUser);

			// redirect back
			return new RedirectResult("/admin/users?created=1");

		}

		#endregion
	}
}