using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Enivate.ResponseHub.Model.Training;

namespace Enivate.ResponseHub.UI.Areas.ControlPanel.Models.Reports.Training
{
	public class UnitTrainerReportItem
	{

		public string Name { get; set; }

		public IList<TrainerSessionItem> TrainingSessions { get; set; }

		public int TotalSessionsTrained { get; set; }

		public UnitTrainerReportItem()
		{
			TrainingSessions = new List<TrainerSessionItem>();
		}

	}
}