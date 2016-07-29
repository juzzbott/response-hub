using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Models.MyAccount
{
	public class UpdateAccountViewModel
	{

		[Required(ErrorMessage = "Please enter the first name for the user")]
		public string FirstName { get; set; }

		[Required(ErrorMessage = "Please enter the surname for the user")]
		public string Surname { get; set; }

	}
}