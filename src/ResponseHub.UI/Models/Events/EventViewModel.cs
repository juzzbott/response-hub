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

		public DateTime  StartDate { get; set; }

		public DateTime? FinishedDate { get; set; }

		public Guid UnitId { get; set; }

		public string UnitName { get; set; }

		public IList<Tuple<Guid, string, string>> AvailableMembers { get; set; }

		public IList<CrewViewModel> Crews { get; set; }

		public IList<EventJobViewModel> Jobs { get; set; }

		public IList<EventJobViewModel> UnassignedJobs { get; set; }

		public int UnassignedJobsCount { get; set; }

		public int InProgressJobsCount { get; set; }

		public int CompletedJobsCount { get; set; }

		public bool Finished { get; set; }

		public string DurationString { get; set; }

		public bool AllMembersAllocated { get; set; }

		public string Description { get; set; }

		public EventViewModel()
		{
			AvailableMembers = new List<Tuple<Guid, string, string>>();
			Crews = new List<CrewViewModel>();
			Jobs = new List<EventJobViewModel>();
			UnassignedJobs = new List<EventJobViewModel>();
		}

	}
}