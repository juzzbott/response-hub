using Enivate.ResponseHub.Model.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Models.Messages
{
	public class EventJobListViewModel
	{

		public Guid EventId { get; set; }

		public string EventName { get; set; }

		public string EventDescription { get; set; }

		public IList<JobMessageListItemViewModel> Jobs { get; set; }

		public EventJobListViewModel()
		{
			Jobs = new List<JobMessageListItemViewModel>();
		}

	}
}