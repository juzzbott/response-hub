using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Areas.ControlPanel.Models.Reports.Training
{
	public class TrainingReportFilterViewModel : ReportFilterViewModel
	{

		public Guid? MemberId { get; set; }

		public IList<Tuple<Guid, string, string>> AvailableMembers { get; set; }

		public TrainingReportFilterViewModel()
		{
			AvailableMembers = new List<Tuple<Guid, string, string>>();
		}
	}
}