﻿using System;
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
		public ServiceType Service { get; set; }

		public IList<SelectListItem> AvailableServices { get; set; }

		public string Description { get; set; }

		public CreateGroupModel()
		{
			AvailableServices = new List<SelectListItem>();
			AvailableServices.Add(new SelectListItem() { Text = "Please select" });
			AvailableServices.Add(new SelectListItem() { Value = "1", Text = "State Emergency Service" });
			AvailableServices.Add(new SelectListItem() { Value = "2", Text = "Country Fire Authority" });

		}

	}
}