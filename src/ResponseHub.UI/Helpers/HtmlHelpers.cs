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

	}
}