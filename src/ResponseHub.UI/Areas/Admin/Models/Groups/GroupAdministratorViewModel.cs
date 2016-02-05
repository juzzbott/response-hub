using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Areas.Admin.Models.Groups
{
	public class GroupAdministratorViewModel
	{

		public Guid UserId { get; set; }

		[Required(ErrorMessage = "Please enter the group administrator first name")]
		public string FirstName { get; set; }

		[Required(ErrorMessage = "Please enter the group administrator surname")]
		public string Surname { get; set; }

		public string EmailAddress { get; set; }

	}
}