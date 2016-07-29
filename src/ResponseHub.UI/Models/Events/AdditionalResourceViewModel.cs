using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Enivate.ResponseHub.UI.Models.Events
{
	public class AdditionalResourceViewModel
	{

		[Required(ErrorMessage = "Please enter a resource name.")]
		public string Name { get; set; }

		[Required(ErrorMessage = "Please select an agency for the resource.")]
		public Guid AgencyId { get; set; }

		public IList<SelectListItem> AvailableAgencies { get; set; }

		public AdditionalResourceViewModel()
		{
			AvailableAgencies = new List<SelectListItem>();
		}

	}
}