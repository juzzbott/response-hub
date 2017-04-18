using Enivate.ResponseHub.Model.Training;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Areas.ControlPanel.Models.Training
{
	public class TrainingHomeViewModel
	{

		public string TrainingOverviewChartData { get; set; }

		public IList<TrainingSession> TrainingSessions { get; set; }

		public TrainingHomeViewModel()
		{
			TrainingSessions = new List<TrainingSession>();
		}

	}
}