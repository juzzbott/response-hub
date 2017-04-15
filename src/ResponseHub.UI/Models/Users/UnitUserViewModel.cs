using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Enivate.ResponseHub.Model.Identity;

namespace Enivate.ResponseHub.UI.Models.Users
{
	public class UnitUserViewModel
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

	}
}