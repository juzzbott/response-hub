using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Enivate.ResponseHub.Model.Identity;

namespace Enivate.ResponseHub.UI.Models.Users
{
	public class UnitMemberViewModel
	{

		public Guid Id { get; set; }

		public string FirstName { get; set; }

		public string Surname { get; set; }

		public string FullName
		{
			get
			{
				return String.Format("{0} {1}", FirstName, Surname);
			}
		}

		public string EmailAddress { get; set; }

		public String UnitRole { get; set; }

		public UserProfile Profile { get; set; }

		public UserStatus Status { get; set; }

		/// <summary>
		/// Maps the identity user to the unit member view model
		/// </summary>
		/// <param name="user">The identity user to map.</param>
		/// <returns></returns>
		public static UnitMemberViewModel FromIdentityUserWithoutRole(IdentityUser user)
		{
			
			// If the user is null, just return null
			if (user == null)
			{
				return null;
			}

			// return the mapped view model
			return new UnitMemberViewModel()
			{
				EmailAddress = user.EmailAddress,
				FirstName = user.FirstName,
				Id = user.Id,
				Profile = user.Profile,
				Status = user.Status,
				Surname = user.Surname
			};
		}

	}
}