using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Enivate.ResponseHub.UI.Areas.ControlPanel.Models.DataExport
{
	public class DataExportFilterViewModel
	{

		public IList<SelectListItem> AvailableGroups { get; set; }

		[Required(ErrorMessage = "You must select a group to export data from.", AllowEmptyStrings = false)]
		public Guid GroupId { get; set; }

		[Required(ErrorMessage = "You must enter a start date for the export.")]
		[DataType(DataType.Date, ErrorMessage = "Please enter a valid date (dd/mm/yyyy).")]
		public DateTime DateFrom { get; set; }

		[Required(ErrorMessage = "You must enter a finish date for the export.")]
		[DataType(DataType.Date, ErrorMessage = "Please enter a valid date (dd/mm/yyyy).")]
		public DateTime DateTo { get; set; }

		public string ExportType { get; set; }

		public DataExportFilterViewModel()
		{
			AvailableGroups = new List<SelectListItem>();
		}

	}
}