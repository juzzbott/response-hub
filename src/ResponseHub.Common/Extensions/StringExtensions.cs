using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Common.Extensions
{
	public static class StringExtensions
	{

		public static string Truncate(this string str, int length, string appelation)
		{

			// If the string is null or empty, or less than the length required, just return empty string
			// Ensures empty string is returned in place of null.
			if (String.IsNullOrEmpty(str) || str.Length < length)
			{
				return (str != null ? str : "");
			}

			str = str.Substring(0, length);

			// if there is an appelation, append that
			if (!String.IsNullOrEmpty(appelation))
			{
				return String.Format("{0}{1}", str, appelation);
			}
			else
			{
				return str;
			}

		}

		/// <summary>
		/// Add a space char before each capital letter and trims leading and trailing whitespace
		/// </summary>
		/// <param name="str">The string to add spaces to.</param>
		/// <returns></returns>
		public static string SpaceBeforeCapital(this string str, bool enforceFirstCapital)
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
