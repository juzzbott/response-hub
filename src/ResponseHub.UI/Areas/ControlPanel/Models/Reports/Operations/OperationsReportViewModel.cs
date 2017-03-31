using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Enivate.ResponseHub.Model.Messages;

namespace Enivate.ResponseHub.UI.Areas.ControlPanel.Models.Reports.Operations
{
	public class OperationsReportViewModel
	{

		public IList<JobMessage> Jobs { get; set; }

		public IList<JobMessage> Messages { get; set; }

		public DateTime StartDate { get; set; }

		public DateTime FinishDate { get; set; }

		public bool UseStandardLayout { get; set; }

		public OperationsReportViewModel()
		{
			Messages = new List<JobMessage>();
			Jobs = new List<JobMessage>();
		}

	}
}