using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Areas.ControlPanel.Models.Reports.Attendance
{
	public class AttendanceReportViewModel
	{
		public DateTime DateFrom { get; set; }

		public DateTime DateTo { get; set; }

		public bool UseStandardLayout { get; set; }

		public string ChartDataJs { get; set; }

		public string ChartOptionsJs { get; set; }

		public IDictionary<string, List<MemberAttendanceReportItem>> AttendanceReportItems { get; set; }

		public AttendanceReportViewModel()
		{
			AttendanceReportItems = new Dictionary<string, List<MemberAttendanceReportItem>>();
		}

	}
}