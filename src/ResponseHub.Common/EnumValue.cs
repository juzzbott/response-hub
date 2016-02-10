using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Common
{
	public class EnumValue
	{

		/// <summary>
		/// Gets the description of the enum from the Description attribute if it exists. 
		/// If no description exists, then the name of the enum object is returned.
		/// </summary>
		/// <param name="enumObject"></param>
		/// <returns></returns>
		public static string GetEnumDescription(object enumObject)
		{
			// If the enumObject is not an enum, then return empty string
			if (enumObject == null || !enumObject.GetType().IsEnum)
			{
				return String.Empty;
			}

			// Get the enum field.
			FieldInfo fi = enumObject.GetType().GetField(enumObject.ToString());

			// Get the Description attribute
			IList<DescriptionAttribute> attributes = fi.GetCustomAttributes<DescriptionAttribute>(false).ToList();

			// If the attributes is null, or there are none, just return the value of the enum
			if (attributes == null || attributes.Count == 0)
			{
				return Enum.GetName(enumObject.GetType(), enumObject);
			}

			// Get the first attribute as the DescriptionAttribute and return the value
			DescriptionAttribute descAttr = (DescriptionAttribute)attributes.First();

			// return the description
			return descAttr.Description;
		}

	}
}
