using Enivate.ResponseHub.Model.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Models.Events
{
	public class EventViewModel
	{

		public string Name { get; set; }

		public DateTime  EventStarted { get; set; }

		public DateTime EventFinished { get; set; }

		public Guid GroupId { get; set; }

		public string GroupName { get; set; }

		public IList<EventResource> GroupResources { get; set; }
		
		public IList<EventResource> AdditionalResources { get; set; }

		public AdditionalResourceViewModel AdditionalResourceModel { get; set; }

		public EventViewModel()
		{
			GroupResources = new List<EventResource>();
			AdditionalResources = new List<EventResource>();
			AdditionalResourceModel = new AdditionalResourceViewModel();
		}

	}
}