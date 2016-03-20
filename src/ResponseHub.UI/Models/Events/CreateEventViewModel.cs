using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Enivate.ResponseHub.UI.Models.Events
{
	public class CreateEventViewModel
	{

		[Required(ErrorMessage = "Please enter a name for the event.")]
		public string Name { get; set; }

		[Required(ErrorMessage = "Please select the group managing the event.")]
		public Guid GroupId { get; set; }

		[Required(ErrorMessage = "Please ensure a start date has been specified.")]
		public string StartDate { get; set; }

		[Required(ErrorMessage = "Please ensure a start time has been specified.")]
		public string StartTime { get; set; }

		public IList<SelectListItem> AvailableGroups { get; set; }

		public CreateEventViewModel()
		{
			AvailableGroups = new List<SelectListItem>();
		} 

	}
}