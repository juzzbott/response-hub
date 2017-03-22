using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

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

		public SignOnViewModel()
		{
			AvailableOperations = new List<Tuple<Guid, string, string>>();
		}

	}
}