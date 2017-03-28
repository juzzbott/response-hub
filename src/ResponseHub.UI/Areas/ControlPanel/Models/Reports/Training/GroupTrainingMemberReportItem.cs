using Enivate.ResponseHub.Model.SignIn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Areas.ControlPanel.Models.Reports.Training
{
	public class GroupTrainingMemberReportItem
	{

		public string Name { get; set; }

		public IDictionary<TrainingType, int> TrainingSessions { get; set; }

		public IDictionary<TrainingType, IList<DateTime>> TrainingDates { get; set; }

		public GroupTrainingMemberReportItem()
		{
			TrainingSessions = new Dictionary<TrainingType, int>();
			TrainingDates = new Dictionary<TrainingType, IList<DateTime>>();
		}

	}
}