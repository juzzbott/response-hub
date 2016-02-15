using Enivate.ResponseHub.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace Enivate.ResponseHub.UI.Helpers
{
	public static class HtmlHelpers
	{

		public static MvcHtmlString EnumDescription(this HtmlHelper helper, object enumObject)
		{
			return new MvcHtmlString(EnumValue.GetEnumDescription(enumObject));
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

	}
}