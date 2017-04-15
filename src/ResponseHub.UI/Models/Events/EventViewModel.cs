using Enivate.ResponseHub.Model.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Models.Events
{
	public class EventViewModel
	{

		public Guid Id { get; set; }

		public string Name { get; set; }

		public DateTime  EventStarted { get; set; }

		public DateTime EventFinished { get; set; }

		public Guid UnitId { get; set; }

		public string UnitName { get; set; }

		public IList<EventResource> UnitResources { get; set; }
		
		public IList<EventResource> AdditionalResources { get; set; }

		public AdditionalResourceViewModel AdditionalResourceModel { get; set; }

		public EventViewModel()
		{
			UnitResources = new List<EventResource>();
			AdditionalResources = new List<EventResource>();
			AdditionalResourceModel = new AdditionalResourceViewModel();
		}

	}
}