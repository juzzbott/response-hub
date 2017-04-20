using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Models.Api.Events
{
	public class AddCrewPostModel
	{

		public string Name { get; set; }

		public string SelectedMembers { get; set; }

		public Guid CrewLeaderId { get; set; }

	}
}