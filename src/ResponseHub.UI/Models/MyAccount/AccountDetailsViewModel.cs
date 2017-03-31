using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

using Enivate.ResponseHub.Model.Identity;

namespace Enivate.ResponseHub.UI.Models.MyAccount
{
	public class AccountDetailsViewModel
	{

		[Required(ErrorMessage = "Please enter the users email address")]
		[EmailAddress(ErrorMessage = "You must enter a valid email address.")]
		public string EmailAddress { get; set; }

		[Required(ErrorMessage = "Please enter the first name for the user")]
		public string FirstName { get; set; }

		public UserProfile Profile { get; set; }

		[Required(ErrorMessage = "Please enter the surname for the user")]
		public string Surname { get; set; }

		public DateTime Created { get; set; }

		public bool CanChangePassword { get; set; }

	}
}