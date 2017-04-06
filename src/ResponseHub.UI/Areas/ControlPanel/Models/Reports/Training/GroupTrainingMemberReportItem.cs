using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Enivate.ResponseHub.Model.Training;

namespace Enivate.ResponseHub.UI.Areas.ControlPanel.Models.Reports.Training
{
	public class GroupTrainingMemberReportItem
	{

		public string Name { get; set; }

		public IDictionary<string, int> TrainingSessions { get; set; }

		public IDictionary<string, IList<DateTime>> TrainingDates { get; set; }

		public int AttendancePercent { get; set; }

		public GroupTrainingMemberReportItem()
		{
			TrainingSessions = new Dictionary<string, int>();
			TrainingDates = new Dictionary<string, IList<DateTime>>();
		}

	}
}