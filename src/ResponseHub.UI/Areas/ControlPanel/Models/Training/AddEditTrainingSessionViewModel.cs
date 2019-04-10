using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Enivate.ResponseHub.Common.Extensions;
using Enivate.ResponseHub.Model.Training;

namespace Enivate.ResponseHub.UI.Areas.ControlPanel.Models.Training
{
	public class AddEditTrainingSessionViewModel
	{

		[Required(ErrorMessage = "You must enter a date for the training session.")]
		public string SessionDate { get; set; }

		[Required(ErrorMessage = "You must enter a time for the training session.")]
		public string SessionTime { get; set; }

		[Required(ErrorMessage = "You must select a training type.")]
		public string TrainingTypes { get; set; }

		public IList<TrainingType> AvailableTrainingTypes { get; set; }

		[Required(ErrorMessage = "You must enter a name for the session.")]
		public string Name { get; set; }

		public IList<Tuple<Guid, string, string>> AvailableUsers { get; set; }

		[Required(ErrorMessage = "You need to add at least one member to the training session.")]
		public string SelectedMembers { get; set; }

		public string SelectedTrainers { get; set; }

        [Required(ErrorMessage = "You must provide a description for the session.")]
        public string Description { get; set; }

		[Required(ErrorMessage = "You must select a training session type.")]
		public TrainingSessionType SessionType { get; set; }

		[Required(ErrorMessage = "You need to enter the training duration.")]
		[RegularExpression("\\d+(\\.\\d{1,2})?", ErrorMessage = "Please ensure to enter a valid duration, in hours.")]
		[Range(0.25, 999999, ErrorMessage = "Training sessions must be a minimum of 15 minutes.")]
		public decimal Duration { get; set; }

        public string EquipmentUsed { get; set; }

        public AddEditTrainingSessionViewModel()
		{
			AvailableTrainingTypes = new List<TrainingType>();
			AvailableUsers = new List<Tuple<Guid, string, string>>();
		}
	}
}