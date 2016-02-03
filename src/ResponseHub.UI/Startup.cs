using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;

using Owin;

using Microsoft.Practices.Unity;

using Enivate.ResponseHub.ApplicationServices.Identity;
using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Model.Identity;
using Enivate.ResponseHub.Model.Identity.Interface;
using Microsoft.Owin.Security.DataProtection;

[assembly: OwinStartupAttribute(typeof(Enivate.ResponseHub.UI.Startup))]
namespace Enivate.ResponseHub.UI
{
	public class Startup
	{
	
		public void Configuration(IAppBuilder app)
		{
			ConfigureAuth(app);
		}

		public void ConfigureAuth(IAppBuilder app)
		{
			// Configure the db context, user manager and signin manager to use a single instance per request
			app.CreatePerOwinContext<UserService>(CreateUserService);
			app.CreatePerOwinContext<SignInManager<IdentityUser, Guid>>(CreateSignInManager);

			// Enable the application to use a cookie to store information for the signed in user
			// and to use a cookie to temporarily store information about a user logging in with a third party login provider
			// Configure the sign in cookie
			app.UseCookieAuthentication(new CookieAuthenticationOptions
			{
				AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
				LoginPath = new PathString("/my-account/login"),
				Provider = new CookieAuthenticationProvider
				{
					// Enables the application to validate the security stamp when the user logs in.
					// This is a security feature which is used when you change a password or add an external login to your account.  
					//sOnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
					//s	validateInterval: TimeSpan.FromMinutes(30),
					//s	regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager))
				}
			});
			app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);
		}

		/// <summary>
		/// Creates a new instance of the UserService class for the IOwinContext
		/// </summary>
		/// <param name="options"></param>
		/// <param name="context"></param>
		/// <returns></returns>
		public static UserService CreateUserService(IdentityFactoryOptions<UserService> options, IOwinContext context)
		{

			// Create the manager object
			IUserRepository repository = UnityConfiguration.Container.Resolve<IUserRepository>();
			ILogger logger = UnityConfiguration.Container.Resolve<ILogger>();
			UserService manager = new UserService(repository, logger);

			// disable user lock by default
			manager.UserLockoutEnabledByDefault = false;
			manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(30);
			manager.MaxFailedAccessAttemptsBeforeLockout = 5;

			// Create the data protection provider.
			IDataProtectionProvider dataProtectionProvider = options.DataProtectionProvider;
			if (dataProtectionProvider != null)
			{
				manager.UserTokenProvider = new DataProtectorTokenProvider<IdentityUser, Guid>(dataProtectionProvider.Create("ASP.NET Identity"));
			}

			// Return the application user manager object.
			return manager;

		}

		/// <summary>
		/// Creates a new sign in manager for the current IOwinContext.
		/// </summary>
		/// <param name="options"></param>
		/// <param name="context"></param>
		/// <returns></returns>
		public static SignInManager<IdentityUser, Guid> CreateSignInManager(IdentityFactoryOptions<SignInManager<IdentityUser, Guid>> options, IOwinContext context)
		{
			return new SignInManager<IdentityUser, Guid>(context.GetUserManager<UserService>(), context.Authentication);
		}

	}
}