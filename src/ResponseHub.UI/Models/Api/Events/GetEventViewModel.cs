using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Enivate.ResponseHub.UI.Models.Events;
using Enivate.ResponseHub.Model.Events;
using System.Threading.Tasks;
using Enivate.ResponseHub.Model.Messages;
using Enivate.ResponseHub.Model.Messages.Interface;

namespace Enivate.ResponseHub.UI.Models.Api.Events
{
	public class GetEventViewModel
	{
		public Guid Id { get; set; }

		public string Name { get; set; }

		public DateTime StartDate { get; set; }

		public DateTime? FinishedDate { get; set; }

		public IList<EventJobViewModel> Jobs { get; set; }

		public int UnassignedJobsCount { get; set; }

		public int InProgressJobsCount { get; set; }

		public int CompletedJobsCount { get; set; }

		public bool Finished { get; set; }

		public string DurationString { get; set; }

		public string Description { get; set; }

		public GetEventViewModel()
		{
			Jobs = new List<EventJobViewModel>();
		}

	}
}