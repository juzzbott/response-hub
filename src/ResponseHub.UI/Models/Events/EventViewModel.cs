using Enivate.ResponseHub.Model.Events;
using Enivate.ResponseHub.UI.Models.Messages;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Models.Events
{
	public class EventViewModel
	{

		public Guid Id { get; set; }

		public string Name { get; set; }

		public DateTime  EventStarted { get; set; }

		public DateTime? EventFinished { get; set; }

		public Guid UnitId { get; set; }

		public string UnitName { get; set; }

		public IList<Tuple<Guid, string, string>> AvailableMembers { get; set; }

		public EventCrewsViewModel EventCrewsModel { get; set; }

		public IList<EventJobViewModel> Jobs { get; set; }

		public IList<EventJobViewModel> UnassignedJobs { get; set; }

		public int UnassignedJobsCount { get; set; }

		public int InProgressJobsCount { get; set; }

		public int CompletedJobsCount { get; set; }

		public bool Finished { get; set; }

		public string DurationString { get; set; }

		public EventViewModel()
		{
			AvailableMembers = new List<Tuple<Guid, string, string>>();
			EventCrewsModel = new EventCrewsViewModel();
			Jobs = new List<EventJobViewModel>();
		}

	}
}