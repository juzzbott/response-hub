using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Enivate.ResponseHub.Model;

namespace Enivate.ResponseHub.UI.Areas.Admin.Models.Groups
{
	public class CreateGroupModel
	{

		[Required(ErrorMessage = "You must enter a group name.")]
		public string Name { get; set; }

		[Required(ErrorMessage = "You must select a service the group belongs to.", AllowEmptyStrings = false)]
		public string Service { get; set; }
		
		[Required(ErrorMessage = "You must enter a capcode / pager address for this group.")]
		public string Capcode { get; set; }

		public IList<SelectListItem> AvailableServices { get; set; }

		public string Description { get; set; }

		[Required(ErrorMessage = "You must enter a group administrator.")]
		[EmailAddress(ErrorMessage = "You must enter a valid email address.")]
		public string GroupAdministratorEmail { get; set; }

		public GroupAdministratorViewModel GroupAdministrator { get; set; }

		public CreateGroupModel()
		{
			AvailableServices = new List<SelectListItem>();
			AvailableServices.Add(new SelectListItem() { Value = "", Text = "Please select" });
			AvailableServices.Add(new SelectListItem() { Value = "1", Text = "State Emergency Service" });
			AvailableServices.Add(new SelectListItem() { Value = "2", Text = "Country Fire Authority" });

			GroupAdministrator = new GroupAdministratorViewModel();

		}

	}
}