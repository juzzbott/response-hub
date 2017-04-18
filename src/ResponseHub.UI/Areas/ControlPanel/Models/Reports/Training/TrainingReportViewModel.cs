using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Enivate.ResponseHub.Model.Training;

namespace Enivate.ResponseHub.UI.Areas.ControlPanel.Models.Reports.Training
{
	public class TrainingReportViewModel
	{

		public IDictionary<TrainingType, string> TrainingTypes { get; set; }

		public IList<UnitTrainingMemberReportItem> MemberReports { get; set; }

		public string ChartDataJs { get; set; }

		public string ChartOptionsJs { get; set; }

		public DateTime DateFrom { get; set; }

		public DateTime DateTo { get; set; }

		public bool UseStandardLayout { get; set; }

		public TrainingReportViewModel()
		{
			TrainingTypes = new Dictionary<TrainingType, string>();
			MemberReports = new List<UnitTrainingMemberReportItem>();
		}
	}
}