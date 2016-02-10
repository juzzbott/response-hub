using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace Enivate.ResponseHub.UI.Filters
{
	public class ClaimsAuthorizeAttribute: AuthorizeAttribute
	{

		private string _claimType;
		private IList<string> _claimValues;

		public ClaimsAuthorizeAttribute()
		{
			_claimValues = new List<string>();
		}
				
		public ClaimsAuthorizeAttribute(string claimType, params string[] claimValues)
		{

			// Instantiate the claim values list
			_claimValues = new List<string>();

			// Set the claim types
			_claimType = claimType;
			foreach(string claimValue in claimValues)
			{
				_claimValues.Add(claimValue);
			}
		}

		public override void OnAuthorization(AuthorizationContext filterContext)
		{

			// Determine if we need to skip authentication.
			bool skipAuthorization = filterContext.ActionDescriptor.IsDefined(typeof(AllowAnonymousAttribute), inherit: true)
									 || filterContext.ActionDescriptor.ControllerDescriptor.IsDefined(typeof(AllowAnonymousAttribute), inherit: true);

			// If we need to skip authentication, just return.
			if (skipAuthorization)
			{
				return;
			}


			// Get the current principal
			ClaimsPrincipal currentPrincipal = (ClaimsPrincipal)filterContext.HttpContext.User;

			if (!String.IsNullOrEmpty(_claimType))
			{

				// Set the default claim exists
				bool claimExists = false;

				foreach (string claimValue in _claimValues)
				{
					// If the current user has the claim with the specified type and value, set claimExists to true and exit the look
					if (currentPrincipal.HasClaim(_claimType, claimValue))
					{
						claimExists = true;
						break;
					}
				}

				// If no claims have been found, check the Roles property. Split the roles by , and check for claims with the specific role name
				if (!claimExists && !String.IsNullOrEmpty(Roles))
				{
					foreach (string role in Roles.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
					{
						if (currentPrincipal.HasClaim(ClaimTypes.Role, role))
						{
							claimExists = true;
							break;
						}
					}
				}

				// If the claim exists, then continue with the rest of the authorization checks, otherwise handle not authorized.
				if (claimExists)
				{
					// ** IMPORTANT **
					// Since we're performing authorization at the action level, the authorization code runs
					// after the output caching module. In the worst case this could allow an authorized user
					// to cause the page to be cached, then an unauthorized user would later be served the
					// cached page. We work around this by telling proxies not to cache the sensitive page,
					// then we hook our custom authorization code into the caching mechanism so that we have
					// the final say on whether a page should be served from the cache.

					HttpCachePolicyBase cachePolicy = filterContext.HttpContext.Response.Cache;
					cachePolicy.SetProxyMaxAge(new TimeSpan(0));
					cachePolicy.AddValidationCallback(CacheValidateHandler, null /* data */);
				}
				else
				{
					base.HandleUnauthorizedRequest(filterContext);
				}

			}
			else
			{
				base.OnAuthorization(filterContext);
			}

		}

		private void CacheValidateHandler(HttpContext context, object data, ref HttpValidationStatus validationStatus)
		{
			validationStatus = OnCacheAuthorization(new HttpContextWrapper(context));
		}

	}
}