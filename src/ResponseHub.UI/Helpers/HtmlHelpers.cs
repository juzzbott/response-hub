using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

using Enivate.ResponseHub.Common.Extensions;
using System.Security.Claims;
using Enivate.ResponseHub.Model.Identity;
using System.Web.Routing;
using Enivate.ResponseHub.Common.Constants;
using Microsoft.AspNet.Identity;
using Enivate.ResponseHub.Model.Groups.Interface;
using Enivate.ResponseHub.Common;
using System.Threading.Tasks;

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


		/// <summary>
		/// Gets the full name of the currently logged in user
		/// </summary>
		/// <param name="helper"></param>
		/// <returns></returns>
		public static string UserDisplayName(this HtmlHelper helper)
		{
			// If the user is null or not authenticated, then just return false
			if (HttpContext.Current == null)
			{
				return "";
			}

			if (HttpContext.Current.User == null || !HttpContext.Current.User.Identity.IsAuthenticated)
			{
				return "";
			}

			// Get the identity as a claims identity
			ClaimsIdentity identity = (ClaimsIdentity)HttpContext.Current.User.Identity;

			// Get the user display name claim
			Claim userDisplayName = identity.Claims.FirstOrDefault(i => i.Type == CustomClaimTypes.UserDisplayName);

			return (userDisplayName != null ? userDisplayName.Value : "");
			
		}

		/// <summary>
		/// Determines if the currenly logged in user an group administrator user.
		/// </summary>
		/// <param name="helper"></param>
		/// <returns></returns>
		public static bool IsGroupAdminUser(this HtmlHelper helper)
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

			return identity.Claims.Any(i => i.Type.Equals(ClaimTypes.Role, StringComparison.CurrentCultureIgnoreCase) && i.Value.Equals(RoleTypes.GroupAdministrator, StringComparison.CurrentCultureIgnoreCase));

		}

		public static bool IsGroupAdminUserOfMultipleGroups(this HtmlHelper helper)
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

			// Get the user id from the claim
			Guid userId = new Guid(identity.GetUserId());

			// Get the group service
			IGroupService groupService = ServiceLocator.Get<IGroupService>();

			IList<Guid> userGroupIds = null;
			int groupCount = 0;
			Task t = Task.Run(async () =>
			{
				userGroupIds = await groupService.GetGroupIdsUserIsGroupAdminOf(userId);
				groupCount = userGroupIds.Count;
			});
			t.Wait();

			return groupCount > 1;
		}

		/// <summary>
		/// Displays a success message based on the result from a query string
		/// </summary>
		/// <param name="helper"></param>
		/// <param name="queryStringKey"></param>
		/// <param name="queryStringValue"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		public static MvcHtmlString SuccessFromQueryString(this HtmlHelper helper, string queryStringKey, string queryStringValue, string message)
		{
			// return the message
			return MessageFromQueryString(queryStringKey, queryStringValue, message, "alert-success");
		}

		/// <summary>
		/// Displays a error message based on the result from a query string
		/// </summary>
		/// <param name="helper"></param>
		/// <param name="queryStringKey"></param>
		/// <param name="queryStringValue"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		public static MvcHtmlString ErrorFromQueryString(this HtmlHelper helper, string queryStringKey, string queryStringValue, string message)
		{
			// return the message
			return MessageFromQueryString(queryStringKey, queryStringValue, message, "alert-danger");
		}

		/// <summary>
		/// Displays a warning message based on the result from a query string
		/// </summary>
		/// <param name="helper"></param>
		/// <param name="queryStringKey"></param>
		/// <param name="queryStringValue"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		public static MvcHtmlString WarningFromQueryString(this HtmlHelper helper, string queryStringKey, string queryStringValue, string message)
		{
			// return the message
			return MessageFromQueryString(queryStringKey, queryStringValue, message, "alert-warning");
		}

		private static MvcHtmlString MessageFromQueryString(string queryStringKey, string queryStringValue, string message, string className)
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
				sbMarkup.AppendLine(String.Format("<p class=\"alert {0} alert-dismissable\" role=\"alert\">", className));
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

		public static MvcHtmlString BreadcrumbControllerLink(this HtmlHelper helper)
		{

			// If the current action name is index, we just need to return the friendly name of the controller
			if (helper.ViewContext.RouteData.Values["Action"].ToString().Equals("index", StringComparison.CurrentCultureIgnoreCase))
			{
				return new MvcHtmlString(SpaceBeforeCapital(helper.ViewContext.RouteData.Values["Controller"].ToString(), true));
			}
			else
			{

				// Get the route as a route
				Route route = (Route)helper.ViewContext.RouteData.Route;

				string url = route.Url;

				// Remove the last part of the url
				int lastIndex = url.LastIndexOf('/');
				url = url.Substring(0, lastIndex);

				// Set the url based on the prefix value
				return new MvcHtmlString(String.Format("<a href=\"/{0}\">{1}</a>",
					url,
					SpaceBeforeCapital(helper.ViewContext.RouteData.Values["Controller"].ToString(), true)));

			}
			
		}

		public static MvcHtmlString BreadcrumbAreaLink(this HtmlHelper helper)
		{
			// Get the route as a route
			Route route = (Route)helper.ViewContext.RouteData.Route;

			// Get the area url from the route url
			string areaUrl = route.Url;
			
			// Get the first index of the / for the area
			int index = route.Url.IndexOf('/');
			if (index > 0)
			{
				areaUrl = areaUrl.Substring(0, index);
			}

			// Set the url based on the prefix value
			return new MvcHtmlString(String.Format("<a href=\"/{0}\">{1}</a>",
				areaUrl,
				SpaceBeforeCapital(helper.ViewContext.RouteData.DataTokens["area"].ToString(), true)));
		}

		public static MvcHtmlString GetFileSizeDisplayForBytes(this HtmlHelper helper, long bytes)
		{
			if (bytes < 1)
			{
				return new MvcHtmlString("0 kb");
			}
			else if (bytes < 1000000)
			{
				decimal kb = (bytes / 1000M);
				return new MvcHtmlString(String.Format("{0} kb", Math.Round(kb, 2)));
			}
			else
			{
				decimal mb = (bytes / 1000000M);
				return new MvcHtmlString(String.Format("{0} mb", Math.Round(mb, 2)));
			}
		}

		/// <summary>
		/// Add a space char before each capital letter and trims leading and trailing whitespace
		/// </summary>
		/// <param name="str">The string to add spaces to.</param>
		/// <returns></returns>
		private static string SpaceBeforeCapital(string str, bool enforceFirstCapital)
		{
			str = Regex.Replace(str, "([A-Z])", " $1").Trim();

			// Ensure the first letter is capital
			if (enforceFirstCapital)
			{
				if (Regex.IsMatch(str, "^[a-z].*"))
				{
					str = String.Format("{0}{1}", str.Substring(0, 1).ToUpper(), str.Substring(1));
				}
			}

			return str;
		}
	}
}