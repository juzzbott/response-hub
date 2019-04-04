using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Enivate.ResponseHub.Model.Training;

namespace Enivate.ResponseHub.UI.Areas.ControlPanel.Models.Reports.Training
{
	public class TrainingActivityReportViewModel
    {

		public DateTime DateFrom { get; set; }

		public DateTime DateTo { get; set; }

		public bool UseStandardLayout { get; set; }

        public bool CanvasToImage { get; set; }

        public string UnitName { get; set; }

        public IList<TrainingActivityItem> TrainingActivitySessions { get; set; }

        public IDictionary<TrainingType, string> TrainingTypes { get; set; }

        public TrainingActivityReportViewModel()
		{
            TrainingActivitySessions = new List<TrainingActivityItem>();
            TrainingTypes = new Dictionary<TrainingType, string>();
        }
	}
}