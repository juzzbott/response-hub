using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Enivate.ResponseHub.Common.Extensions;
using Enivate.ResponseHub.Model.SignIn;
using Enivate.ResponseHub.Model.Training;

namespace Enivate.ResponseHub.UI.Models.SignIn
{
	public class SignInViewModel
	{

		[Required(ErrorMessage = "Please ensure you have selected a date.")]
		public string StartDate { get; set; }

		[Required(ErrorMessage = "Please ensure you have a selected a time.")]
		public string StartTime { get; set; }

		[Required(ErrorMessage = "You must select a sign on type.")]
		public SignInType SignOnType { get; set; }

		public string OperationDescription { get; set; }

		public Guid? OperationJobId { get; set; }

		public IList<SignInOperationItem> AvailableOperations { get; set; }

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

		}

	}
}