using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Models.Api.Messages
{
	public class GetDistanceFromJobModel
	{

		public string Error { get; set; }

		public bool Success { get; set; }

		public double Distance { get; set; }

		public string ReferencedJobNumber { get; set; }

		public Guid ReferencedJobId { get; set; }

	}
}