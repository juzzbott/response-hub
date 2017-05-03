using Enivate.ResponseHub.Model.SignIn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Areas.ControlPanel.Models.Reports.Attendance
{
	public class MemberAttendanceReportItem
	{

		public string FullName { get; set; }

		public string MemberNumber { get; set; }

		public DateTime SignInTime { get; set; }

		public SignInType SignInType { get; set; }

		public string Description { get; set; }

	}
}