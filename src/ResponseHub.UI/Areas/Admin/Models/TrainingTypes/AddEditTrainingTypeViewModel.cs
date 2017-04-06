using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Areas.Admin.Models.TrainingTypes
{
	public class AddEditTrainingTypeViewModel
	{

		public bool EditingMode { get; set; }

		[Required(ErrorMessage = "You must enter a name for the training type.")]
		public string Name { get; set; }

		[Required(ErrorMessage = "You must enter a short name for the training type.")]
		public string ShortName { get; set; }

		public string Description { get; set; }

	}

}