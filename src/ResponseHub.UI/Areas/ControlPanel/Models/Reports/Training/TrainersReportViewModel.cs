using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Enivate.ResponseHub.Model.Training;

namespace Enivate.ResponseHub.UI.Areas.ControlPanel.Models.Reports.Training
{
	public class TrainersReportViewModel
	{

		public IDictionary<TrainingType, string> TrainingTypes { get; set; }

		public IList<UnitTrainerReportItem> TrainerReports { get; set; }

		public DateTime DateFrom { get; set; }

		public DateTime DateTo { get; set; }

		public bool UseStandardLayout { get; set; }

		public TrainersReportViewModel()
		{
			TrainingTypes = new Dictionary<TrainingType, string>();
			TrainerReports = new List<UnitTrainerReportItem>();
		}
	}
}