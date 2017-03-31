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
		[DataType(DataType.Date, ErrorMessage = "Please enter a valid date (dd/mm/yyyy).")]
		public DateTime SessionDate { get; set; }

		[Required(ErrorMessage = "You must select a training type.")]
		public TrainingType TrainingType { get; set; }

		public IList<SelectListItem> AvailableTrainingTypes { get; set; }

		public IList<Tuple<Guid, string, string>> AvailableUsers { get; set; }

		[Required(ErrorMessage = "You need to add at least one member to the training session.")]
		public string SelectedMembers { get; set; }

		[Required(ErrorMessage = "You need to add at least one trainer to the training session.")]
		public string SelectedTrainers { get; set; }

		public string Description { get; set; }

		[Required(ErrorMessage = "You must select a training session type.")]
		public TrainingSessionType SessionType { get; set; }

		[Required(ErrorMessage = "You need to enter the training duration.")]
		[RegularExpression("\\d+(\\.\\d{1,2})?", ErrorMessage = "Please ensure to enter a valid duration, in hours.")]
		[Range(0.25, 999999, ErrorMessage = "Training sessions must be a minimum of 15 minutes.")]
		public decimal Duration { get; set; }

		public AddEditTrainingSessionViewModel()
		{
			AvailableTrainingTypes = new List<SelectListItem>();
			AvailableTrainingTypes.Add(new SelectListItem() { Text = "Please select...", Value="" });
			foreach (TrainingType trainingType in Enum.GetValues(typeof(TrainingType)))
			{
				AvailableTrainingTypes.Add(new SelectListItem() { Value = ((int)trainingType).ToString(), Text = trainingType.GetEnumDescription() });
			}

			AvailableUsers = new List<Tuple<Guid, string, string>>();
		}
	}
}