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
			// If the enumObject is not an enum, then return empty string
			if (enumObject == null || !enumObject.GetType().IsEnum)
			{
				return MvcHtmlString.Empty;
			}

			// Get the enum field.
			FieldInfo fi = enumObject.GetType().GetField(enumObject.ToString());

			// Get the Description attribute
			IList<DescriptionAttribute> attributes = fi.GetCustomAttributes<DescriptionAttribute>(false).ToList();

			// If the attributes is null, or there are none, just return the value of the enum
			if (attributes == null || attributes.Count == 0)
			{
				return new MvcHtmlString(Enum.GetName(enumObject.GetType(), enumObject));
			}

			// Get the first attribute as the DescriptionAttribute and return the value
			DescriptionAttribute descAttr = (DescriptionAttribute)attributes.First();

			// return the description
			return new MvcHtmlString(descAttr.Description);

		}

	}
}