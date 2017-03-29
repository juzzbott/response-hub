using Enivate.ResponseHub.Common.Extensions;
using Enivate.ResponseHub.Model.SignIn;
using Enivate.ResponseHub.Model.Training;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Enivate.ResponseHub.UI.Areas.ControlPanel.Models.Training
{
	public class AddTrainingSessionViewModel
	{

		[Required(ErrorMessage = "You must enter a start date for the export.")]
		[DataType(DataType.Date, ErrorMessage = "Please enter a valid date (dd/mm/yyyy).")]
		public DateTime DateFrom { get; set; }

		[Required(ErrorMessage = "You must select a training type.")]
		public TrainingType TrainingType { get; set; }

		public IList<SelectListItem> AvailableTrainingTypes { get; set; }

		public AddTrainingSessionViewModel()
		{
			AvailableTrainingTypes = new List<SelectListItem>();
			foreach (TrainingType trainingType in Enum.GetValues(typeof(TrainingType)))
			{
				AvailableTrainingTypes.Add(new SelectListItem() { Value = ((int)trainingType).ToString(), Text = trainingType.GetEnumDescription() });
			}
		}
	}
}