using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Models.MyAccount
{
	public class ForgottenPasswordViewModel
	{

		[Required(ErrorMessage = "Please enter your email address.")]
		[EmailAddress(ErrorMessage = "Please enter a valid email address.")]
		[Display(Name = "Email address")]
		public string Email { get; set; }

	}
}