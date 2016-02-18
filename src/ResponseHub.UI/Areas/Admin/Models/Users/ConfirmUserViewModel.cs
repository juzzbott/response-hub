﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Areas.Admin.Models.Users
{
	public class ConfirmUserViewModel
	{

		[Required(ErrorMessage = "Please enter the first name for the user")]
		public string FirstName { get; set; }

		[Required(ErrorMessage = "Please enter the surname for the user")]
		public string Surname { get; set; }

		public string EmailAddress { get; set; }

		public bool UserExists { get; set; }

		public string Role { get; set; }
	}
}