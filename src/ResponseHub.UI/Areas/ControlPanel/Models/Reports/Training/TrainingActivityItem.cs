using System.Collections.Generic;

using Enivate.ResponseHub.Model.Training;

namespace Enivate.ResponseHub.UI.Areas.ControlPanel.Models.Reports.Training
{
    public class TrainingActivityItem : TrainingSessionItem
    {

        public string SessionType { get; set; }

        public IList<TrainingAttendanceItem> Members { get; set; }

        public IList<TrainingAttendanceItem> Trainers { get; set; }

        public TrainingActivityItem()
        {
            Members = new List<TrainingAttendanceItem>();
            Trainers = new List<TrainingAttendanceItem>();
        }

    }
}