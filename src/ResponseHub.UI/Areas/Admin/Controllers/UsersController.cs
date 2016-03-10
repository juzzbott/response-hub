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
using System.Security.Claims;

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
		public async Task<ActionResult> Index()
        {

			// Create the list of users
			List<IdentityUser> users = new List<IdentityUser>();

			if (String.IsNullOrEmpty(Request.QueryString["q"]))
			{
				// Get all the users for the group
				users.AddRange(await UserService.GetAll());
			}
			else
			{

				// Get the search results
				IList<IdentityUser> searchResults = await UserService.SearchUsers(Request.QueryString["q"]);

				// Add the search results to the users list
				users.AddRange(searchResults);
			}

			// Get the list of user view models from the collection of identity users.
			IList<UserViewModel> userModels = new List<UserViewModel>();
			foreach(IdentityUser user in users)
			{

				// Create the user model object from the identity user.
				UserViewModel userModel = new UserViewModel()
				{
					EmailAddress = user.EmailAddress,
					FirstName = user.FirstName,
					FullName = user.FullName,
					Id = user.Id,
					IsGroupAdmin = user.Claims.Any(i => i.Type == ClaimTypes.Role && i.Value.Equals(RoleTypes.GroupAdministrator, StringComparison.CurrentCultureIgnoreCase)),
					IsSystemAdmin = user.Claims.Any(i => i.Type == ClaimTypes.Role && i.Value.Equals(RoleTypes.SystemAdministrator, StringComparison.CurrentCultureIgnoreCase)),
					Surname = user.Surname
				};
				userModels.Add(userModel);
			}

			// return the view.
			return View(userModels.OrderBy(i => i.FullName).ThenByDescending(i => i.IsSystemAdmin).ToList());
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

			bool emailExists = await UserService.EmailAddressExists(model.EmailAddress);

			// If the email address exists, show the error to the user
			if (emailExists)
			{
				ModelState.AddModelError("", "Sorry, there is already an account with this email address. System administrators cannot also be group members. Please try a different email address.");
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