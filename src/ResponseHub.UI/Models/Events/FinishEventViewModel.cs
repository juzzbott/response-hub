using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Models.Events
{
	public class FinishEventViewModel
	{

		public Guid EventId { get; set; }

		[Required(ErrorMessage = "Please ensure a finish date has been specified.")]
		public string FinishDate { get; set; }

		[Required(ErrorMessage = "Please ensure a finish time has been specified.")]
		public string FinishTime { get; set; }
	}
}