using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Models.Events
{
	public class CreateEditCrewViewModel
	{

		public Guid EventId { get; set; }

		public Guid? CrewId { get; set; }

		[Required(ErrorMessage = "You must enter a name for the crew")]
		public string Name { get; set; }

		[Required(ErrorMessage = "You must select at least one crew member")]
		public string SelectedMembers { get; set; }

		public Guid CrewLeaderId { get; set; }

		public IList<Tuple<Guid, string, string>> AvailableMembers { get; set; }

		public CreateEditCrewViewModel()
		{
			AvailableMembers = new List<Tuple<Guid, string, string>>();
		}
	}
}