using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Areas.ControlPanel.Models.Reports.Training
{
    public class TrainingAttendanceItem
    {

        public Guid MemberId { get; set; }

        public string FirstName { get; set; }

        public string Surname { get; set; }

        public string MemberNumber { get; set; }

    }
}