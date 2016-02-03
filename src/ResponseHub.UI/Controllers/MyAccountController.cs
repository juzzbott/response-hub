﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using Microsoft.Practices.Unity;

using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;

using Enivate.ResponseHub.ApplicationServices.Identity;
using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Model.Identity;
using Enivate.ResponseHub.UI.Models.MyAccount;

namespace Enivate.ResponseHub.UI.Controllers
{

	[RoutePrefix("my-account")]
	[Authorize]
    public class MyAccountController : Controller
    {

		private ILogger _log;
		protected ILogger Log
		{
			get
			{
				return _log ?? (_log = UnityConfiguration.Container.Resolve<ILogger>());
			}
		}

		private SignInManager<IdentityUser, Guid> _signInManager;
		protected SignInManager<IdentityUser, Guid> SignInManager
		{
			get
			{
				return _signInManager ?? (_signInManager = HttpContext.GetOwinContext().Get<SignInManager<IdentityUser, Guid>>());
			}
		}

		private UserService _userService;
		protected UserService UserService
		{
			get
			{
				return _userService ?? (_userService = HttpContext.GetOwinContext().Get<UserService>());
			}
		}

		// GET: MyAccount
		public ActionResult Index()
        {
            return View();
        }

		#region Login

		// GET: /my-account/login
		[Route("login")]
		[AllowAnonymous]
		public ActionResult Login()
		{
			return View();
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
		
						string redirectTo = GetReturnUrl("/");
		
						return new RedirectResult(redirectTo);
		
					case SignInStatus.LockedOut:
						user = await UserService.FindByNameAsync(model.Email);
						// TODO: Event Log - _eventLog.LogEvent(EventTypes.LOGIN, "User '{0}' unable to login. Account locked.", user.Id);
						return View("LockedOut");
		
					case SignInStatus.Failure:
					default:
						// TODO: Event Log - _eventLog.LogEvent(EventTypes.LOGIN, "Failed login attempt. Email: {0} | User IP: {1} | User Agent: {2}", model.Email, Request.UserHostAddress, Request.UserAgent);
						ViewBag.InvalidLogin = true;
						return View(model);
				}
		
			}
			catch (Exception ex)
			{
				await _log.Error(String.Format("There was an error logging in user: '{0}'.", model.Email), ex);
				ModelState.AddModelError("", "There was a system error trying to log you in.");
				return View(model);
			}
		}

		#endregion

		#region Helpers

		/// <summary>
		/// Gets the return url for the application to return you too after login or register.
		/// </summary>
		/// <param name="defaultUrl"></param>
		/// <returns></returns>
		private string GetReturnUrl(string defaultUrl)
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

			// By default, return the default url
			return defaultUrl;

		}

		#endregion

	}
}