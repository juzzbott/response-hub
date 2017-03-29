using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Areas.ControlPanel.Models.Reports
{
	public class ReportFilterViewModel
	{

		[Required(ErrorMessage = "You must enter a start date for the report.")]
		[DataType(DataType.Date, ErrorMessage = "Please enter a valid date (dd/mm/yyyy).")]
		public DateTime DateFrom { get; set; }

		[Required(ErrorMessage = "You must enter a finish date for the report.")]
		[DataType(DataType.Date, ErrorMessage = "Please enter a valid date (dd/mm/yyyy).")]
		public DateTime DateTo { get; set; }

		public string ReportFormat { get; set; }
	}
}