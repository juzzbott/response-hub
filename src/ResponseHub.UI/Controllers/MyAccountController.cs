using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using Microsoft.Practices.Unity;

using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;

using Enivate.ResponseHub.ApplicationServices;
using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Model.Identity;
using Enivate.ResponseHub.UI.Models.MyAccount;
using System.Security.Claims;
using System.Net;

namespace Enivate.ResponseHub.UI.Controllers
{

	[RoutePrefix("my-account")]
	public class MyAccountController : BaseController
	{
		
		private SignInManager<IdentityUser, Guid> _signInManager;
		protected SignInManager<IdentityUser, Guid> SignInManager
		{
			get
			{
				return _signInManager ?? (_signInManager = HttpContext.GetOwinContext().Get<SignInManager<IdentityUser, Guid>>());
			}
		}
		
		private IAuthenticationManager _authenticationManager;
		protected IAuthenticationManager AuthenticationManager
		{
			get
			{
				return _authenticationManager ?? (_authenticationManager = HttpContext.GetOwinContext().Authentication);
			}
		}
		
		#region Login

		// GET: /my-account/login
		[Route("login")]
		[AllowAnonymous]
		public ActionResult Login()
		{
			return View();
		}

		private static async Task CreateNewUser()
		{
			Enivate.ResponseHub.DataAccess.Interface.IUserRepository userRepo = UnityConfiguration.Container.Resolve<Enivate.ResponseHub.DataAccess.Interface.IUserRepository>();
			Enivate.ResponseHub.Logging.ILogger logger = UnityConfiguration.Container.Resolve<Enivate.ResponseHub.Logging.ILogger>();
			UserService svc = new UserService(userRepo, logger);

			PasswordHasher hasher = new PasswordHasher();
			string passwordHash = hasher.HashPassword("wysiwyg");

			IdentityUser user = new IdentityUser()
			{
				Created = DateTime.UtcNow,
				FirstName = "Justin",
				Surname = "McKay",
				EmailAddress = "juzzbott@gmail.com",
				UserName = "juzzbott@gmail.com",
				PasswordHash = passwordHash
			};

			await svc.CreateAsync(user);

		}

		// POST: /my-account/login
		[Route("login")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		[AllowAnonymous]
		public async Task<ActionResult> Login(LoginViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			try
			{

				// Log the user in
				IdentityUser user;
				SignInStatus result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, true, false);

				// Determine the result of the sign in
				switch (result)
				{

					case SignInStatus.Success:
						user = await UserService.FindByNameAsync(model.Email);
						// TODO: Event Log - _eventLog.LogEvent(EventTypes.LOGIN, "User '{0}' logged in successfully.", user.Id);

						string redirectTo = GetReturnUrl("/", user);

						return new RedirectResult(redirectTo);

					case SignInStatus.LockedOut:
						user = await UserService.FindByNameAsync(model.Email);
						// TODO: Event Log - _eventLog.LogEvent(EventTypes.LOGIN, "User '{0}' unable to login. Account locked.", user.Id);
						return View("LockedOut");

					case SignInStatus.Failure:
					default:
						// TODO: Event Log - _eventLog.LogEvent(EventTypes.LOGIN, "Failed login attempt. Email: {0} | User IP: {1} | User Agent: {2}", model.Email, Request.UserHostAddress, Request.UserAgent);
						ModelState.AddModelError("", "The account details you have entered are invalid.");
						return View(model);
				}

			}
			catch (Exception ex)
			{
				await Log.Error(String.Format("There was an error logging in user: '{0}'.", model.Email), ex);
				ModelState.AddModelError("", "There was a system error trying to log you in.");
				return View(model);
			}
		}

		#endregion

		#region Logout

		[Route("logout")]
		[AllowAnonymous]
		public ActionResult Logout()
		{
			AuthenticationManager.SignOut();
			return new RedirectResult("/my-account/login");
		}

		#endregion

		#region My Account

		[Route]
		public async Task<ActionResult> Index()
		{
			// Get the current user
			IdentityUser currentUser = await GetCurrentUser();

			// If the current user is null, throw bad request exception
			if (currentUser == null)
			{
				throw new HttpException((int)HttpStatusCode.BadRequest, "Unable to process request.");
			}

			// Create the account details view model
			AccountDetailsViewModel model = new AccountDetailsViewModel()
			{
				Created = currentUser.Created.ToLocalTime(),
				EmailAddress = currentUser.EmailAddress,
				FirstName = currentUser.FirstName,
				Surname = currentUser.Surname
			};

			return View(model);
		}

		#endregion

		#region Update Account

		[Route("update-account")]
		public async Task<ActionResult> UpdateAccountDetails()
		{

			// Get the current identity user
			IdentityUser currentUser = await GetCurrentUser();

			// Create the view models
			UpdateAccountViewModel model = new UpdateAccountViewModel()
			{
				FirstName = currentUser.FirstName,
				Surname = currentUser.Surname
			};

			// return the view
			return View(model);

		}

		[Route("update-account")]
		[ValidateAntiForgeryToken]
		[HttpPost]
		public async Task<ActionResult> UpdateAccountDetails(UpdateAccountViewModel model)
		{

			// Ensure the form is valid
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			try
			{
				// Update the user details
				await UserService.UpdateAccountDetails(UserId, model.FirstName, model.Surname);

				// Redirect to the my account screen
				return new RedirectResult("/my-account?account_updated=1");
			}
			catch (Exception ex)
			{
				// Log the error
				await Log.Error(String.Format("Unable to update user account details. Message: {0}", ex.Message), ex);
				ModelState.AddModelError("", "Sorry, there was a system error updating your account details.");
				return View(model);
			}

		}

		[Route("update-email")]
		public async Task<ActionResult> UpdateEmailAddress()
		{

			// Get the current identity user
			IdentityUser currentUser = await GetCurrentUser();

			// Create the model
			UpdateEmailViewModel model = new UpdateEmailViewModel()
			{
				EmailAddress = currentUser.EmailAddress
			};

			return View(model);

		}

		[Route("update-email")]
		[ValidateAntiForgeryToken]
		[HttpPost]
		public async Task<ActionResult> UpdateEmailAddress(UpdateEmailViewModel model)
		{

			// Ensure the form is valid
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			try
			{
				// Update the user details
				IdentityResult result = await UserService.UpdateUserNameAsync(UserId, model.EmailAddress, model.Password);

				if (result.Succeeded)
				{

					// Redirect to the my account screen
					return new RedirectResult("/my-account?email_updated=1");

				}
				else
				{
					// Add the model error
					ModelState.AddModelError("", result.Errors.FirstOrDefault());
					return View(model);
				} 
			}
			catch (Exception ex)
			{
				// Log the error
				await Log.Error(String.Format("Unable to update your email address details. Message: {0}", ex.Message), ex);
				ModelState.AddModelError("", "Sorry, there was a system error updating your account details.");
				return View(model);
			}

		}

		#endregion

		#region Change password

		[Route("change-password")]
		public async Task<ActionResult> ChangePassword()
		{

			// Get the user id and the user entity
			Guid userId = new Guid(User.Identity.GetUserId());
			IdentityUser user = await UserService.FindByIdAsync(userId);

			// If there is no user currently assigned to the user account (i.e. login from Facebook), give the user the option to create a password.
			if (user != null && String.IsNullOrEmpty(user.PasswordHash))
			{
				// Show Create password
				return View("CreatePassword");
			}
			else
			{
				// Show shange password
				return View();
			}
		}

		[Route("change-password")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
		{

			// Ensure the model is valid
			if (!ModelState.IsValid)
			{
				return View();
			}

			// Get the user id and the user entity
			Guid userId = new Guid(User.Identity.GetUserId());
			IdentityUser user = await UserService.FindByIdAsync(userId);

			// Ensure the password is valid
			bool passwordValid = await UserService.CheckPasswordAsync(user, model.AccountPassword);
			if (!passwordValid)
			{
				ModelState.AddModelError("", "Sorry, the account password you have entered is incorrect.");
				return View(model);
			}

			try
			{

				// Change the user password.
				IdentityResult result = await UserService.ChangePasswordAsync(userId, model.AccountPassword, model.NewPassword);

				if (result.Succeeded)
				{

					try
					{
						// Send the outbound message
						//await sendPasswordChangedMessage(user);
					}
					catch (Exception ex)
					{
						await Log.Error("Unable to send password changed mail message.", ex);
					}

					//_eventLog.LogEvent(EventTypes.CHANGE_PASSWORD, "User '{0}' changed password. IP Address of user: {1}", userId, Request.UserHostAddress);
					return new RedirectResult("/my-account/change-password/complete");
				}
				else
				{
					ModelState.AddModelError("", "Sorry, there was an error updating your account password.");
					return View(model);
				}

			}
			catch (Exception ex)
			{
				await Log.Error(String.Format("There was an error updating the account password for user: '{0}'.", userId), ex);
				ModelState.AddModelError("", "Sorry, there was an error updating your account password.");
				return View(model);
			}

		}

		[Route("create-password")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> CreatePassword(CreatePasswordViewModel model)
		{

			// Ensure the model is valid
			if (!ModelState.IsValid)
			{
				return View();
			}

			// Get the user id and the user entity
			Guid userId = new Guid(User.Identity.GetUserId());
			IdentityUser user = await UserService.FindByIdAsync(userId);

			if (user == null || user.PasswordHash != null)
			{
				ModelState.AddModelError("", "Sorry, we are unable to create a password for your account at this time.");
				return View(model);
			}

			try
			{

				// Create the user
				PasswordHasher hasher = new PasswordHasher();
				string passwordHash = hasher.HashPassword(model.NewPassword);

				// Change the user password.
				IdentityResult result = await UserService.CreatePasswordAsync(user, passwordHash);

				if (result.Succeeded)
				{

					try
					{
						// Send the outbound message
						//await sendPasswordCreatedMessage(user);
					}
					catch (Exception ex)
					{
						await Log.Error("Unable to send password created mail message.", ex);
					}

					//_eventLog.LogEvent(EventTypes.CREATE_PASSWORD, "User '{0}' created a password. IP Address of user: {1}", userId, Request.UserHostAddress);
					return new RedirectResult("/my-account/change-password/complete");
				}
				else
				{
					ModelState.AddModelError("", "Sorry, there was an error creating your account password.");
					return View(model);
				}

			}
			catch (Exception ex)
			{
				await Log.Error(String.Format("There was an error creating the account password for user: '{0}'.", userId), ex);
				ModelState.AddModelError("", "Sorry, there was an error creating your account password.");
				return View(model);
			}

		}

		[Route("change-password/complete")]
		public ActionResult ChangePasswordComplete()
		{
			return View();
		}

		#endregion

		#region Activate Account

		[Route("activate/{token:length(64)}")]
		[AllowAnonymous]
		public async Task<ActionResult> ActivateAccount(string token)
		{

			// Get the identity by the activation token
			IdentityUser user = await UserService.GetUserByActivationToken(token);

			// If the user is null, show the invalid token message
			if (user == null)
			{
				ViewBag.InvalidActivationToken = true;
			}
			
			return View();
		}

		[Route("activate/{token:length(64)}")]
		[AllowAnonymous]
		[HttpPost]
		public async Task<ActionResult> ActivateAccount(string token, CreatePasswordViewModel model)
		{

			if (!ModelState.IsValid)
			{
				return View(model);
			}

			// Get the identity by the activation token
			IdentityUser user = await UserService.GetUserByActivationToken(token);

			// If the user is null, show the invalid token message
			if (user == null)
			{
				ViewBag.InvalidActivationToken = true;
				return View();
			}

			// Create the user password
			PasswordHasher hasher = new PasswordHasher();
			string passwordHash = hasher.HashPassword(model.NewPassword);

			// Change the user password.
			IdentityResult result = await UserService.CreatePasswordAsync(user, passwordHash);

			if (result.Succeeded)
			{

				// Activate the account
				await UserService.ActivateAccount(user.Id);

				// Redirect to the complete page.
				return new RedirectResult("/my-account/activate/complete");
			}
			else
			{
				// Log the error and return
				await Log.Error("Unable to activate user account.");
				if (result.Errors.Any())
				{
					await Log.Error(result.Errors.FirstOrDefault());
				}
				ModelState.AddModelError("", "There was a system error activating your account.");
				return View(model);
			}

		}

		[Route("activate/complete")]
		[AllowAnonymous]
		public ActionResult ActivateAccountComplete()
		{
			return View();
		}

		#endregion

		#region Helpers

		/// <summary>
		/// Gets the return url for the application to return you too after login or register.
		/// </summary>
		/// <param name="defaultUrl"></param>
		/// <returns></returns>
		private string GetReturnUrl(string defaultUrl, IdentityUser authenticatedUser)
		{

			// If there is a return url query string, then use that, otherwise redirect to the specified path
			if (!String.IsNullOrEmpty(Request.QueryString["ReturnUrl"]))
			{
				string queryUrl = Server.UrlDecode(Request.QueryString["ReturnUrl"]);

				// If the current url does not start with the return url, then use the return url
				if (!Request.Url.PathAndQuery.ToLower().StartsWith(queryUrl.ToLower()))
				{
					return queryUrl;
				}
			}

			// We don't have any query string return urls, so we can set one here based on role
			// For system admin users, go to the admin section
			if (authenticatedUser != null && authenticatedUser.Claims != null && authenticatedUser.Claims.Any(i => i.Value.Equals(RoleTypes.SystemAdministrator) && i.Type == ClaimTypes.Role))
			{
				return "/admin";
			}

			// By default, return the default url
			return defaultUrl;

		}

		#endregion

	}
}