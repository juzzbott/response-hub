using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Areas.Admin.Models.Users
{
	public class NewAdminUserViewModel
	{

		[Required(ErrorMessage = "Please enter the users email address")]
		[EmailAddress(ErrorMessage = "You must enter a valid email address.")]
		public string EmailAddress { get; set; }

		[Required(ErrorMessage = "Please confirm the users email address")]
		[EmailAddress(ErrorMessage = "You must enter a valid email address.")]
		[Compare("EmailAddress", ErrorMessage = "Your confirmed email address does not match.")]
		public string ConfirmEmailAddress { get; set; }

		[Required(ErrorMessage = "Please enter the first name for the user")]
		public string FirstName { get; set; }

		[Required(ErrorMessage = "Please enter the surname for the user")]
		public string Surname { get; set; }
	}
}