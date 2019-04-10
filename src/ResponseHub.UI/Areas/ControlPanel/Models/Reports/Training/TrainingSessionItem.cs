using Enivate.ResponseHub.Model.Training;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Areas.ControlPanel.Models.Reports.Training
{
	public class TrainingSessionItem
	{

		public Guid Id { get; set; }

		public string Name { get; set; }

		public string Description { get; set; }

		public DateTime SessionDate { get; set; }

		public string Duration { get; set; }

		public string TrainingType { get; set; }

        public string EquipmentUsed { get; set; }

    }
}