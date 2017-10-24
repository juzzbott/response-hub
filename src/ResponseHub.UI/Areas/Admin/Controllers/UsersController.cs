using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

using Unity;

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
		public async Task<ActionResult> Index()
		{

			// Create the list of users
			List<IdentityUser> users = new List<IdentityUser>();

			if (String.IsNullOrEmpty(Request.QueryString["q"]))
			{
				// Get all the users for the unit
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
			foreach (IdentityUser user in users)
			{

				// Create the user model object from the identity user.
				UserViewModel userModel = new UserViewModel()
				{
					EmailAddress = user.EmailAddress,
					FirstName = user.FirstName,
					FullName = user.FullName,
					Id = user.Id,
					IsUnitAdmin = user.Claims.Any(i => i.Type == ClaimTypes.Role && i.Value.Equals(RoleTypes.UnitAdministrator, StringComparison.CurrentCultureIgnoreCase)),
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

			try
			{

				bool emailExists = await UserService.EmailAddressExists(model.EmailAddress);

				// If the email address exists, show the error to the user
				if (emailExists)
				{
					ModelState.AddModelError("", "Sorry, there is already an account with this email address. System administrators cannot also be unit members. Please try a different email address.");
					return View(model);
				}

				// Create the profile
				UserProfile profile = new UserProfile();

				// Create the administrator user
				IdentityUser newUser = await UserService.CreateAsync(model.EmailAddress, model.FirstName, model.Surname, new List<string>() { RoleTypes.SystemAdministrator }, profile, true);

				// Send the email
				await MailService.SendAccountActivationEmail(newUser);

				// redirect back
				return new RedirectResult("/admin/users?created=1");

			}
			catch (Exception ex)
			{
				await Log.Error("Error creating new user. Message: " + ex.Message, ex);
				ModelState.AddModelError("", "There was a system error creating the new user.");
				return View(model);
			}

		}

		#endregion

		#region Edit user

		[Route("{id:guid}")]
		public async Task<ActionResult> EditUser(Guid id)
		{

			// Get the user
			IdentityUser user = await UserService.FindByIdAsync(id);

			// If the user is null, throw 404
			if (user == null)
			{
				throw new HttpException((int)HttpStatusCode.NotFound, "Not found.");
			}

			// Create the edit user view model
			EditUserViewModel model = new EditUserViewModel()
			{
				EmailAddress = user.EmailAddress,
				FirstName = user.FirstName,
				MemberNumber = user.Profile?.MemberNumber,
				Surname = user.Surname,
				UserHasProfile = !user.Claims.Any(i => i.Type == ClaimTypes.Role && i.Value.ToLower() == RoleTypes.SystemAdministrator.ToLower())
			};

			// return the view
			return View(model);

		}

		[Route("{id:guid}")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> EditUser(Guid id, EditUserViewModel model)
		{
			// Get the user
			IdentityUser user = await UserService.FindByIdAsync(id);

			// If the user is null, throw 404
			if (user == null)
			{
				throw new HttpException((int)HttpStatusCode.NotFound, "Not found.");
			}

			// Get the existing email address
			string existingEmailAddress = user.EmailAddress;

			// Set the "UserHasProfile" object
			model.UserHasProfile = !user.Claims.Any(i => i.Type == ClaimTypes.Role && i.Value.ToLower() == RoleTypes.SystemAdministrator.ToLower());

			// Ensure the model is valid
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			try
			{

				// Set the profile data
				if (model.UserHasProfile)
				{
					user.Profile.MemberNumber = model.MemberNumber;
				}

				// Save the user
				await UserService.UpdateAccountDetails(id, model.FirstName, model.Surname, model.EmailAddress, user.Profile);

				try
				{

					// If the email address has changed, then send the email notification
					if (existingEmailAddress.Equals(user.EmailAddress, StringComparison.CurrentCultureIgnoreCase))
					{
						// Set the properties of the user.
						user.FirstName = model.FirstName;
						user.Surname = model.Surname;
						user.EmailAddress = model.EmailAddress;
						user.UserName = model.EmailAddress;

						// Send the email
						await MailService.SendAccountEmailChangedEmail(user, existingEmailAddress);
					}

				}
				catch (Exception ex)
				{

					await Log.Error(String.Format("Error sending account email changed email. Message: {0}", ex.Message), ex);
					ModelState.AddModelError("", "The user account details were updated successfully however there was a system error sending the notification email.");
					return View(model);
				}

				// Redirect back to the users list
				return new RedirectResult("/admin/users");

			}
			catch (Exception ex)
			{
				await Log.Error(String.Format("Error saving user. Message: {0}", ex.Message), ex);
				ModelState.AddModelError("", "There was a system error saving the changes to the user.");
				return View(model);
			}

		}

		#endregion
	}
}