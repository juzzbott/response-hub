using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

using Enivate.ResponseHub.Common.Extensions;
using System.Security.Claims;
using Enivate.ResponseHub.Model.Identity;

namespace Enivate.ResponseHub.UI.Helpers
{
	public static class HtmlHelpers
	{

		public static MvcHtmlString EnumDescription(this HtmlHelper helper, Enum enumObject)
		{
			return new MvcHtmlString(enumObject.GetEnumDescription());
		}

		public static MvcHtmlString BackUrlOrDefault(this HtmlHelper helper, string defaultUrl)
		{
			// By default, set to the default Url
			string url = defaultUrl;

			// If the context and url referrer is set, then use that url.
			if (HttpContext.Current != null && HttpContext.Current.Request.UrlReferrer != null)
			{
				url = HttpContext.Current.Request.UrlReferrer.PathAndQuery;
			}

			// return the url
			return new MvcHtmlString(url);

		}

		public static bool IsAdminUser(this HtmlHelper helper)
		{
			// If the user is null or not authenticated, then just return false
			if (HttpContext.Current == null)
			{
				return false;
			}

			if (HttpContext.Current.User == null || !HttpContext.Current.User.Identity.IsAuthenticated)
			{
				return false;
			}

			// Get the identity as a claims identity
			ClaimsIdentity identity = (ClaimsIdentity)HttpContext.Current.User.Identity;

			return identity.Claims.Any(i => i.Type.Equals(ClaimTypes.Role, StringComparison.CurrentCultureIgnoreCase) && i.Value.Equals(RoleTypes.SystemAdministrator, StringComparison.CurrentCultureIgnoreCase));

		}

	}
}