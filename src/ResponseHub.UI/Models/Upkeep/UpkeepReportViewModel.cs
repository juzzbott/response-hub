using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Enivate.ResponseHub.UI.Models.Upkeep
{
	public class UpkeepReportViewModel
	{

		[Required(ErrorMessage = "You must enter a name for the report.")]
		public string Name { get; set; }

		public DateTime Created { get; set; }

		public DateTime? Submitted { get; set; }

		public IList<SelectListItem> AvailableTasks { get; set; }

		[Required(ErrorMessage = "You need to add at least one task to complete for the report.")]
		public string SelectedTasks { get; set; }

		public UpkeepReportViewModel()
		{
			AvailableTasks = new List<SelectListItem>();
		}

	}
}