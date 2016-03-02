using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Enivate.ResponseHub.UI.Areas.Admin.Models.Groups
{
	public class ChangeUserRoleViewModel
	{

		public Guid GroupId { get; set; }

		[Required(ErrorMessage = "Please select a role for the user")]
		public string Role { get; set; }

		public IList<SelectListItem> AvailableRoles { get; set; }

		public ChangeUserRoleViewModel()
		{
			AvailableRoles = new List<SelectListItem>();
		}

	}
}