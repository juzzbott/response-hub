using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Models.MyAccount
{
	public class ResetPasswordViewModel
	{

		[Required(ErrorMessage = "Please enter an account password")]
		public string Password { get; set; }

		[Required(ErrorMessage = "Please confirm your account password.")]
		[Compare("Password", ErrorMessage = "Your confirmed password does not match.")]
		[Display(Name = "Confirm password")]
		public string ConfirmPassword { get; set; }

	}
}