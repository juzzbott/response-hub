using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Areas.Admin.Models.Users
{
	public class UserViewModel
	{

		public Guid Id { get; set; }

		public string EmailAddress { get; set; }

		public string FirstName { get; set; }

		public string Surname { get; set; }

		public string FullName { get; set; }

		public bool IsSystemAdmin { get; set; }

		public bool IsUnitAdmin { get; set; }

	}
}