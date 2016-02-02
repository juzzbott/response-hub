using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Enivate.ResponseHub.UI.Controllers
{

	[RoutePrefix("my-account")]
	[Authorize]
    public class MyAccountController : Controller
    {
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
		//[Route("login")]
		//[HttpPost]
		//[ValidateAntiForgeryToken]
		//[AllowAnonymous]
		//public async Task<ActionResult> Login(LoginViewModel model)
		//{
		//	if (!ModelState.IsValid)
		//	{
		//		return View(model);
		//	}
		//
		//	try
		//	{
		//
		//		// Log the user in
		//		IApplicationUser user;
		//		SignInStatus result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, true, false);
		//
		//		// Determine the result of the sign in
		//		switch (result)
		//		{
		//
		//			case SignInStatus.Success:
		//				user = await UserManager.FindByNameAsync(model.Email);
		//				_eventLog.LogEvent(EventTypes.LOGIN, "User '{0}' logged in successfully.", user.Id);
		//
		//				string redirectTo = getReturnUrl("/");
		//
		//				return new RedirectResult(redirectTo);
		//
		//			case SignInStatus.LockedOut:
		//				user = await UserManager.FindByNameAsync(model.Email);
		//				_eventLog.LogEvent(EventTypes.LOGIN, "User '{0}' unable to login. Account locked.", user.Id);
		//				return View("LockedOut");
		//
		//			case SignInStatus.Failure:
		//			default:
		//				_eventLog.LogEvent(EventTypes.LOGIN, "Failed login attempt. Email: {0} | User IP: {1} | User Agent: {2}", model.Email, Request.UserHostAddress, Request.UserAgent);
		//				ViewBag.InvalidLogin = true;
		//				return View(model);
		//		}
		//
		//	}
		//	catch (Exception ex)
		//	{
		//		_log.Error(String.Format("There was an error logging in user: '{0}'.", model.Email), ex);
		//		ModelState.AddModelError("", "There was a system error trying to log you in.");
		//		return View(model);
		//	}
		//}

		#endregion

	}
}