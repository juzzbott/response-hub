using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

using Enivate.ResponseHub.Common.Extensions;
using System.Security.Claims;
using Enivate.ResponseHub.Model.Identity;

namespace Enivate.ResponseHub.UI.Helpers
{
	public static class HtmlHelpers
	{

		/// <summary>
		/// Gets the enum description for the enum value.
		/// </summary>
		/// <param name="helper"></param>
		/// <param name="enumObject"></param>
		/// <returns></returns>
		public static MvcHtmlString EnumDescription(this HtmlHelper helper, Enum enumObject)
		{
			return new MvcHtmlString(enumObject.GetEnumDescription());
		}

		/// <summary>
		/// Gets the url for the back button or uses default if no referrer info is available.
		/// </summary>
		/// <param name="helper"></param>
		/// <param name="defaultUrl"></param>
		/// <returns></returns>
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

		/// <summary>
		/// Determines if the currenly logged in user an admin user.
		/// </summary>
		/// <param name="helper"></param>
		/// <returns></returns>
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

		public static MvcHtmlString SuccessFromQueryString(this HtmlHelper helper, string queryStringKey, string queryStringValue, string message)
		{

			// Get the current request
			HttpRequest request = HttpContext.Current.Request;

			// If query string key is present and equals the value, then display the message
			if (request != null && !String.IsNullOrEmpty(request.QueryString[queryStringKey]) && request.QueryString[queryStringKey].Equals(queryStringValue, StringComparison.CurrentCultureIgnoreCase))
			{

				// Create the markup.
				StringBuilder sbMarkup = new StringBuilder();
				sbMarkup.AppendLine("<div class=\"row\">");
				sbMarkup.AppendLine("<div class=\"col-sm-12\">");
				sbMarkup.AppendLine("<p class=\"alert alert-success alert-dismissable\" role=\"alert\">");
				sbMarkup.AppendLine("<button type=\"button\" class=\"close\" data-dismiss=\"alert\" aria-label=\"Close\"><span aria-hidden=\"true\">&times;</span></button>");
				sbMarkup.AppendLine(message);
				sbMarkup.AppendLine("</p>");
				sbMarkup.AppendLine("</div>");
				sbMarkup.AppendLine("</div>");

				// return the mvc html string
				return new MvcHtmlString(sbMarkup.ToString());

			}
			else
			{
				// Just return an empty string
				return MvcHtmlString.Empty;
			}
		}

	}
}