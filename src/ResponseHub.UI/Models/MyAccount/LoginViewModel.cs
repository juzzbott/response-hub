using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Models.MyAccount
{
	public class LoginViewModel
	{

		[Required(ErrorMessage = "You must enter your email address.")]
		[EmailAddress(ErrorMessage = "You must enter a valid email address.")]
		[Display(Name = "Email address")]
		public string Email { get; set; }

		[Required(ErrorMessage = "You must enter an account password.")]
		public string Password { get; set; }

	}
}