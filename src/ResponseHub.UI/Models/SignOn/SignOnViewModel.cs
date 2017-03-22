﻿using Enivate.ResponseHub.Common.Extensions;
using Enivate.ResponseHub.Model.SignOn;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Enivate.ResponseHub.UI.Models.SignOn
{
	public class SignOnViewModel
	{

		[Required(ErrorMessage = "Please ensure you have selected a date.")]
		public string StartDate { get; set; }

		[Required(ErrorMessage = "Please ensure a selected a time.")]
		public string StartTime { get; set; }

		[Required(ErrorMessage = "You must select a sign on type.")]
		public int SignOnType { get; set; }

		public string OperationDescription { get; set; }

		public Guid? OperationJobId { get; set; }

		public IList<Tuple<Guid, string, string>> AvailableOperations { get; set; }

		public TrainingType TrainingType { get; set; }

		public IList<SelectListItem> AvailableTrainingTypes { get; set; }

		public SignOnViewModel()
		{
			AvailableOperations = new List<Tuple<Guid, string, string>>();
			AvailableTrainingTypes = new List<SelectListItem>();

			// Set the available training types from the enum list
			foreach (TrainingType trainingType in Enum.GetValues(typeof(TrainingType)))
			{
				AvailableTrainingTypes.Add(new SelectListItem() { Value = ((int)trainingType).ToString(), Text = trainingType.GetEnumDescription() });
			}

		}

	}
}