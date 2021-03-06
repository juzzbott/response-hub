using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Enivate.ResponseHub.Model.Identity;

namespace Enivate.ResponseHub.UI.Models.Profile
{
	public class ProfileViewModel
	{

		public string FirstName { get; set; }

		public string Surname { get; set; }

		public string FullName
		{
			get
			{
				return String.Format("{0} {1}", FirstName, Surname);
			}
		}

		public DateTime DateCreated { get; set; }

		public UserProfile Profile { get; set; }

	}
}