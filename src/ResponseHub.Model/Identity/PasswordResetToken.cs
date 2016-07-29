using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Common;

namespace Enivate.ResponseHub.Model.Identity
{
	public class PasswordResetToken
	{

		public string Token { get; set; }

		public DateTime Expires { get; set; }

		/// <summary>
		/// Generates a new reset password token object with the default expiry.
		/// </summary>
		/// <returns></returns>
		public static PasswordResetToken Generate()
		{
			// HACK: Magic numbers
			return Generate(DateTime.UtcNow.AddDays(2));
		}

		/// <summary>
		/// Generates a new reset password token that expires after 'expires'.
		/// </summary>
		/// <param name="expires"></param>
		/// <returns></returns>
		public static PasswordResetToken Generate(DateTime expires)
		{
			// Generate a guid to hash
			string guid = Guid.NewGuid().ToString();

			// Generate the token
			string token = HashGenerator.GetSha256HashString(guid, 1000);

			// Return the new reset password token.
			return new PasswordResetToken()
			{
				Expires = expires,
				Token = token
			};

		}

	}
}
