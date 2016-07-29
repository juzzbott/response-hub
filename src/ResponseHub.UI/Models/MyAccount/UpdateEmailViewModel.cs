using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Models.MyAccount
{
	public class UpdateEmailViewModel
	{

		[Required(ErrorMessage = "Please enter the users email address")]
		[EmailAddress(ErrorMessage = "You must enter a valid email address.")]
		public string EmailAddress { get; set; }

		[Required(ErrorMessage = "Please enter a new account password")]
		public string Password { get; set; }
	}
}