using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Enivate.ResponseHub.Common.Extensions;
using Enivate.ResponseHub.Model.SignIn;
using Enivate.ResponseHub.Model.Training;
using Enivate.ResponseHub.UI.Validators;

namespace Enivate.ResponseHub.UI.Models.SignIn
{
	public class SignInViewModel
	{

		[Required(ErrorMessage = "Please ensure you have selected a date.")]
		public string StartDate { get; set; }

		[Required(ErrorMessage = "Please ensure you have a selected a time.")]
		public string StartTime { get; set; }

		[Required(ErrorMessage = "You must select a sign on type.")]
		public SignInType SignInType { get; set; }

		[SignInTypeDescription("SignInType", SignInType.Operation, ErrorMessage = "You must enter a description for the operation.")]
		public string OperationDescription { get; set; }

		public Guid? OperationJobId { get; set; }

		public IList<SignInOperationItem> AvailableOperations { get; set; }

		[SignInTypeDescription("SignInType", SignInType.Training, ErrorMessage = "You must enter a description for the training session.")]
		public string TrainingDescription { get; set; }

		public IList<SelectListItem> AvailableOtherTypes { get; set; }

		[SignInTypeDescription("SignInType", SignInType.Other, ErrorMessage = "You must select a type of sign on from the list.")]
		public OtherSignInType SignInTypeOther { get; set; }

		public string OtherTypeDescription { get; set; }

		public IList<SelectListItem> AvailableUsers { get; set; }

		[Required(ErrorMessage = "Please ensure you have selected a unit to sign in for.")]
		public Guid UnitId { get; set; }

		[Required(ErrorMessage = "Please select a user to sign in.")]
		public Guid UserId { get; set; }

		public bool SignOutRequired { get; set; }

		public SignInViewModel()
		{
			AvailableOperations = new List<SignInOperationItem>();
			AvailableUsers = new List<SelectListItem>();
			AvailableOtherTypes = new List<SelectListItem>();

			foreach (OtherSignInType otherType in Enum.GetValues(typeof(OtherSignInType)))
			{
				AvailableOtherTypes.Add(new SelectListItem() { Value = ((int)otherType).ToString(), Text = otherType.GetEnumDescription() });
			}

		}

	}
}