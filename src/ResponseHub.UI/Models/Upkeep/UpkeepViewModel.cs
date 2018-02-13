using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Models.Upkeep
{
	public class UpkeepViewModel
	{

		public IList<UpkeepReportViewModel> OpenReports { get; set; }

		public IList<UpkeepReportViewModel> ClosedReports { get; set; }

		public UpkeepViewModel()
		{
			OpenReports = new List<UpkeepReportViewModel>();
			ClosedReports = new List<UpkeepReportViewModel>();
		}

	}
}