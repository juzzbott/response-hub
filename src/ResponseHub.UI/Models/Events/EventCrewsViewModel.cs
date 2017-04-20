using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Models.Events
{
	public class EventCrewsViewModel
	{

		public IList<CrewViewModel> Crews { get; set; }

		[Required(ErrorMessage = "You must enter a name for the crew")]
		public string CrewName { get; set; }

		[Required(ErrorMessage = "You must select at least one crew member")]
		public string SelectedMembers { get; set; }

		public Guid CrewLeaderId { get; set; }

		public IList<Tuple<Guid, string, string>> AvailableMembers { get; set; }

		public EventCrewsViewModel()
		{
			AvailableMembers = new List<Tuple<Guid, string, string>>();
			Crews = new List<CrewViewModel>();
		}
	}
}