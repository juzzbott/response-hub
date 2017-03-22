using Enivate.ResponseHub.Model.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Areas.ControlPanel.Models.DataExport
{
	public class HtmlDataExportViewModel
	{

		public IList<JobMessage> Jobs { get; set; }

		public IList<JobMessage> Messages { get; set; }

		public DateTime StartDate { get; set; }

		public DateTime FinishDate { get; set; }

		public HtmlDataExportViewModel()
		{
			Messages = new List<JobMessage>();
			Jobs = new List<JobMessage>();
		}

	}
}