using Enivate.ResponseHub.Model.SignIn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Areas.ControlPanel.Models.Reports.Training
{
	public class TrainingReportViewModel
	{

		public IDictionary<TrainingType, string> TrainingTypes { get; set; }

		public IList<GroupTrainingMemberReportItem> MemberReports { get; set; }

		public string ChartDataJs { get; set; }

		public string ChartOptionsJs { get; set; }

		public DateTime DateFrom { get; set; }

		public DateTime DateTo { get; set; }

		public bool UseStandardLayout { get; set; }

		public TrainingReportViewModel()
		{
			TrainingTypes = new Dictionary<TrainingType, string>();
			MemberReports = new List<GroupTrainingMemberReportItem>();
		}
	}
}