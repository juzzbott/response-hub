using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Models.MyAccount
{
	public class ChangePasswordViewModel
	{

		[Required(ErrorMessage = "Please enter your current account password")]
		[Display(Name = "Account password")]
		public string AccountPassword { get; set; }

		[Required(ErrorMessage = "Please enter a new account password")]
		[Display(Name = "New Password")]
		public string NewPassword { get; set; }

		[Required(ErrorMessage = "Please confirm your account password.")]
		[Compare("NewPassword", ErrorMessage = "Your confirmed password does not match.")]
		[Display(Name = "Confirm password")]
		public string ConfirmPassword { get; set; }

	}
}