using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Enivate.ResponseHub.UI.Areas.Admin.Models.Users
{
	public class NewUserViewModel
	{

		[Required(ErrorMessage = "Please enter the users email address")]
		public string EmailAddress { get; set; }

		[Required(ErrorMessage = "Please select a role for the user")]
		public string Role { get; set; }

		public IList<SelectListItem> AvailableRoles { get; set; }

		public NewUserViewModel()
		{
			AvailableRoles = new List<SelectListItem>();
		}

	}
}